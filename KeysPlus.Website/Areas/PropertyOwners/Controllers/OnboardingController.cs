using KeysPlus.Data;
using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KeysPlus.Website.Models;
using System.Globalization;
using System.Collections.Specialized;
using System.Net.Mail;
using System.Threading.Tasks;

namespace KeysPlus.Website.Areas.PropertyOwners.Controllers
{
    [Authorize]
    public class OnboardingController : Controller
    {
        KeysEntities db = new KeysEntities();
        decimal actualTotalRepayment = 0;

        // GET: PropertyOwners/Onboarding
        public ActionResult Index()
        {

            var freqs = PropertyOwnerService.GetAllPaymentFrequencies();
            var propertyTypes = PropertyService.GetAllProprtyTypes();
            var propertyHomeValueTypes = PropertyService.GetAllProprtyHomeValueTypes();
            ViewBag.Frequencies = freqs;
            ViewBag.PropertyTypes = propertyTypes;
            ViewBag.PropertyHomeValueTypes = propertyHomeValueTypes;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddProperty(PropertyMyOnboardModel model)
        {
            var status = true;
            var message = "Record added successfully";
            var data = model;
            AddTenantToPropertyModel tenant = new AddTenantToPropertyModel();
          
            //*********** AddNewProperty
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var newProp = PropertyOwnerService.AddOnboardProperty(login, model);
            var address = model.Address.ToAddressString();
            var ownerPerson = AccountService.GetPersonByLoginId(login.Id);
            ////*********** AddRepayments

            var newRepayment = new PropertyRepayment();

            if (newProp == null)
            {
                return Json(new { Success = false, message = "Cannot find the property!" });
            }
            else
            {

                newRepayment = PropertyOwnerService.AddOnboardRepayment(login, model.Repayments, newProp.Id);
                decimal _totalRepayment = 0;

                int _nosWeeks = 0;
                int _nosFortnights = 0;
                int _nosMonthly = 0;
                if (newRepayment != null)
                {
                    foreach (Service.Models.RepaymentViewModel repayment in model.Repayments)
                    {

                        switch (repayment.FrequencyType)
                        {
                            case 1: // Weekly
                                    // find the nos of weeks in datediff(StartDate, EndDate)
                                _nosWeeks = ((newRepayment.EndDate - newRepayment.StartDate) ?? TimeSpan.Zero).Days / 7;
                                // _totalAmount = nos weeks * amount
                                _totalRepayment = _nosWeeks * newRepayment.Amount;
                                break;
                            case 2:   // Fortnightly
                                      // find the nos of Fortnights in datediff(StartDate, EndDate)
                                _nosFortnights = ((newRepayment.EndDate - newRepayment.StartDate) ?? TimeSpan.Zero).Days / 14;
                                // _totalAmount = nos weeks * amount
                                _totalRepayment = _nosFortnights * newRepayment.Amount;
                                break;
                            case 3: //Monthly
                                    // find the nos of Monthls in datediff(StartDate, EndDate)
                                _nosMonthly = ((newRepayment.EndDate - newRepayment.StartDate) ?? TimeSpan.Zero).Days / 30;
                                _totalRepayment = _nosMonthly * newRepayment.Amount;
                                // _totalAmount = nos Monthls * amount
                                break;
                        }

                        actualTotalRepayment += _totalRepayment;

                    }
                }

                ////*****AddExpenses
                //var newExpense = new PropertyExpense();
                //newExpense = PropertyOwnerService.AddOnboardExpense(login, model.Expenses, newProp.Id);
                //******AddFinancial
                var newFinancial = new PropertyFinance();
                newFinancial = PropertyOwnerService.AddOnboardFinance(login, model, newProp.Id, actualTotalRepayment);

                //return Json( new { Success = true , PropertyId = newProp.Id});
                if (!model.IsOwnerOccupied)
                {
                    var ten = AccountService.GetExistingLogin(model.TenantToPropertyModel.TenantEmail);
                    if (ten == null)
                    {
                        var sendEmail = false;
                        string temPass = null;
                        /// CREATE AN ACCOUNT AND SEND EMAIL TO TENANT TO ACTIVATE AND RESET ACCOUNT
                        temPass = UtilService.GeneraterRandomKey(8);
                        var createRes = AccountService.CreateTenantAccount(model.TenantToPropertyModel, login, temPass);
                        if (createRes.IsSuccess)
                        {
                            ten = createRes.NewObject as Login;
                            sendEmail = true;
                        }
                        if (sendEmail && temPass != null)
                        {
                            var per = AccountService.GetPersonByLoginId(ten.Id);
                            await EmailService.SendCreateAccountToTenantSendgrid(per, model.TenantToPropertyModel.TenantEmail, temPass, ownerPerson, ten.EmailConfirmationToken, address);
                        }
                    }
                    else
                    {
                        if (!ten.IsActive)
                        {
                            var resultTenantActive = PropertyService.ActivateTenant(login, ten.Id);
                            if (resultTenantActive.IsSuccess)
                            {
                                // SEND EMAIL INTIMATING THAT ACCOUNT HAS BEEN ACTIVATED BY THE OWNER 
                                //await SendActivationEmailToTenant(model);

                            }
                            else
                            {
                                return Json(new { Sucess = false, redirect = "Redirect", url = Url.Action("Index", "PropertyOwners") });
                            }
                            
                        }
                    }
                    var person = AccountService.GetPersonByLoginId(ten.Id);
                    var result = PropertyService.AddTenantToProperty(login, person.Id, newProp.Id, model.TenantToPropertyModel.StartDate,
                        model.TenantToPropertyModel.EndDate, model.TenantToPropertyModel.PaymentFrequencyId, model.TenantToPropertyModel.PaymentAmount);
                    if (result.IsSuccess)
                    {
                        string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "Tenant/Home/MyRentals");
                        SendGridEmailModel mail = new SendGridEmailModel
                        {
                            RecipentName = model.TenantToPropertyModel.FirstName,
                            ButtonText = "",
                            ButtonUrl = url,
                            RecipentEmail = model.TenantToPropertyModel.TenantEmail,
                            OwnerName = ownerPerson.FirstName,
                            Address = address,
                        };
                        await EmailService.SendEmailWithSendGrid(EmailType.OwnerAddTenantEmail, mail);
                        return Json(new { Sucess = true, Msg = "Added!", result = "Redirect", url = Url.Action("Index", "PropertyOwners") });

                    }
                    else
                    {
                        return Json(new { Sucess = false, Msg = result.ErrorMessage, redirect = "Redirect", url = Url.Action("Index", "PropertyOwners") });
                    }
                    
                }
            }
            
            return Json(new { success = status, message = message, data = tenant });

        }
        
