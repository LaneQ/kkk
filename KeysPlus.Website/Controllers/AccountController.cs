using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using KeysPlus.Data;
using System.Net;
using System.Text;
using System.Configuration;
using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
using SiteUtil = KeysPlus.Website.Models.SiteUtil;
using StringCipher = KeysPlus.Website.Models.StringCipher;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Web;


namespace KeysPlus.Website.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        KeysEntities db = new KeysEntities();
        /// <summary>
        /// Logins the specified return URL.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>

        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Login(string returnUrl)
        {

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            //ViewBag.Status = TempData["Status"].ToString();
            return View();
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult ChangePassword()
        {
            var changePasswordModel = new ChangePasswordModel
            {
                OldPassword = string.Empty,
                NewPassword = string.Empty,
                ConfirmPassword = string.Empty
            };
            return View(changePasswordModel);
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);

            if (!AccountService.ValidateUser(SiteUtil.CurrentUserName, model.OldPassword))
            {
                ModelState.AddModelError("OldPassword", "* Please enter correct current password.");
            }

            if (model.NewPassword.Equals(model.OldPassword))
            {
                ModelState.AddModelError("NewPassword", "* New Password must differ from old password.");
            }
            if (ModelState.IsValid)
            {
                var result = AccountService.ChangePasswordAfterLogin(model, login.Email);
                if (result.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }
                return View("Error");
            }
            else
            {
                return View(model);
            }
        }

        /// <summary>
        /// Changes the password success.
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        /// <summary>
        /// Logins the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = AccountService.Login(model);
                if (result.IsSuccess)
                {

                    FormsAuthentication.SetAuthCookie(model.UserName.ToLower(), model.RememberMe);
                    SiteUtil.ResetCurrentUserSession(model.UserName);
                    //string userName = (string)Session["CurrentUserName"];
                    //string[] userRoles = (string[])Session["UserRoles"];
                    
                    //ClaimsIdentity identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);

                    //identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userName));

                    //userRoles.ToList().ForEach((role) => identity.AddClaim(new Claim(ClaimTypes.Role, role)));

                    //identity.AddClaim(new Claim(ClaimTypes.Name, userName));

                    //HttpContext.GetOwinContext().Authentication.SignIn(identity);

                    var login = AccountService.GetLoginByEmail(model.UserName);

                    var firstTime = login.FirstTimeLogin ?? false;
                    if (!firstTime)
                    {
                        login.FirstTimeLogin = true;
                        var updateRes = AccountService.UpdateLogin(login);
                        if (updateRes.IsSuccess)
                        {
                            var role = AccountService.GetUserRolesbyEmail(login.Email).FirstOrDefault();
                            switch (role)
                            {
                                case 4:
                                    return RedirectToAction("Index", "Onboarding", new { Area = "PropertyOwners" });
                                case 5:
                                    return RedirectToAction("Onboarding", "Home", new { Area = "Tenants" });
                                case 6:
                                    return RedirectToAction("Index", "Onboarding", new { Area = "Companies" });
                            }
                        }
                    }
                    if ((Url == null || Url.IsLocalUrl(returnUrl))
                        && returnUrl.Length > 1
                        && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//")
                        && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }

                    var userRole = AccountService.GetUserRolesbyEmail(login.Email).FirstOrDefault();
                    switch (userRole)
                    {
                        /*
                         * 2: Admin            => Admin page
                         * 3: Property Manager => Dashboard
                         * 4: Property Owner   => Dashboard
                         * 5: Tenant           => Dashboard
                         * 6: Service Provider => Market place
                         * Other roles         => Properties for rent
                         */

                        case 2:
                            return RedirectToAction("Index", "Home", new { Area = "Admin" });
                        case 3:
                        case 4:
                        case 5:
                            return RedirectToAction("Dashboard", "Home", new { Area = "" });
                        case 6:
                            return RedirectToAction("Dashboard", "Home", new { Area = "" });
                        default:
                            return RedirectToAction("Dashboard", "Home", new { Area = "" });
                    }
                }
                else
                {
                    ModelState.AddModelError("Password", result.ErrorMessage);
                    return View(model);
                }
            }
            return View(model);
        }

        private ActionResult DoLoginAfterActivate(Login user)
        {

            return View();
        }

        [HttpGet]
        public ActionResult LogOff()
        {
            Session.Abandon();
            AccountService.LogOff();
            HttpContext.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        public JsonResult IsPassValid(string oldPassword)
        {
            return Json(AccountService.ValidateUser(SiteUtil.CurrentUserName, oldPassword), JsonRequestBehavior.AllowGet);
        }
        public JsonResult IsPassNew(string newPassword)
        {
            return Json(!AccountService.ValidateUser(SiteUtil.CurrentUserName, newPassword), JsonRequestBehavior.AllowGet);
        }

        public async Task<bool> SendEmailToUser(int userId, string token)
        {
            using (var db = new KeysEntities())
            {
                var user = db.Login.FirstOrDefault(x => x.Id == userId);
                var person = db.Person.FirstOrDefault(x => x.Id == userId);
                //String strPathAndQuery = Request.Url.PathAndQuery;
                //String strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                var emailConfirmTokenUrl = Url.Action("Activate", "Account", new { token = token, userId = userId }, Request.Url.Scheme);
                return await EmailService.SendActivationEmail(person.FirstName, user.Email, emailConfirmTokenUrl);
                //AccountService.SendActivationEmailToUser(userId, emailConfirmTokenUrl);
            }
        }


        public ActionResult AddRoleToLogin(int roleId)
        {

            var currentUser = AccountService.GetPersonByEmail(User.Identity.Name);
            var addNewRoleResult = AccountService.AddNewRoleToUser(roleId, currentUser);

            if (addNewRoleResult.IsSuccess)
            {
                switch (roleId)
                {
                    case 4:
                        return RedirectToAction("Index", "Onboarding", new { Area = "PropertyOwners" });
                    case 5:
                        return RedirectToAction("Onboarding", "Home", new { Area = "Tenants" });
                    case 6:
                        return RedirectToAction("Index", "Onboarding", new { Area = "Companies" });
                }
            }

            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Activate(string token, int userId)
        {
            ViewBag.token = token;
            ViewBag.userid = userId;
            TempData["Status"] = "OOPS....!!!!  You have clicked on the old activation link Or Your Account details are not correct";
            ViewBag.Flag = "Normal";
            DateTime utcDateTime = DateTime.Now.ToUniversalTime();
            var activateUser = AccountService.GetAwaitingActivateUserById(userId, token);
            if (activateUser != null)
            {
                if (activateUser.EmailConfirmationTokenExpiryDate < utcDateTime)
                {
                    TempData["Status"] = "OOPS...!!!!! Your Activation Link has expired. Please click on the below link to Activate the Account";
                    ViewBag.Flag = "Expired";
                }
               else if (activateUser.IsActive == true && activateUser.EmailConfirmed == true)
                {
                    TempData["Status"] = "Your account has already been activated.";
                    FormsAuthentication.SetAuthCookie(activateUser.UserName.ToLower(), true);
                    SiteUtil.ResetCurrentUserSession(activateUser.UserName);
                    //var role = AccountService.GetUserRolesbyEmail(activateUser.Email).FirstOrDefault();
                    var findingUser = db.Login.Find(userId);
                    var role = Convert.ToInt32(findingUser.IsActive);
                    switch (role)
                    {
                        case 1:
                            return RedirectToAction("Dashboard", "Home");
                            //case 4:
                            //    return RedirectToAction("Index", "Onboarding", new { Area = "PropertyOwners" });
                            //case 5:
                            //    return RedirectToAction("Onboarding", "Home", new { Area = "Tenants" });
                            //case 6:
                            //    return RedirectToAction("Index", "Onboarding", new { Area = "Companies" });
                    }
                    return RedirectToAction("Login", "Account");
                }
                else if (activateUser.EmailConfirmationTokenExpiryDate > utcDateTime)
                {
                    var result = AccountService.ActivateLogin(activateUser.Id);
                    if (result.IsSuccess)
                    {
                        FormsAuthentication.SetAuthCookie(activateUser.UserName.ToLower(), true);
                        SiteUtil.ResetCurrentUserSession(activateUser.UserName);
                        var role = AccountService.GetUserRolesbyEmail(activateUser.Email).FirstOrDefault();
                        switch (role)
                        {
                            case 4:
                                return RedirectToAction("Index", "Onboarding", new { Area = "PropertyOwners" });
                            case 5:
                                return RedirectToAction("Onboarding", "Home", new { Area = "Tenants" });
                            case 6:
                                return RedirectToAction("Index", "Onboarding", new { Area = "Companies" });
                        }
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        TempData["Status"] = "Something went wrong, please try again later!.";
                    }
                }
                //else
                //{
                //    TempData["Status"] = "OOPS...!!!!! Your Activation Link has expired. Please click on the below link to Activate the Account";
                //    ViewBag.Flag = "Expired";
                //}
            }
            else
            {
                TempData["Status"] = "OOPS....!!!!  You have clicked on the old activation link Or Your Account details are not correct";
            }
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Activate(string HdToken, string HdUserId)
        {

            int userId = Convert.ToInt32(HdUserId);
            var result = AccountService.ResetActivate(userId, HdToken);
            if (result.IsSuccess)
            {
                await SendEmailToUser(userId, result.NewObject.ToString());
                TempData["Status"] = "Your Account has been reset. Please activate the account by clicking the activate link in your email";
                ViewBag.Flag = "Reset";
            }
            else
            {
                TempData["Status"] = result.ErrorMessage;
                ViewBag.Flag = "Cancel";
            }
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult ActivateMessage()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword(string token, string EmailAddress /*email*/, DateTime? ResetPasswordTokenExpiryDate)
        {
            ViewBag.token = token;
            ViewBag.Email = Decrypt(EmailAddress);
            DateTime? tokenDate = ResetPasswordTokenExpiryDate;
            ViewBag.resetSession = "true";
            var resetUser = AccountService.GetLoginByEmail(ViewBag.Email);

            if (resetUser != null)
            {
                if (tokenDate > DateTime.Now)
                {
                    ViewBag.resetSession = "false";
                    TempData["Status"] = "Your Account Reset password token has expired! Please do reset again";
                }
                else
                {
                    if (resetUser.ResetPasswordToken != token)
                    {
                        ViewBag.resetSession = "false";
                        TempData["Status"] = "The Reset Password Link seems to be Tampered!";
                    }
                    else
                    {
                        return View();
                    }
                }
            }
            else
            {
                ViewBag.resetSession = "false";
                TempData["Status"] = "The Reset Password Link seems to be Tampered!";
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(ResetPasswordViewModel model, string code)
        {
            string token = code;
            TempData["Status"] = "OOPS....!!!! Your Account details are not correct";
            var result = AccountService.ChangePassword(model, token);
            if (result.IsSuccess)
            {
                TempData["Status"] = "Congratulations...!!!!! Your password has been reset.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["Status"] = result.ErrorMessage;

            }
            return View();
            //if (!string.IsNullOrEmpty(token))
            //{
            //    if (db.Login.Any(n => n.ResetPasswordToken == token))
            //    {
            //        var salt = SiteUtil.GeneratePassword(10, 5);
            //        var passwordHash = SiteUtil.CreatePasswordHash(model.Password, salt);

            //        var LoginAgain = db.Login.Where(x => x.ResetPasswordToken == token).FirstOrDefault();
            //        if (LoginAgain.ResetPasswordTokenExpiryDate < DateTime.Now)
            //        {
            //            TempData["Status"] = "Your Account Reset password token has expired! Please do reset again";
            //        }
            //        else
            //        {
            //            LoginAgain.PasswordHash = passwordHash;
            //            LoginAgain.SecurityStamp = salt;
            //            LoginAgain.ResetPasswordToken = "";
            //            db.SaveChanges();
            //            return RedirectToAction("Login", "Account");
            //        }
            //    }
            //    else
            //    {
            //        TempData["Status"] = "OOPS....!!!! Your Account details are not correct";
            //    }
            //}
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var resetTokenHash = Guid.NewGuid().ToString();
            var result = AccountService.ResetPasswordToken(model, resetTokenHash);
            var user = AccountService.GetPersonByEmail(model.Email);
            var userLogin = AccountService.GetLoginByEmail(model.Email);
            var name = "User";
            if (user != null)
            {
                name = user.FirstName;
            } else
            {
                result.IsSuccess = false;
            }
            if (result.IsSuccess)
            {
                //   SendEmailForForgotPassword(model.Email, user.ResetPasswordTokenExpiryDate, resetTokenHash.ToString());

                var EmailAddress = Encrypt(model.Email);
                var nvc = new NameValueCollection();
                nvc.Add("token", resetTokenHash);
                nvc.Add("EmailAddress", EmailAddress);
                nvc.Add("ResetPasswordTokenExpiryDate", userLogin.ResetPasswordTokenExpiryDate.ToString());
                string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "/Account/ResetPassword", UtilService.ToQueryString(nvc));
                SendGridEmailModel mail = new SendGridEmailModel
                {
                    RecipentName = name,
                    ButtonUrl = url,
                    RecipentEmail = model.Email,

                };
                await EmailService.SendEmailWithSendGrid(EmailType.ForGetPasswordEmail, mail);



                return View("ForgotPasswordConfirmation");
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return View();
            }
        }

        //public void SendEmailForForgotPassword(string EmailAddress, DateTime? ResetPasswordTokenExpiryDate, string Token)
        //{

        //    string EmailID = ConfigurationManager.AppSettings["EmailServer"];
        //    string Password = ConfigurationManager.AppSettings["Password"];
        //    var user = AccountService.GetPersonByEmail(EmailAddress);
        //    var username = user.FirstName;
        //    String strPathAndQuery = Request.Url.PathAndQuery;
        //    String strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");

        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("Hi " + username + ",<br/>");
        //    sb.Append("<br/><table style='border-color:#666' cellpadding='10' width='100%'>");
        //    sb.Append("<tbody><tr style='background:#3c8dbc'>");
        //    sb.Append("<td>You have requested a password reset.To reset your password please click here.</td></tr><tr>");
        //    //sb.Append("<td> Welcome User To complete the signup process please click on the activation link ");
        //    //    var emailConfirmTokenUrl = Url.Action("Activate", "Account", new { token = token, userId = userId }, Request.Url.Scheme)

        //    //var emailConfirmTokenUrl = Url.Action("Activate", "Account", new { token = token, userId = userId }, Request.Url.Scheme);
        //    //sb.Append($"<a href='{emailConfirmTokenUrl}'>Activate Now</a>");

        //    //var emailConfirmTokenUrl = Url.Action("ResetPassword", "Account", new { token = Token, EmailAddress = EmailAddress, ResetPasswordTokenExpiryDate = ResetPasswordTokenExpiryDate }, Request.Url.Scheme);
        //    //sb.Append($"<a href='{emailConfirmTokenUrl}'>Click Now</a>");

        //    var emailConfirmTokenUrl = Url.Action("ResetPassword", "Account", new { token = Token, EmailAddress = Encrypt(EmailAddress), ResetPasswordTokenExpiryDate = ResetPasswordTokenExpiryDate }, Request.Url.Scheme);
        //    sb.Append($"<a href='{emailConfirmTokenUrl}'>ClickHere</a>");
        //    //  sb.Append(String.Format("<a href=\"{0}Account/ResetPassword?token={1}\">Click Now</a>", strUrl, Token));
        //    sb.AppendLine("</td></tr></tbody></table>");
        //    sb.Append("<br/><br/>Regards,<br>Software Team");
        //    try
        //    {
        //        SmtpClient client = new SmtpClient
        //        {
        //            Host = "smtp.gmail.com",
        //            Port = 587,
        //            EnableSsl = true,
        //            DeliveryMethod = SmtpDeliveryMethod.Network,
        //            Credentials = new System.Net.NetworkCredential(EmailID, Password),
        //            Timeout = 10000,
        //        };

        //        MailMessage Mail = new MailMessage();
        //        Mail.To.Add(EmailAddress);
        //        Mail.From = new MailAddress(EmailID);
        //        Mail.Subject = ("Reset Password");
        //        Mail.Body = (sb.ToString());
        //        Mail.IsBodyHtml = true;
        //        client.Send(Mail);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}

        private static string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        private string Decrypt(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes;
            try
            {
                cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            catch (FormatException)
            {
                ViewBag.resetSession = "false";
                TempData["Status"] = "The Reset Password Link seems to be Tampered!";
            };

            return cipherText;
        }

        //***************************************************************************
        //                      Role For DropDownList
        //****************************************************************************
        ///<summary>
        /// Registers this instance.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            //var allRoles = AccountService.GetAllRoles().Where(a => (a.Name.ToUpper().Replace(" ", "") != "ADMIN" && a.Name.ToUpper().Replace(" ", "") != "SUPERADMIN")).ToList(); //Bug Fix - #2075
            var allRoles = AccountService.GetAllRoles().Where(x => x.Id != 1 && x.Id != 2 && x.Id != 3).ToList();
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Roles = allRoles;
            return View();
        }

        /// Registers the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            #region should move to AccountService
            if (AccountService.CheckIfUserExist(model.UserName))
            {
                ModelState.AddModelError("UserName", "Email already in use!");
                var allRoles = AccountService.GetAllRoles().Where(a => (a.Name.ToUpper().Replace(" ", "") != "ADMIN" && a.Name.ToUpper().Replace(" ", "") != "SUPERADMIN")).ToList();
                ViewBag.Roles = allRoles;
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var emailConfirmToken = Guid.NewGuid().ToString();

                if (model.UserName == model.Password)
                {
                    ModelState.AddModelError("Password", "User Name and Password should not be same!");
                }
                else
                {
                    try
                    {
                        var newUser = AccountService.AddNewLoginAndPerson(model, emailConfirmToken);
                        if (newUser == null)
                        {
                            ViewBag.Error = "Something went wrong, please try again later!";
                            var allRoles = AccountService.GetAllRoles().Where(a => (a.Name.ToUpper().Replace(" ", "") != "ADMIN" && a.Name.ToUpper().Replace(" ", "") != "SUPERADMIN")).ToList(); //Bug Fix #2075
                            ViewBag.Roles = allRoles; //Bug Fix #2075
                            return View(model);
                        }
                        await SendEmailToUser(newUser.Id, emailConfirmToken);
                        return RedirectToAction("ActivateMessage", "Account");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return View(model);
                    }


                }
                var allRoles1 = AccountService.GetAllRoles().Where(a => (a.Name.ToUpper().Replace(" ", "") != "ADMIN" && a.Name.ToUpper().Replace(" ", "") != "SUPERADMIN")).ToList(); //Bug Fix #2075
                ViewBag.Roles = allRoles1; //Bug Fix #2075
                return View(model);
            }
            else
            {
                var allRoles = AccountService.GetAllRoles().Where(a => (a.Name.ToUpper().Replace(" ", "") != "ADMIN" && a.Name.ToUpper().Replace(" ", "") != "SUPERADMIN")).ToList(); //Bug Fix - #2075

                ViewBag.Roles = allRoles;
                return View(model);
            }
            #endregion
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult RegisterToBeTenant(AddTenantToPropertyModel model)
        {
            var registerModel = new RegisterToBeTenantModel
            {
                //  RegisterModel = new RegisterViewModel {UserName = model.TenantEmail},
                UserName = model.TenantEmail,
                PropertyId = model.PropertyId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                PaymentFrequencyId = model.PaymentFrequencyId,
                PaymentAmount = model.PaymentAmount
            };
            return View(registerModel);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> RegisterToBeTenant(RegisterToBeTenantModel model)
        {
            if (AccountService.CheckIfUserExist(model.UserName))
            {
                ModelState.AddModelError("UserName", "Email already in use");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var emailConfirmToken = Guid.NewGuid().ToString();
                //var salt = UtilService.GeneratePassword(10, 5);
                //var passwordHash = AccountService.CreatePasswordHash(model.Password, salt);
                try
                {
                    var newTenant = new RegisterViewModel
                    {
                        UserName = model.UserName,
                        Password = model.Password,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        RoleId = 5,
                    };
                    var newUser = AccountService.AddNewLoginAndPerson(newTenant, emailConfirmToken);
                    if (newUser == null)
                    {
                        ViewBag.Error = "Please try again later!";
                        return View(model);
                    }
                    var addTenantModel = new AddTenantToPropertyModel { PropertyId = model.PropertyId, StartDate = model.StartDate, EndDate = model.EndDate, PaymentAmount = model.PaymentAmount, PaymentFrequencyId = model.PaymentFrequencyId };

                    var result = await AccountService.SendActivationEmailToTenant(newUser.Id, newUser.UserName, emailConfirmToken.ToString(), addTenantModel);
                    if (result.IsSuccess) return RedirectToAction("ActivateMessage", "Account");
                    else
                    {
                        ViewBag.Error = result.ErrorMessage;
                        return View(model);
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = "Please try again later!";
                    Console.WriteLine(e.Message);
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ActivateToBeTenant(ActivateTobeTenantModel model)
        {

            model.StartDate = DateTime.Parse(model.StartDate1);

            if (model.EndDate1 != null)
            {
                model.EndDate = DateTime.Parse(model.EndDate1);
            }
            else
            {
                model.EndDate = null;
            }

            if (ModelState.IsValid)
            {
                var activateTenant = AccountService.GetAwaitingActivateUserByEmail(model.Email, model.Token);
                if (activateTenant == null)
                {
                    return View("AccountError");
                }
                if (activateTenant.IsActive == true && activateTenant.EmailConfirmed == true)
                {
                    TempData["Status"] = "Your Account has already activated.";
                    var user = model.Email;

                    var login = AccountService.GetLoginByEmail(user);
                    var addTenantResult = PropertyService.AddTenant(new Tenant { Id = model.TenantId });
                    var result = PropertyService.AddTenantToProperty(login, model.TenantId, model.PropertyId, model.StartDate, model.EndDate, model.PaymentFrequencyId, model.PaymentAmount);
                    if (addTenantResult.IsSuccess && result.IsSuccess)
                    {
                        TempData["Status"] = "Your Account has already activated.";
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        return View("AccountError");
                    }
                }
                else
                {
                    var date = activateTenant.EmailConfirmationTokenExpiryDate ?? null;
                    if (date != null)
                    {
                        var nowTime = DateTime.Now.ToUniversalTime();
                        var expiryDate = Convert.ToDateTime(date);
                        var Checkresult = DateTime.Compare(nowTime, expiryDate);
                        if (Checkresult > 0)
                        {
                            TempData["Status"] = "Your Email has already Expired.";
                            return View("AccountError");
                        }
                    }
                    var activateResult = AccountService.ActivateLogin(activateTenant.Id);
                    if (activateResult.IsSuccess)
                    {
                        var user = User.Identity.Name;

                        var login = AccountService.GetLoginByEmail(user);
                        var addTenantResult = PropertyService.AddTenant(new Tenant { Id = model.TenantId });
                        var result = PropertyService.AddTenantToProperty(login, model.TenantId, model.PropertyId, model.StartDate, model.EndDate, model.PaymentFrequencyId, model.PaymentAmount);
                        if (addTenantResult.IsSuccess && result.IsSuccess)
                        {
                            TempData["Status"] = "Congratulations...!!!!! Your Account has been activated.";
                            return RedirectToAction("Login", "Account");
                        }
                        else
                        {
                            return View("AccountError");
                        }
                    }
                    else
                    {
                        return View("AccountError");
                    }
                }

            }
            else
            {
                return View("AccountError");
            }

        }

        [HttpGet]
        public ActionResult AccountOverview()
        {
            var username = User.Identity.Name;
            var currentUser = AccountService.GetLoginByEmail(username);
            Login getloginuser = db.Login.FirstOrDefault(u => u.UserName.Equals(username));
            Person personData = db.Person.FirstOrDefault(us => us.Id.Equals(getloginuser.Id));
            ServiceProvider serviceSuppData = db.ServiceProvider.FirstOrDefault(s => s.Id.Equals(personData.Id));
            Company companyData = serviceSuppData != null ? db.Company.FirstOrDefault(c => c.Id == serviceSuppData.CompanyId) : null;
            Address phyAddress = db.Address.FirstOrDefault(x => x.AddressId.Equals(personData.PhysicalAddressId));
            Address billAddress = db.Address.FirstOrDefault(x => x.AddressId.Equals(personData.BillingAddressId));
            //AccountOverviewModel model = personData.MapTo<AccountOverviewModel>();
            AccountOverView model = new AccountOverView();
            model.Profile = new ProfileOverView();
            model.Profile = personData.MapTo<ProfileOverView>();
            model.Profile.PhoneNumber = personData.Login.PhoneNumber;
            model.Profile.FullUserName = personData.FirstName + " " + personData.MiddleName + " " + personData.LastName;
            //model.ProfilePhoto = personData.ProfilePhoto;
            model.Profile.ProfilePhotoModel = new MediaModel { Data = Url.Content(MediaService.GetContentPath(personData.ProfilePhoto)), Status = "load" };
            model.Profile.MediaFiles = personData.PersonMedia.Select(y => new MediaModel
            {
                Data = Url.Content(MediaService.GetContentPath(y.NewFilename)),
                Id = y.Id,
                MediaType = MediaService.GetMediaType(y.OldFilename),
                NewFileName = y.NewFilename,
                OldFileName = y.OldFilename,
                Status = "load",
                Size = MediaService.GetFileSize(y.NewFilename),
            }).ToList();
            model.Profile.PhysicalAddress = phyAddress.MapTo<AddressViewModel>();
            model.Profile.BillingAddress = billAddress.MapTo<AddressViewModel>();
            if (companyData != null)
            {
                model.IsServiceSupplier = true;
                var com = new CompanyDetailViewModel
                {
                    Id = companyData.Id,
                    Name = companyData.Name,
                    Phone = companyData.PhoneNumber,
                    Website = companyData.Website,
                    ProfilePhoto = new MediaModel { Data = Url.Content(MediaService.GetContentPath(companyData.ProfilePhoto)), Status = "load" }

                };
                model.Company = com;
                Address compPhyAddress = db.Address.FirstOrDefault(w => w.AddressId == companyData.PhysicalAddressId);
                Address compBillAddress = db.Address.FirstOrDefault(w => w.AddressId == companyData.BillingAddressId);
                model.Company.PhysicalAddress = compPhyAddress.MapTo<AddressViewModel>();
                model.Company.BillingAddress = compBillAddress.MapTo<AddressViewModel>();
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(ProfileOverView model)
        {
            int roleId=0;
            if (model.UserRole == "Tenant") { roleId = 5; } 
            if(model.UserRole=="Service Provider") { roleId = 6; } 
            if(model.UserRole=="Property Owner") { roleId = 4; }
            if (model.UserRole == "Guest") { roleId = 7; }
            var loginRole = new LoginRole {
                PersonId=model.Id,
                RoleId = roleId,
                IsActive = true,
                PendingApproval=false
            };
            var checkRole = db.LoginRole.Where(m => m.RoleId == roleId && m.PersonId == model.Id);
            if(checkRole==null)
            {
                if (model.UserRole != null)
                {
                    db.LoginRole.Add(loginRole);
                }
            }
            OpenPageByRoles(model.UserRole);
            var person = db.Person.Where(p => p.Id == model.Id).FirstOrDefault();
            var status = true;
            var message = "";
            var files = Request.Files;
            if (!ModelState.IsValid)
            {
                status = false;
                message = "Something went wrong, a field must have been invalid.";
            }
            else
            {

                person.FirstName = model.FirstName;
                person.MiddleName = model.MiddleName;
                person.LastName = model.LastName;
                person.PlaceOfBirth = model.PlaceOfBirth;
                person.Language = model.Language;
                person.Occupation = model.Occupation;
                person.LinkedinUrl = model.LinkedinUrl;
                person.Login.PhoneNumber = model.PhoneNumber;
                person.UpdatedBy = HttpContext.User.Identity.GetUserName();
                person.UpdatedOn = DateTime.Now;

                var saveResultProfile = MediaService.SaveMediaFilesWithFilterAndRotate(files, 5, "NewPhoto");

                if (!(person.ProfilePhoto == null))
                {
                    if (model.RemoveUserPhoto == true)
                    {
                        MediaService.RemoveMediaFile(person.ProfilePhoto);
                        person.ProfilePhoto = "";
                    }
                }
                
                if (saveResultProfile != null && saveResultProfile.IsSuccess)
                {
                    var newOb = saveResultProfile.NewObject as List<MediaModel>;
                    person.ProfilePhoto = newOb[0].NewFileName;
                }

                var saveResult = MediaService.SaveMediaFilesWithFilter(files, 5, "Files");
                if (saveResult != null)
                {

                    var newOb = saveResult.NewObject as List<MediaModel>;
                    var mediaresult = AccountService.SavePersonMedia(newOb, model.Id);
                }

                if (model.FilesRemoved != null)
                {
                    foreach (var item in model.FilesRemoved)
                    {
                        var media = person.PersonMedia.FirstOrDefault(y => y.Id == item);
                        if (media != null)
                        {
                            db.PersonMedia.Remove(media);
                            MediaService.RemoveMediaFile(media.NewFilename);
                        }
                    }
                }
                if (person.PhysicalAddressId == person.BillingAddressId)
                {
                    if (model.IsShipSame)
                    {
                        person.Address1.Number = model.PhysicalAddress.Number;
                        person.Address1.Street = model.PhysicalAddress.Street;
                        person.Address1.PostCode = model.PhysicalAddress.PostCode;
                        person.Address1.City = model.PhysicalAddress.City;
                        person.Address1.Suburb = model.PhysicalAddress.Suburb;
                    }
                    else
                    {
                        Address billingAddress = new Address
                        {
                            Number = model.BillingAddress.Number,
                            CountryId = 1,
                            Street = model.BillingAddress.Street,
                            City = model.BillingAddress.City,
                            Suburb = model.BillingAddress.Suburb,
                            PostCode = model.BillingAddress.PostCode,
                            Lat = model.BillingAddress.Latitude,
                            Lng = model.BillingAddress.Longitude,
                            CreatedBy = HttpContext.User.Identity.GetUserName(),
                            CreatedOn = DateTime.Now,
                            UpdatedBy = HttpContext.User.Identity.GetUserName(),
                            UpdatedOn = DateTime.Now
                        };
                        db.Address.Add(billingAddress);
                        person.BillingAddressId = billingAddress.AddressId;
                        db.SaveChanges();
                    }
                }
                else
                {
                    if (!model.IsShipSame)
                    {
                        //physical address
                        person.Address1.Number = model.PhysicalAddress.Number;
                        person.Address1.Street = model.PhysicalAddress.Street;
                        person.Address1.PostCode = model.PhysicalAddress.PostCode;
                        person.Address1.City = model.PhysicalAddress.City;
                        person.Address1.Suburb = model.PhysicalAddress.Suburb;

                        //billing address
                        person.Address.Number = model.BillingAddress.Number;
                        person.Address.Street = model.BillingAddress.Street;
                        person.Address.PostCode = model.BillingAddress.PostCode;
                        person.Address.City = model.BillingAddress.City;
                        person.Address.Suburb = model.BillingAddress.Suburb;


                    }
                    else
                    {
                        person.Address1.Number = model.PhysicalAddress.Number;
                        person.Address1.Street = model.PhysicalAddress.Street;
                        person.Address1.PostCode = model.PhysicalAddress.PostCode;
                        person.Address1.City = model.PhysicalAddress.City;
                        person.Address1.Suburb = model.PhysicalAddress.Suburb;

                        var bill = db.Address.Where(p => p.AddressId == model.BillingAddress.AddressId).First();


                        person.BillingAddressId = person.PhysicalAddressId;
                        db.SaveChanges();
                    }
                }
                db.SaveChanges();
                message = "User edited successfully.";
            }
            return Json(new { success = status, message = message });
        }

        [HttpPost]
        public ActionResult EditCompany(CompanyDetailViewModel model)
        {
            var company = db.Company.Where(p => p.Id == model.Id).FirstOrDefault();
            var status = true;
            var message = "";
            //var file = model.Image;
            var files = Request.Files;

            if (!ModelState.IsValid)
            {
                status = false;
                message = "Something went wrong, a field must have been invalid.";
            }
            else
            {
                company.Name = model.Name;
                company.PhoneNumber = model.Phone;
                if (!String.IsNullOrWhiteSpace(model.Website))
                {
                    company.Website = model.Website;
                }
                if (model.Website == "null") company.Website = null;
                var saveResultProfile = MediaService.SaveMediaFilesWithFilterAndRotate(files, 1, "NewCompPhoto");

                if (!(company.ProfilePhoto == null))
                {
                    if (model.RemoveProfilePhoto == true)
                    {
                        MediaService.RemoveMediaFile(company.ProfilePhoto);
                        company.ProfilePhoto = "";
                    }
                }

                if (saveResultProfile != null && saveResultProfile.IsSuccess)
                {
                    var newOb = saveResultProfile.NewObject as List<MediaModel>;
                    company.ProfilePhoto = newOb[0].NewFileName;
                }

                if (company.PhysicalAddressId == company.BillingAddressId)
                {
                    if (model.IsCompShipSame)
                    {
                        company.Address1.Number = model.PhysicalAddress.Number;
                        company.Address1.Street = model.PhysicalAddress.Street;
                        company.Address1.PostCode = model.PhysicalAddress.PostCode;
                        company.Address1.City = model.PhysicalAddress.City;
                        company.Address1.Suburb = model.PhysicalAddress.Suburb;
                    }
                    else
                    {
                        Address billingAddress = new Address
                        {
                            Number = model.BillingAddress.Number,
                            CountryId = 1,
                            Street = model.BillingAddress.Street,
                            City = model.BillingAddress.City,
                            Suburb = model.BillingAddress.Suburb,
                            PostCode = model.BillingAddress.PostCode,
                            Lat = model.BillingAddress.Latitude,
                            Lng = model.BillingAddress.Longitude,
                            CreatedBy = HttpContext.User.Identity.GetUserName(),
                            CreatedOn = DateTime.Now,
                            UpdatedBy = HttpContext.User.Identity.GetUserName(),
                            UpdatedOn = DateTime.Now
                        };
                        db.Address.Add(billingAddress);
                        company.BillingAddressId = billingAddress.AddressId;
                        db.SaveChanges();
                    }
                }
                else
                {
                    if (!model.IsCompShipSame)
                    {
                        //physical address
                        company.Address1.Number = model.PhysicalAddress.Number;
                        company.Address1.Street = model.PhysicalAddress.Street;
                        company.Address1.PostCode = model.PhysicalAddress.PostCode;
                        company.Address1.City = model.PhysicalAddress.City;
                        company.Address1.Suburb = model.PhysicalAddress.Suburb;

                        //billing address
                        company.Address.Number = model.BillingAddress.Number;
                        company.Address.Street = model.BillingAddress.Street;
                        company.Address.PostCode = model.BillingAddress.PostCode;
                        company.Address.City = model.BillingAddress.City;
                        company.Address.Suburb = model.BillingAddress.Suburb;


                    }
                    else
                    {
                        company.Address1.Number = model.PhysicalAddress.Number;
                        company.Address1.Street = model.PhysicalAddress.Street;
                        company.Address1.PostCode = model.PhysicalAddress.PostCode;
                        company.Address1.City = model.PhysicalAddress.City;
                        company.Address1.Suburb = model.PhysicalAddress.Suburb;

                        var bill = db.Address.Where(p => p.AddressId == model.BillingAddress.AddressId).First();


                        company.BillingAddressId = company.PhysicalAddressId;
                        db.SaveChanges();
                    }
                }
                db.SaveChanges();
                message = "Company edited successfully.";
            }
            return Json(new { success = status, message = message });
        }
        [HttpGet]
        public JsonResult CheckIfCanBeTenant(string tenantEmail, int propertyId)
        {
            var res = AccountService.CanBeTenant(tenantEmail, propertyId);
            return Json(new { Exist = res.Result != null , Success = res.IsSuccess, Msg = res.ErrorMessage, FirstName = res.Result?.FirstName, LastName = res.Result?.LastName }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CheckAccountExist(string tenantEmail, int propertyId)
            {
            var login = AccountService.GetExistingTenant(tenantEmail);
            var result = TenantService.IsTenantInProperty(tenantEmail, propertyId);
            var existingSupplier = CompanyService.IsUserServiceSupplier(tenantEmail);
            var existingOwner = PropertyOwnerService.IsUserOwner(tenantEmail);
            var existingTenant = TenantService.IsUsernameTenant(tenantEmail);

            var existingSupplierOrOwner =  existingSupplier || existingOwner  ? true : false;
     
            Person person = null;
            if (login != null )
            {
                person = AccountService.GetPersonById(login.Id);
            }
            return Json(new { Exist = login != null, FirstName = person?.FirstName, LastName = person?.LastName, TenantInProperty = result, SupplierOrOwner = existingSupplierOrOwner }, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ViewResult ChooseRoles()
        {
            var allRoles = AccountService.GetAllRoles().Where(a => (a.Name.ToUpper().Replace(" ", "") != "ADMIN" && a.Name.ToUpper().Replace(" ", "") != "SUPERADMIN")).ToList(); //Bug Fix - #2075
            ViewBag.Roles = allRoles;
            return View();
        }

        public ViewResult OpenPageByRoles(string userRole)
        {

            return View();
        }
    }    
}
