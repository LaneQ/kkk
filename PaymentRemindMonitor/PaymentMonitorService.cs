using KeysPlus.Data;
using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentRemindMonitor
{
    public partial class PaymentMonitorService : ServiceBase
    {
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Task payMonitorTask = null;
        private Task messMonitorTask = null;
        //private readonly KeysEntities db = new KeysEntities();
        public PaymentMonitorService()
        {
            InitializeComponent();
            
        }

        protected override void OnStart(string[] args)
        {
            payMonitorTask = new Task(Monitor, cts.Token, TaskCreationOptions.LongRunning);
            messMonitorTask = new Task(async () => await MessageMonitor(), cts.Token, TaskCreationOptions.LongRunning);
            payMonitorTask.Start();
            messMonitorTask.Start();
        }

        protected override void OnStop()
        {
            cts.Cancel();
            payMonitorTask.Wait();
            messMonitorTask.Wait();
        }

        private async Task MessageMonitor()
        {
            CancellationToken cancellation = cts.Token;
            TimeSpan interval = TimeSpan.Zero;
            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                using (var db = new KeysEntities())
                {
                    try
                    {
                        var items = db.OutGoingMessage.Where(x => x.IsActive && (!x.IsProcessed || x.Error != null));
                        foreach (var m in items)
                        {
                            var per = db.Person.FirstOrDefault(x => x.Id == m.PersonId);
                            var login = per.Login;
                            var nvc = new NameValueCollection();
                            
                            string ownerUrl = "http://new-keys.azurewebsites.net/PropertyOwners/Manage/RentalPaymentTracking";
                            string tenantUrl = "http://new-keys.azurewebsites.net/Teanants/Home/MyRentals";
                            var mailModel = new SendGridEmailModel
                            {
                                RecipentName = per.FirstName,
                                RecipentEmail = login.UserName,
                                Address = m.Subject,
                            };
                            HttpStatusCode res;
                            switch (m.MessageTypeId)
                            {
                                case 1:
                                    mailModel.ButtonUrl = tenantUrl;
                                    mailModel.Date = m.Message;
                                    res = await EmailService.SendEmailWithSendGrid(EmailType.TenantPaymentReminder, mailModel);

                                    if (res != HttpStatusCode.Accepted)
                                    {
                                        m.Error = res.ToString();
                                    }
                                    else m.Error = null;
                                    break;
                                case 2:
                                    mailModel.ButtonUrl = ownerUrl;
                                    var tokens = m.Message.Split(new char[0]).ToList();
                                    var date = tokens.ElementAt(tokens.Count - 1);
                                    tokens.RemoveAt(tokens.Count - 1);
                                    var tenantName = String.Join(" ", tokens);
                                    mailModel.TenantName = tenantName;
                                    mailModel.Date = date;
                                    res = await EmailService.SendEmailWithSendGrid(EmailType.OwnerUpcomingRentalPayment, mailModel);
                                    if (res != HttpStatusCode.Accepted)
                                    {
                                        m.Error = res.ToString();
                                    }
                                    else m.Error = null;
                                    break;
                            }
                            m.IsProcessed = true;
                        }
                        db.SaveChanges();
                        if (cancellation.IsCancellationRequested)
                        {
                            break;
                        }
                        interval = new TimeSpan(0, 0, 10);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception.
                        Debug.WriteLine(ex.StackTrace);
                        interval = new TimeSpan(0, 0, 10);
                    }
                }
                    
            }
        }

        private void Monitor()
        {
            CancellationToken cancellation = cts.Token;
            TimeSpan interval = TimeSpan.Zero;
            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                using (var db = new  KeysEntities())
                {
                    try
                    {
                        var tps = db.TenantProperty.Where(x => x.Id == 1582 /*&& (x.IsActive ?? false) && x.Property.IsActive*/);
                        foreach (var tp in tps)
                        {
                            var paymentDue = TenantService.GetNextPaymentDate(tp.PaymentStartDate, tp.PaymentDueDate, tp.PaymentFrequencyId);
                            if (paymentDue.HasValue)
                            {
                                var today = DateTime.UtcNow.ToLocalTime();
                                var dayDiff = paymentDue.Value - today;
                                var payFreq = tp.PaymentFrequencyId == 1 ? 7 : tp.PaymentFrequencyId == 2 ? 14 : 30;
                                if (dayDiff.TotalDays >= 0 /*&& dayDiff.TotalDays < 2*/)
                                {
                                    var lastQueueDate = tp.LastQueuedPaymentRemind;
                                    double days = 0;
                                    if (lastQueueDate.HasValue)
                                    {
                                        days = Math.Abs((paymentDue.Value - lastQueueDate.Value).TotalDays);
                                    }
                                    if (lastQueueDate == null || days >= payFreq)
                                    {
                                        var addr = tp.Property.Address.ToAddressString();
                                        var newQueueItem = new OutGoingMessage
                                        {
                                            PersonId = tp.TenantId,
                                            MessageTypeId = 1,
                                            DeliveryTypeId = 1,
                                            TargetAddress = tp.Tenant.Person.Login.Email,
                                            Subject = addr,
                                            Message = paymentDue.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                            DateCreated = DateTime.UtcNow,
                                            IsProcessed = false,
                                            IsActive = true,
                                        };
                                        db.OutGoingMessage.Add(newQueueItem);
                                        var owners = tp.Property.OwnerProperty.Select(x => x.Person).Select(x => new { Id = x.Id, Email = x.Login.UserName }); ;
                                        foreach (var o in owners)
                                        {
                                            var tenantName = tp.Tenant.Person.FirstName;
                                            var dateStr = paymentDue.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                                            newQueueItem = new OutGoingMessage
                                            {
                                                PersonId = o.Id,
                                                MessageTypeId = 2,
                                                DeliveryTypeId = 1,
                                                TargetAddress = o.Email,
                                                Subject = addr,
                                                Message = tenantName + " " + dateStr,
                                                DateCreated = DateTime.UtcNow,
                                                IsProcessed = false,
                                                IsActive = true,
                                            };
                                            db.OutGoingMessage.Add(newQueueItem);
                                        }
                                        tp.LastQueuedPaymentRemind = DateTime.UtcNow;
                                        var newPaymentTracking = new RentalPaymentTracking
                                        {
                                            TenantPropertyId = tp.Id,
                                            DueDate = paymentDue.Value,
                                            PaymentComplete = false
                                        };
                                        db.RentalPaymentTracking.Add(newPaymentTracking);
                                    }

                                }

                            }
                        }
                        db.SaveChanges();
                        if (cancellation.IsCancellationRequested)
                        {
                            break;
                        }
                        interval = new TimeSpan(0, 0, 10);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception.
                        interval = new TimeSpan(0, 0, 5);
                    }
                }
                
            }
        }
    }
}
