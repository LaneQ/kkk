using KeysPlus.Website.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Collections;
using System.Data;
using KeysPlus.Data;
using KeysPlus.Service.Services;

namespace KeysPlus.Website.Models
{
    public class SiteUtil
    {

        public static string NZ_TIMEZONE_NAME = "New Zealand Standard Time";

        public const string DOCUMENT_PATH = @"~/Documents";
        public static int CurrentUserId
        {
            get
            {
                if (HttpContext.Current.Session["CurrentUserId"] == null)
                {
                    ResetCurrentUserSession();
                }

                return (int)HttpContext.Current.Session["CurrentUserId"];
            }
            set
            {
                HttpContext.Current.Session["CurrentUserId"] = value;
            }
        }

        public static string[] UserRoles
        {
            get
            {
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["UserRoles"] == null)
                {
                    ResetUserRolesSession();
                }

                return (string[])HttpContext.Current.Session["UserRoles"];
            }
            set
            {
                if (HttpContext.Current.Session != null)
                {
                    HttpContext.Current.Session["UserRoles"] = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether [current user admin].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current user admin]; otherwise, <c>false</c>.
        /// </value>
        public static bool CurrentUserAdmin
        {
            get
            {
                if (HttpContext.Current.Session["CurrentUserAdmin"] == null)
                {
                    ResetCurrentUserSession();
                }

                // modified for retrieve password page
                return HttpContext.Current.Session["CurrentUserAdmin"] == null ? false : (bool)HttpContext.Current.Session["CurrentUserAdmin"];
            }
            set
            {
                HttpContext.Current.Session["CurrentUserAdmin"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of the current user.
        /// </summary>
        /// <value>
        /// The name of the current user.
        /// </value>
        public static string CurrentUserName
        {
            get
            {
                if (HttpContext.Current.Session["CurrentUserName"] == null)
                {
                   ResetCurrentUserSession();
                }

                return HttpContext.Current.Session["CurrentUserName"].ToString();
            }
            set
            {
                HttpContext.Current.Session["CurrentUserName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the current user.
        /// </summary>
        /// <value>
        /// The name of the current user.
        /// </value>
        public static string CurrentUserDisplayName
        {
            get
            {
                if (HttpContext.Current.Session["CurrentUserDisplayName"] == null)
                {
                    ResetCurrentUserSession();
                }

                // modified for retrieve password page
                return HttpContext.Current.Session["CurrentUserDisplayName"] == null ? string.Empty : HttpContext.Current.Session["CurrentUserDisplayName"].ToString();
            }
            set
            {
                HttpContext.Current.Session["CurrentUserDisplayName"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the current customer id.
        /// </summary>
        /// <value>
        /// The current customer id.
        /// </value>
        public static int CurrentCompanyId
        {
            get
            {
                if (HttpContext.Current.Session["CurrentCompanyId"] == null)
                {
                    ResetCurrentUserSession();
                }

                //// modified for retrieve password page
                return HttpContext.Current.Session["CurrentCompanyId"] == null ? 0 : (int)HttpContext.Current.Session["CurrentCompanyId"];
            }
            set
            {
                HttpContext.Current.Session["CurrentCompanyId"] = value;
            }
        }

        

        /// <summary>
        /// Generates the password.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="nonLetter">The non letter.</param>
        /// <returns></returns>
        //public static string GeneratePassword(int length, int nonLetter)
        //{
        //    return Membership.GeneratePassword(length, nonLetter);
        //}

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="x">The x.</param>
        public static void LogError(Exception x)
        {
            //Elmah.ErrorSignal.FromCurrentContext().Raise(x);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is session loaded.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is session loaded; otherwise, <c>false</c>.
        /// </value>
        public static bool IsSessionValid
        {
            get
            {
                return HttpContext.Current.Session["IsSessionValid"] != null && (bool)HttpContext.Current.Session["IsSessionValid"];
            }
            set
            {
                HttpContext.Current.Session["IsSessionValid"] = value;
            }
        }

        public static void ResetUserRolesSession(string userName = null)
        {
            using (var db = new KeysEntities())
            {
                if (string.IsNullOrEmpty(userName))
                {
                    userName = System.Web.HttpContext.Current.User.Identity.Name;
                }
                var user = db.Login.FirstOrDefault(n => n.UserName == userName && n.IsActive);
                var roles = db.LoginRole.Where(x => x.PersonId == user.Id).Select(x => x.Role.Name).ToArray();
                if (user == null)
                {
                    return;
                }
                SiteUtil.UserRoles = roles;
            }
        }
        public static void ResetCurrentUserSession(string userName = null)
        {
            if (string.IsNullOrEmpty(userName))
            {
                userName = System.Web.HttpContext.Current.User.Identity.Name;
            }

            using (var db = new KeysEntities())
            {
                var user = db.Login.FirstOrDefault(n => n.UserName == userName && n.IsActive);
                var roles = db.LoginRole.Where( x => x.PersonId == user.Id).Select(x => x.Role.Name).ToArray();
                if (user == null)
                {
                    return;
                }
                SiteUtil.IsSessionValid = true;
                SiteUtil.CurrentUserId = user.Id;
                SiteUtil.CurrentUserName = user.UserName;
                SiteUtil.CurrentUserDisplayName = user.UserName;
                SiteUtil.UserRoles = roles;
            }
        }
    }
    public static class StringCipher
    {
        public const string Secret = "9gkck6bxer&z2tu5k!se*9-see_o=bk*9x_@&o9lftmn6l4t0-";

        // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes("tu89geji340t89u2");

        // This constant is used to determine the keysize of the encryption algorithm.
        private const int keysize = 256;

        public static string Encrypt(string plainText, string passPhrase)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                byte[] cipherTextBytes = memoryStream.ToArray();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }


    }
}