        public async Task<ActionResult> SendActivationEmailToTenant(PropertyMyOnboardModel model)
        {
            var user = User.Identity.Name;
            if (String.IsNullOrEmpty(user))
            {
                return Json(new { Success = false, ErrorMsg = "Invalid user!" });
            }
            var owner = AccountService.GetLoginByEmail(user);
            if (owner == null)
            {
                return Json(new { Success = false, ErrorMsg = "Can not find current user!" });
            }
            var ownerPerson = AccountService.GetPersonByLoginId(owner.Id);
            var nvc = new NameValueCollection();
            nvc.Set("TenantEmail", model.TenantToPropertyModel.TenantEmail);
            string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "Account/RegisterToBeTenant", UtilService.ToQueryString(nvc));
            string subject = "Property Community: Account Activated";
            string body =
               "Hello !<br />"
               + $"{ownerPerson.FirstName} has added you to be a tenant in his/her property and activated your account on Property Community.<br />";

            MailMessage msg = new MailMessage()
            {
                Subject = subject,
                SubjectEncoding = System.Text.Encoding.UTF8,
                Body = body,
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true
            };
            msg.To.Add(model.TenantToPropertyModel.TenantEmail);
            try
            {
                await EmailService.SendAsync(msg);
                return Json(new { Success = true, Status = "Await response" });
            }
            catch (Exception ex)
            {
                return Json(new { Sucess = false, msg = ex.ToString() });
            }
        }

        public async Task<ActionResult> SendInvitationEmailToTenant(AddTenantToPropertyModel model)
        {
            var user = User.Identity.Name;
            if (String.IsNullOrEmpty(user))
            {
                return Json(new { Success = false, ErrorMsg = "Invalid user!" });
            }
            var owner = AccountService.GetLoginByEmail(user);
            if (owner == null)
            {
                return Json(new { Success = false, ErrorMsg = "Can not find current user!" });
            }
            var ownerPerson = AccountService.GetPersonByLoginId(owner.Id);
            var property = PropertyService.GetPropertyById(model.PropertyId);
            if (property == null)
            {
                return Json(new { Success = false, ErrorMsg = "Can not find property!" });
            }
            var nvc = new NameValueCollection();
            nvc.Set("TenantEmail", model.TenantEmail);
            nvc.Set("PropertyId", model.PropertyId.ToString());
            nvc.Set("StartDate", model.StartDate.ToString());
            nvc.Set("EndDate", model.EndDate.ToString());
            nvc.Set("PaymentFrequencyId", model.PaymentFrequencyId.ToString());
            nvc.Set("PaymentAmount", model.PaymentAmount.ToString());
            string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "Account/RegisterToBeTenant", UtilService.ToQueryString(nvc));
            string subject = "Property Community: Invitation to register";
            string body =
               "Hello !<br />"
               + $"{ownerPerson.FirstName} has added you to be a tenant in his/her property and invited you to register at Property Community.<br />"
               + $"name has added you to be a tenant in his/her property and invited you to register at Property Community.<br />"

               + "Please <a target='_blank' href=" + url + "> Click Here </a> to register<br />";
            MailMessage msg = new MailMessage()
            {
                Subject = subject,
                SubjectEncoding = System.Text.Encoding.UTF8,
                Body = body,
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true
            };
            msg.To.Add(model.TenantEmail);
            try
            {
                await EmailService.SendAsync(msg);
                return Json(new { Success = true, Status = "Await response" });
            }
            catch (Exception ex)
            {
                return Json(new { Sucess = false, msg = ex.ToString() });
            }
        }

        protected override void Dispose(bool disposing)
        {
            db?.Dispose();
            base.Dispose(disposing);
        }
    }
}