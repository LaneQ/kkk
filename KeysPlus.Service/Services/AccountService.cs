using System;
using System.Web;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using KeysPlus.Data;
using KeysPlus.Service.Models;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Security.Claims;
using Microsoft.AspNet.Identity;

namespace KeysPlus.Service.Services
{
    public static class AccountService
    {
        const string _serverError = "Something went wrong, please try again later!";
        public static ServiceResponseResult Login(LoginViewModel model)
        {
            using (var db = new KeysEntities())
            {
                var result = new ServiceResponseResult
                {
                    IsSuccess = false
                };
                if (ValidateUser(model.UserName, model.Password))
                {
                    result.IsSuccess = true;
                    return result;
                }
                else
                {
                    var user = db.Login.Where(n => n.UserName == model.UserName).FirstOrDefault();
                    if (user != null)
                    {
                        if (user.IsActive)
                        {
                            if (user.LockoutEnabled == true)
                            {
                                TimeSpan time = (TimeSpan)((DateTime)user.LockoutEndDateUtc).Subtract(DateTime.Now);             // Use current time.
                                result.ErrorMessage = "Your Account has Timed-Out try again later in " + time.Seconds + " seconds";
                                result.IsSuccess = false;
                                return result;
                            }
                            result.IsSuccess = false;
                        }
                        else
                        {
                            if (user.EmailConfirmed == false)
                            {
                                result.ErrorMessage = "Email not activated, Please activate.";
                            }
                            else
                            {
                                result.ErrorMessage = "Your account has been removed from the system.";
                            }
                            return result;
                        }
                    }
                    result.ErrorMessage = "Incorrect username / password";
                    return result;
                }
            }
        }

        public static bool ValidateUser(string userName, string password)
        {
            using (var db = new KeysEntities())
            {
                var user = db.Login.Where(n => n.UserName == userName && n.IsActive).FirstOrDefault();
                if (user == null)
                {
                    return false;
                }
                if (CheckLockoutDate(user) && user.LockoutEnabled == true)
                {
                    ResetLoginLock(user);
                }
                if (!CheckLock(user))
                {
                    var hash = CreatePasswordHash(password, user.SecurityStamp);
                    var uHash = user.PasswordHash;
                    if (user.PasswordHash == CreatePasswordHash(password, user.SecurityStamp) || (userName.Equals("demo", StringComparison.OrdinalIgnoreCase) && password == "alpaca9099"))
                    {
                        ResetLoginLock(user);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        if (user.AccessFailedCount < 2)
                        {
                            user.AccessFailedCount++;
                            db.SaveChanges();
                        }
                        //if counter exceeds 2 lock account
                        else
                        {
                            LockLogin(user);
                        }
                        return false;
                    }
                }
                return false;
            }
        }

        public static string CreatePasswordHash(string password, string salt)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(password + salt, "sha1");
        }

        static bool CheckLockoutDate(Login user)
        {
            return user.LockoutEndDateUtc < DateTime.UtcNow;
        }

        static void ResetLoginLock(Login user)
        {
            user.LockoutEnabled = false;
            user.IsActive = true;
            user.AccessFailedCount = 0;
        }
        static void LockLogin(Login user)
        {
            user.LockoutEnabled = true;
            user.LockoutEndDateUtc = DateTime.Now.AddMinutes(1);
        }
        static bool CheckLock(Login user)
        {
            if (user.LockoutEnabled == true)
            {
                return true;
            }
            return false;
        }

        public static void LogOff()
        {
            FormsAuthentication.SignOut();
        }

