using KeysPlus.Data;
using KeysPlus.Service.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Web.Security;

namespace KeysPlus.Service.Services
{
    public static class UtilService
    {
        public static string NZ_TIMEZONE_NAME = "New Zealand Standard Time";

        public const string DOCUMENT_PATH = @"~/Documents";

        public static string UrlGenerator(HttpRequest request, string path, string query = null)
        {
            var url = new UriBuilder();
            url.Scheme = request.Url.Scheme;
            url.Host = request.Url.Host;
            if (url.Host == "localhost")
            {
                url.Port = request.Url.Port;
            }
            url.Path = path;
            if (query != null) url.Query = query;
            return url.ToString();
        }

        public static string ToQueryString(NameValueCollection nvc)
        {
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            return string.Join("&", array);
        }

        public static string GeneratePassword(int length, int nonLetter)
        {
            return Membership.GeneratePassword(length, nonLetter);
        }

        public static String ToAddressString(this Address address)
        {
            return (address.Number ?? "") + " " + (address.Street ?? "") + " " + (address.Suburb ?? "")
                    + " " + (address.City ?? "");
        }

        public static String ToAddressString(this AddressViewModel address)
        {
            return (address.Number ?? "") + " " + (address.Street ?? "") + " " + (address.Suburb ?? "")
                    + " " + (address.City ?? "");
        }

        public static string GeneraterRandomKey(int size)
        {
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[size];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public static RouteValueDictionary AddPageValue(this RouteValueDictionary rvd, int page)
        {
            var _rvd= new RouteValueDictionary(rvd);
            _rvd.Add("Page", page);
            return _rvd;
        }
        public static RouteValueDictionary AddRouteValue(this RouteValueDictionary rvd, string key, object obj)
        {
            var _rvd = new RouteValueDictionary(rvd);
            _rvd.Add(key, obj);
            return _rvd;
        }

        public static void MapFrom<B>(this object a, B b) where B : class, new()
        {
            var properties = typeof(B).GetProperties().Where(property => property.CanRead && property.CanWrite).ToList();
            foreach (var property in properties)
            {
                object value = property.GetValue(b);
                property.SetValue(a, value);
            }
        }

        public static B MapTo<B>(this object element) where B : class, new()
        {
            var type = element.GetType();
            var properties = type.GetProperties()
                .Where(property => property.CanRead && property.CanWrite).ToList();
            B b = new B();
            foreach (var property in properties)
            {
                PropertyInfo propertyInfo = typeof(B).GetProperty(property.Name);
                if (propertyInfo != null)
                {
                    object value = property.GetValue(element);
                    propertyInfo.SetValue(b, value);
                }
            }
            return b;
        }
    }

    public class SearchUtil
    {
        //public string FormatString { get; set; }

        public int CheckDisplayType(string searchString)
        {
            //trim redundant space
            string newString = searchString.Trim();
            //check whether search string starts from *,e,g. **key
            //trim all * and display results ending with key
            if (newString[0] == '*')
            {
                return 1; // 1 means ends with
            }
            //check whether search string ends at *,e,g. key**
            //trim all * and display results starts with key
            else if (newString[newString.Length - 1] == '*') //Bug Fix #2082 : Out of bound array error
            {
                return 2; // 2 means starts with
            }
            else
            {
                return 3; //3 means contains
            }
        }

        public string ConvertString(string searchString)
        {
            //convert search string to lower case and trim redundant space
            searchString = searchString.ToLower().Trim();
            string formatString = searchString;
            int type = CheckDisplayType(searchString);
            //type 1 or 2 means there is(are) * in the input string
            //trim all the * in the string
            if (type <= 2)
            {
                formatString = searchString.Replace('*', ' ').Trim();

            }

            return formatString;
        }


        
    }
}
