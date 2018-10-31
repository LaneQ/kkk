using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Threading;
using KeysPlus.Service.Models;
using System.Collections.Specialized;
using KeysPlus.Data;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Web.Script.Serialization;

namespace KeysPlus.Service.Services
{
    public class EmailService
    {
        private static string EmailID = ConfigurationManager.AppSettings["EmailServer"];
        private static string Password = ConfigurationManager.AppSettings["Password"];
        private static string Host = "smtp.gmail.com";
        private static int Port = 587;
        private static bool EnableSsl = true;
        private static string SendGridApiKey = ConfigurationManager.AppSettings["SendGridApiKey"];
        private static string SendGridActivationTemplateId = ConfigurationManager.AppSettings["ActivationEmail"];

        private static SmtpClient Client = new SmtpClient()
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(EmailID, Password),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = EnableSsl,
            Host = Host,
            Port = Port,
        };
        public static string GetSendGridTemplateId(EmailType emailType)
        {
            var s = emailType.ToString();
            return ConfigurationManager.AppSettings[emailType.ToString()];
        }
        public static Task SendAsync(MailMessage message)
        {
            message.From = new MailAddress(EmailID);
            var mailSendingThread = new Thread(() =>
            {
                try
                {
                    Client.Send(message);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
            mailSendingThread.Start();
            return Task.FromResult(0);
        }

        public  static async Task<ServiceResponseResult> SendActivationEmailToTenant(PropertyMyOnboardModel model , Person ownerPerson)
        {
           
            var nvc = new NameValueCollection();
            nvc.Set("TenantEmail", model.TenantToPropertyModel.TenantEmail);
            string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "Account/Login", UtilService.ToQueryString(nvc));
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
                await SendAsync(msg);
                return new ServiceResponseResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new ServiceResponseResult { IsSuccess = false };
            }
        }

        public static async Task<ServiceResponseResult> SendCreateAccountToTenant(PropertyMyOnboardModel model, string temPass, Person ownerPerson)
        {
          
            var nvc = new NameValueCollection();
            string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "Account/Login");
            string subject = "Property Community: Acount Created By Landlord";
            string body =
               "Hello !<br />"
               + $"{ownerPerson.FirstName} has added you to be a tenant in his/her property at Property Community.<br />"
               + "Your account details are as follow :<br />"
               + $"User name : {model.TenantToPropertyModel.TenantEmail}<br />"
               + $"Password : {temPass}<br />"
               + "Please <a target='_blank' href=" + url + "> Click Here </a> sign in.<br />";
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
                await SendAsync(msg);
                return new ServiceResponseResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new ServiceResponseResult { IsSuccess = false };
            }          
        }

        public static async Task<HttpStatusCode> SendEmailSendGrid(SendGridEmailModel mailModel)
        {
            var client = new SendGridClient(SendGridApiKey);
            var personlized =new[]
            {
                new {
                    Subject = mailModel.Subject,
                    To = new List<Recipient>
                    {
                        new Recipient
                        {
                            Email = mailModel.RecipentEmail,
                        }
                    },
                    Substitutions = new Dictionary<string, string>
                    {
                        {"%first_name%",mailModel.RecipentName},
                        {"%owner%",mailModel.OwnerName},
                        {"%body%", mailModel.Body },
                        {"%button_text%" , mailModel.ButtonText},
                        {"%button_url%", mailModel.ButtonUrl },
                        {"%subject%", mailModel.Subject }
                    }

                }
                
            }.ToList();
            var jsonObject = new {
                from = new Recipient{
                    Email = "noreply@property.community",
                    Name = "Property Community"
                },
                template_id = SendGridActivationTemplateId,
                personalizations = personlized
            };

            var json = new JavaScriptSerializer().Serialize(jsonObject);
            var response = await client.RequestAsync(method: SendGridClient.Method.POST,
                                                     requestBody: json.ToString(),
                                                     urlPath: "mail/send");
            return response.StatusCode;
        }
        
        public static async Task<bool> SendActivationEmail(string userName, string userEmail, string confirmUrl)
        {
            string subject = "";
            string body = "";
            SendGridEmailModel mail = new SendGridEmailModel
            {
                RecipentName = userName,
                Subject = subject,
                Body = body,
                ButtonUrl = confirmUrl,
                RecipentEmail = userEmail
            };
            try
            {
                await SendEmailWithSendGrid(EmailType.ActivationEmail, mail);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> SendCreateAccountToTenantSendgrid(Person tenant, string email, string temPass, Person owner, string token, string address)
        {
            var nvc = new NameValueCollection();
            nvc.Set("token", token);
            nvc.Set("userId", tenant.Id.ToString());
            string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "Account/Activate", UtilService.ToQueryString(nvc));
            SendGridEmailModel mail = new SendGridEmailModel
            {
                RecipentName = tenant.FirstName,
                ButtonUrl = url,
                RecipentEmail = email,
                NewUserName = email,
                NewPassWord = temPass,
                OwnerName = owner.FirstName,
                Address= address
            };
            try
            {
                await SendEmailWithSendGrid(EmailType.OwnerCreatNewTenantEmail, mail);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }

        }


        public static async Task<HttpStatusCode> SendEmailWithSendGrid(EmailType emailType, SendGridEmailModel mailModel)
        {
            var client = new SendGridClient(SendGridApiKey);
            var personlized = new[]
            {
                new {
                    Subject = mailModel.Subject,
                    To = new List<Recipient>
                    {
                        new Recipient
                        {
                            Email = mailModel.RecipentEmail,
                        }
                    },
                    Substitutions = new Dictionary<string, string>
                    {
                        {"%first_name%",mailModel.RecipentName},
                        {"%owner_name%",mailModel.OwnerName},
                        {"%body%", mailModel.Body },
                        {"%button_text%" , mailModel.ButtonText},
                        {"%button_url%", mailModel.ButtonUrl },
                        {"%subject%", mailModel.Subject },
                        { "%new_user_name%", mailModel.NewUserName},
                        { "%new_password%", mailModel.NewPassWord},
                        {"%address%", mailModel.Address },
                        {"%job_title%", mailModel.JobTitle },
                        {"%person_type%", mailModel.PersonType },
                        {"%tenant_name%", mailModel.TenantName },
                        {"%date%", mailModel.Date },
                    }

                }

            }.ToList();
            var jsonObject = new
            {
                from = new Recipient
                {
                    Email = "noreply@property.community",
                    Name = "Property Community"
                },
                template_id = GetSendGridTemplateId(emailType),
                personalizations = personlized
            };

            var json = new JavaScriptSerializer().Serialize(jsonObject);
            var response = await client.RequestAsync(method: SendGridClient.Method.POST,
                                                     requestBody: json.ToString(),
                                                     urlPath: "mail/send");
            return response.StatusCode;
        }
        public static async Task SendEmailToGroup(EmailType emailType, IEnumerable<Recipient> recipients, string btnUrl = null, string address = null)
        {
            foreach (var person in recipients)
            {
                var mailModel = new SendGridEmailModel
                {
                    RecipentName = person.Name,
                    ButtonText = "",
                    ButtonUrl = btnUrl,
                    RecipentEmail = person.Email,
                    Address = address,
                    PersonType = person.PersonType
                };
                await SendEmailWithSendGrid(emailType, mailModel);
            }
        }
    }
}