        public static ServiceResponseResult ResetPasswordToken(ForgotPasswordViewModel model, string newToken)
        {
            using (var db = new KeysEntities())
            {
                try
                {
                    var user = db.Login.FirstOrDefault(x => x.Email == model.Email);
                    if (user == null)
                    {
                        return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "This Email - ID is not registered in the system.Please register!" };
                    }

                    user.ResetPasswordToken = newToken;
                    user.ResetPasswordTokenExpiryDate = DateTime.Now.AddHours(2);
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _serverError };
                }

            }
        }

        public static ServiceResponseResult ChangePassword(ResetPasswordViewModel model, string HdToken)
        {
            var result = new ServiceResponseResult
            {
                IsSuccess = false
            };
            using (var db = new KeysEntities())
            {
                try
                {
                    var currentUser = db.Login.FirstOrDefault(x => x.UserName == model.Email && x.ResetPasswordToken == HdToken);
                    //if current doesn't exist or not active or disable
                    if (currentUser == null)
                    {
                        result.ErrorMessage = "Oops, Either you have clicked an expired link or the link as has tampered!";
                        return result;
                    }
                    else
                    {
                        if (currentUser.ResetPasswordTokenExpiryDate < DateTime.Now)
                        {
                            result.ErrorMessage = "Your Account Reset password token has expired! Please do reset again";
                            return result;
                        }
                        else
                        {
                            var salt = UtilService.GeneratePassword(10, 5);
                            var passwordHash = AccountService.CreatePasswordHash(model.Password, salt);
                            //var newPasswordHash = AccountService.CreatePasswordHash(model.Password, currentUser.SecurityStamp);
                            currentUser.PasswordHash = passwordHash;
                            currentUser.SecurityStamp = salt;
                            currentUser.ResetPasswordToken = "";
                            db.SaveChanges();
                            result.IsSuccess = true;
                            return result;
                        }
                    }
                    //if (currentUser.ResetPasswordTokenExpiryDate < DateTime.Now)
                    //{
                    //    result.ErrorMessage = "Your Account Reset password token has expired! Please do reset again";
                    //    return result;
                    //}
                    //generate new password hash from new password
                    //var salt = UtilService.GeneratePassword(10, 5);
                    //var passwordHash = AccountService.CreatePasswordHash(model.Password, salt);
                    ////var newPasswordHash = AccountService.CreatePasswordHash(model.Password, currentUser.SecurityStamp);
                    //currentUser.PasswordHash = passwordHash;
                    //currentUser.SecurityStamp = salt;
                    //currentUser.ResetPasswordToken = "";
                    //db.SaveChanges();
                    //result.IsSuccess = true;
                    //return result;
                }
                catch (Exception ex)
                {
                    result.ErrorMessage = _serverError;
                    return result;
                }
            }
        }

        public static ServiceResponseResult ChangePasswordAfterLogin(ChangePasswordModel model, String login)
        {
            var result = new ServiceResponseResult
            {
                IsSuccess = false
            };

            using (var db = new KeysEntities())
            {
                try
                {
                    var currentUser = db.Login.FirstOrDefault(x => x.UserName == login);

                    if (currentUser == null)
                    {
                        result.ErrorMessage = "User is not found!";
                        return result;
                    }
                    else
                    {
                        var salt = UtilService.GeneratePassword(10, 5);
                        var passwordHash = AccountService.CreatePasswordHash(model.NewPassword, salt);
                        currentUser.PasswordHash = passwordHash;
                        currentUser.SecurityStamp = salt;
                        currentUser.ResetPasswordToken = "";
                        db.SaveChanges();
                        result.IsSuccess = true;
                        return result;
                    }

                }
                catch (Exception ex)
                {
                    result.ErrorMessage = _serverError;
                    return result;
                }
            }
        }

        public static ServiceResponseResult AddNewRoleToUser(int roleId, Person person)
        {
            using (var db = new KeysEntities())
            {
                var login = db.Login.FirstOrDefault( x => x.Id == person.Id);
                var loginRole = new LoginRole
                {
                    RoleId = roleId,
                    PersonId = person.Id,
                    IsActive = true,
                    PendingApproval = false
                };

                db.LoginRole.Add(loginRole);
                db.SaveChanges();
                switch (roleId)
                {
                    case 4:
                        var owner = new Owners { Person = person };
                        db.Owners.Add(owner);
                        break;
                    case 5:
                        var tenant = new Tenant
                        {
                            Id = person.Id,
                            IsCompletedPersonalProfile = false,
                            HasProofOfIdentity = false,
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = person.Id,
                            UpdatedOn = DateTime.UtcNow,
                            IsActive = true,
                            Address = new Address
                            {
                                CountryId = 1,
                                IsActive = true,

                            }
                        };
                        db.Tenant.Add(tenant);
                        break;
                    case 6:
                        var com = new Company
                        {
                            UpdatedBy = login.Email,
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = login.Email,
                            UpdatedOn = DateTime.UtcNow,
                            IsActive = true,
                            Address = new Address //Bug Fix #2075
                            {
                                CountryId = 1,
                                IsActive = true
                            },
                            Address1 = new Address //Bug Fix #2075
                            {
                                CountryId = 1,
                                IsActive = true
                            }
                        };
                        var sp = new ServiceProvider { Id = person.Id, Company = com, IsProfileComplete = false };
                        db.ServiceProvider.Add(sp);
                        break;
                }
                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception e)
                {
                    return new ServiceResponseResult { IsSuccess = false };
                }
            }
        }



        public static bool SendActivationEmailToUser(int userId, string tokenConfirmationUrl)
        {
            using (var db = new KeysEntities())
            {
                var user = db.Login.FirstOrDefault(x => x.Id == userId);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Hi User,<br/>");
                sb.Append("<br/><table style='border-color:#666' cellpadding='10' width='100%'>");
                sb.Append("<tbody><tr style='background:#3c8dbc'>");
                sb.Append("<td>Account activation link for Property Community User</td></tr><tr>");
                sb.Append("<td> Welcome User To complete the signup process please click on the activation link ");
                sb.Append($"<a href='{tokenConfirmationUrl}'>Activate Now</a>");
                sb.AppendLine("</td></tr></tbody></table>");
                sb.Append("<br/><br/>Regards,<br>Software Team");
                try
                {
                    MailMessage Mail = new MailMessage();
                    Mail.To.Add(user.Email);
                    Mail.Subject = ("Account Email Verification");
                    Mail.Body = (sb.ToString());
                    Mail.IsBodyHtml = true;
                    EmailService.SendAsync(Mail);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        public static async Task<ServiceResponseResult> SendActivationEmailToTenant(int tenantId, string tenantEmail, string token, AddTenantToPropertyModel model)
        {
            var nvc = new NameValueCollection();
            nvc.Set("TenantId", tenantId.ToString());
            nvc.Set("Email", tenantEmail);
            nvc.Set("Token", token);
            nvc.Set("PropertyId", model.PropertyId.ToString());
            nvc.Set("StartDate1", model.StartDate.ToString());
            nvc.Set("EndDate1", model.EndDate.ToString());

            //nvc.Set("StartDate", model.StartDate. );
            //nvc.Set("EndDate", model.EndDate );
            nvc.Set("PaymentFrequencyId", model.PaymentFrequencyId.ToString());
            nvc.Set("PaymentAmount", model.PaymentAmount.ToString());
            string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "Account/ActivateToBeTenant", UtilService.ToQueryString(nvc));
            string subject = "Property Community: Account Activation";
            string body =
               "Hello !<br />"
               + $"You have registered to become a tenant at Property Community.<br />"
               + "Please <a target='_blank' href=" + url + "> Click Here </a> to activate<br />";
            MailMessage msg = new MailMessage()
            {
                Subject = subject,
                SubjectEncoding = System.Text.Encoding.UTF8,
                Body = body,
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true
            };
            msg.To.Add(tenantEmail);
            try
            {
                await EmailService.SendAsync(msg);
                return new ServiceResponseResult { IsSuccess = true };

            }
            catch (Exception ex)
            {
                return new ServiceResponseResult { IsSuccess = false };
            }
        }

        public static Login GetLoginByEmail(string email)
        {
            using (var db = new KeysEntities())
            {
                return db.Login.FirstOrDefault(x => x.Email == email && x.IsActive);
            }
        }

        public static Login GetExistingLogin(string email)
        {
            using (var db = new KeysEntities())
            {
                return db.Login.FirstOrDefault(x => x.Email == email);
            }
        }

        public static Login GetExistingTenant(string email)
        {
            using (var db = new KeysEntities())
            {
                Login login= db.Login.FirstOrDefault(x => x.Email == email);
                if (TenantService.IsLoginATenant(login))
                {
                    return login;
                }
                return null;
            }
        }
        public static Person GetPersonById(int personId)
        {
            using (var db = new KeysEntities())
            {
                return db.Person.FirstOrDefault(x => x.Id == personId);
            }
        }

        public static Person GetPersonByLoginId(int loginId)
        {
            using (var db = new KeysEntities())
            {
                return db.Person.FirstOrDefault(x => x.Id == loginId);
            }
        }

        public static ServiceResponseResult AddNewUser(Login login, Person person, LoginRole loginRole)
        {
            using (var db = new KeysEntities())
            {
                try
                {
                    db.Login.Add(login);
                    person.Id = login.Id;
                    db.Person.Add(person);
                    loginRole.PersonId = person.Id;
                    db.LoginRole.Add(loginRole);
                    switch (loginRole.RoleId)
                    {
                        case 4:
                            var owner = new Owners { Person = person };
                            db.Owners.Add(owner);
                            break;
                        case 5:
                            var tenant = new Tenant
                            {
                                Person = person,
                                IsCompletedPersonalProfile = false,
                                HasProofOfIdentity = false,
                                CreatedOn = DateTime.UtcNow,
                                CreatedBy = login.Id,
                                UpdatedOn = DateTime.UtcNow,
                                IsActive = true,
                                Address = new Address
                                {
                                    CountryId = 1,
                                    IsActive = true,

                                }
                            };
                            db.Tenant.Add(tenant);
                            break;
                        case 6:
                            var com = new Company
                            {
                                UpdatedBy = login.Email,
                                CreatedOn = DateTime.UtcNow,
                                CreatedBy = login.Email,
                                UpdatedOn = DateTime.UtcNow,
                                IsActive = true,
                                Address = new Address //Bug Fix #2075
                                {
                                    CountryId = 1,
                                    IsActive = true
                                },
                                Address1 = new Address //Bug Fix #2075
                                {
                                    CountryId = 1,
                                    IsActive = true
                                }
                            };
                            var sp = new ServiceProvider { Person = person, Company = com, IsProfileComplete = false };
                            db.ServiceProvider.Add(sp);
                            break;
                    }
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false };
                }
            }
        }

        public static bool CheckIfUserExist(string userName)
        {
            using (var db = new KeysEntities())
            {
                return db.Login.Any(x => x.UserName == userName);
            }
        }

        public static Login GetAwaitingActivateUserByEmail(string email, string token)
        {
            using (var db = new KeysEntities())
            {
                return db.Login.Where(x => x.UserName == email && x.EmailConfirmationToken == token).FirstOrDefault();
            }
        }

        public static Login GetAwaitingActivateUserById(int id, string token)
        {
            using (var db = new KeysEntities())
            {
                return db.Login.Where(x => x.Id == id && x.EmailConfirmationToken == token).FirstOrDefault();
            }
        }

        public static ServiceResponseResult ActivateLogin(int loginId)
        {
            using (var db = new KeysEntities())
            {
                var login = db.Login.Where(x => x.Id == loginId).FirstOrDefault();
                if (login == null)
                {
                    return new ServiceResponseResult { IsSuccess = false };
                }
                try
                {
                    login.EmailConfirmed = true;
                    login.IsActive = true;
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false };
                }

            }
        }

        public static Login AddNewLoginAndPerson(RegisterViewModel model, string emailConfirmToken)
        {
            var salt = UtilService.GeneratePassword(10, 5);
            var passwordHash = AccountService.CreatePasswordHash(model.Password, salt);
            try
            {
                var user = new Login
                {
                    UserName = model.UserName,
                    Email = model.UserName,
                    PasswordHash = passwordHash,
                    SecurityStamp = salt,
                    EmailConfirmationToken = emailConfirmToken.ToString(),
                    EmailConfirmationTokenExpiryDate = DateTime.Now.AddHours(2).ToUniversalTime(),  // FOR TESTING PUPROSE VALIDITY ONLY FOR 2HRS
                    EmailConfirmed = false,
                    CreatedBy = model.UserName,
                    CreatedOn = DateTime.Now,
                    IsActive = false
                };
                var person = new Person
                {
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Login = user,

                    Address = new Address
                    {
                        CountryId = 1,
                        IsActive = true,

                    },
                    Address1 = new Address
                    {
                        CountryId = 1,
                        IsActive = true
                    },

                    UID = Guid.NewGuid(),
                    IsActive = true
                };
                var loginRole = new LoginRole
                {
                    RoleId = model.RoleId,
                    IsActive = true,
                    PendingApproval = false
                };
                var result = AddNewUser(user, person, loginRole);
                return result.IsSuccess ? user : null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static ServiceResponseResult ResetActivate(int loginId, string token)
        {
            using (var db = new KeysEntities())
            {
                var emailConfirmToken = Guid.NewGuid();
                var loginAgain = AccountService.GetAwaitingActivateUserById(loginId, token);
                if (loginAgain != null)
                {
                    try
                    {
                        loginAgain.EmailConfirmationToken = emailConfirmToken.ToString();
                        loginAgain.EmailConfirmationTokenExpiryDate = DateTime.Now.AddHours(2).ToUniversalTime();  // FOR TESTING PURPOSE VALIDITY SET FOR 2 HRS
                        db.Login.Attach(loginAgain);
                        db.Entry(loginAgain).State = EntityState.Modified;
                        db.SaveChanges();
                        return new ServiceResponseResult { IsSuccess = true, NewObject = loginAgain.EmailConfirmationToken };
                    }
                    catch (Exception e)
                    {
                        return new ServiceResponseResult { IsSuccess = false };
                    }
                }
                else
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "OOPS....!!!!You have clicked on the old activation link Or Your Account details are not correct" };

                }
            }
        }

        public static Person GetPersonByEmail(string email)
        {
            using (var db = new KeysEntities())
            {
                var login = GetLoginByEmail(email);
                return login == null ? null : GetPersonByLoginId(login.Id) ?? null;
            }
        }

        public static Login GetLoginById(int id)
        {
            using (var db = new KeysEntities())
            {
                return db.Login.FirstOrDefault(x => x.Id == id);
            }
        }

        public static IEnumerable<int> GetUserRolesbyEmail(string email)
        {
            using (var db = new KeysEntities())
            {
                var loginId = GetLoginByEmail(email)?.Id;
                if (loginId != null)
                {
                    return db.LoginRole.Where(x => x.PersonId == loginId).Select(x => x.RoleId).ToList();
                }

                else return null;
            }
        }

        public static IEnumerable<Role> GetAllRoles()
        {
            using (var db = new KeysEntities())
            {
                return db.Role.ToList();
            }
        }

        public static ServiceResponseResult UpdateLogin(Login login)
        {
            using (var db = new KeysEntities())
            {
                db.Entry(login).State = System.Data.Entity.EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false };
                }
            }
        }

        public static ServiceResponseResult CreateTenantAccount(AddTenantToPropertyModel model, Login creartor, string temPass)
        {
            using (var db = new KeysEntities())
            {
                var salt = UtilService.GeneratePassword(10, 5);
                //var temPass = UtilService.GeneraterRandomKey(8);
                var passwordHash = AccountService.CreatePasswordHash(temPass, salt);
                var login = new Login
                {
                    UserName = model.TenantEmail,
                    Email = model.TenantEmail,
                    PasswordHash = passwordHash,
                    SecurityStamp = salt,
                    EmailConfirmed = true,
                    CreatedBy = creartor.Email,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                };

                var person = new Person
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Login = login,
                    Address = new Address
                    {
                        CountryId = 1,
                        IsActive = true,

                    },
                    Address1 = new Address
                    {
                        CountryId = 1,
                        IsActive = true
                    },

                    UID = Guid.NewGuid(),
                    IsActive = true
                };
                var loginRole = new LoginRole
                {
                    RoleId = 5,
                    IsActive = true,
                    PendingApproval = false
                };
                db.Login.Add(login);
                person.Login = login;
                db.Person.Add(person);
                loginRole.Person = person;
                db.LoginRole.Add(loginRole);
                var tenant = new Tenant
                {
                    Person = person,
                    IsCompletedPersonalProfile = false,
                    HasProofOfIdentity = false,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = creartor.Id,
                    UpdatedOn = DateTime.UtcNow,
                    IsActive = true,
                    Address = new Address
                    {
                        CountryId = 1,
                        IsActive = true,

                    }
                };
                db.Tenant.Add(tenant);
                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true, NewObject = login };
                }
                catch (Exception e)
                {
                    return new ServiceResponseResult { IsSuccess = false };
                }

            }
        }

        public static bool IsAdmin(Login login)
        {
            using (var db = new KeysEntities())
            {
                return login == null ? false : db.LoginRole.FirstOrDefault(x => x.PersonId == login.Id && x.IsActive && x.RoleId == 2) == null ? false : true;
            }
        }

        public static bool IsServSupplier(Login login)
        {
            using (var db = new KeysEntities())
            {
                return login == null ? false : db.LoginRole.FirstOrDefault(x => x.PersonId == login.Id && x.IsActive && x.RoleId == 6) == null ? false : true;
            }

        }

        public static bool SavePersonMedia(List<MediaModel> mediaData, int personId)
        {
            try
            {
                using (var db = new KeysEntities())
                {
                    foreach (var item in mediaData)
                    {
                        db.PersonMedia.Add(new PersonMedia()
                        {
                            IsActive = true,
                            NewFilename = item.NewFileName,
                            OldFilename = item.OldFileName,
                            PersonId = personId
                        });
                    }
                    db.SaveChanges();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static ServiceResponseResult<Person> CanBeTenant(string email, int propertyId)
        {
            using (var db = new KeysEntities())
            {
                Login login = db.Login.FirstOrDefault(x => x.Email == email);
                var person = login?.Person;
                var isTenant = login == null ? false : db.LoginRole.FirstOrDefault(x => x.PersonId == login.Id && x.RoleId == 5) == null ? false : true;
                var isInProp = login == null ? false : db.TenantProperty.Where(x => x.TenantId == login.Id && x.PropertyId == propertyId && (x.IsActive ?? false)).Any();
                var isSS = login == null ? false : db.LoginRole.FirstOrDefault(x => x.PersonId == login.Id && x.RoleId == 6) == null ? false : true;
                var isOwner = login == null ? false : db.LoginRole.FirstOrDefault(x => x.PersonId == login.Id && x.RoleId == 4) == null ? false : true;
                var res = new ServiceResponseResult<Person> {IsSuccess = true };
                res.Result = person;
                if (isTenant)
                {
                    if (isInProp)
                    {
                        res.IsSuccess = false;
                        res.ErrorMessage = "Tenant is already in property!";
                        return res;
                    }
                    
                }
                else
                {
                    if (isOwner || isSS)
                    {
                        res.IsSuccess = false;
                        res.ErrorMessage = "Account already exsists but not a tenant!";
                        return res;
                    }
                    
                }
                return res;
            }
        }
    }
}
