using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Utility;

namespace Web.UI.Code
{
    public static class Etag
    {
        static string GenerateETag(DateTime lastModified, DateTime now)
        {
            if (lastModified.Year < 2000)
                lastModified = DateTime.Now.AddYears(-10).ToUniversalTime(); // avoid out of range issues.

            long num = lastModified.ToFileTime();
            long num2 = now.ToFileTime();
            string str = num.ToString("X8", CultureInfo.InvariantCulture);
            if ((num2 - num) <= 0x1c9c380L)
            {
                return ("W/\"" + str + "\"");
            }
            return ("\"" + str + "\"");
        }

        private static string GetETag(string url)
        {
            var md5Hasher = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(url));
            return ("W/\"" + Convert.ToBase64String(data) + "\"");
        }


        public static bool IsCached(string absoluteLocation)
        {
            // you need to clear the headers that ASP.NET automatically adds
            HttpContext.Current.Response.ClearHeaders();

            // now get the etag (hash the 
            var etag = GetETag(absoluteLocation);

            // see if the etag matches what was sent
            var requestedETag = HttpContext.Current.Request.Headers["If-None-Match"];
            if (requestedETag == etag)
                return true;

            // otherwise set cacheability and etag.
            HttpContext.Current.Response.Cache.SetValidUntilExpires(true);
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
            HttpContext.Current.Response.Cache.SetExpires(DateTime.Now.AddMonths(1));
            HttpContext.Current.Response.Cache.SetLastModified(DateTime.UtcNow);
            HttpContext.Current.Response.Cache.SetETag(etag);

            return false;
        }
    }
}