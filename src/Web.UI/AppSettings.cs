using System;
using System.Configuration;
using com.sun.org.apache.bcel.@internal.generic;
using Ramp.Contracts.ViewModel;
using System.Linq;

namespace Web.UI
{
    public static class AppSettings
    {
        private static string Get(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static string CompanyName
        {
            get { return "OnRamp"; }
        }

        public static class Urls
        {
            public static string ResolveUrl(string url, CompanyViewModel company = null)
            {
                var root = Main;
                if (string.IsNullOrWhiteSpace(root) && company != null)
                    root = BuildMainFromModel(company);
                if (root.Substring(root.Length - 1, 1) != "/") root += "/";
                var newUrl = url.Replace("~", root);
                if (company != null)
                {

                    newUrl = ReplaceSubdomain(newUrl, company.LayerSubDomain);
                }

                return newUrl;
            }

            public static string ReplaceSubdomain(string url, string subdomain)
            {
               var host = new Uri(url).Host;
                var oldSubdomain = host.Replace("."+ DomainSuffix, string.Empty);
                if (oldSubdomain.Split('.').Any())
                    oldSubdomain = oldSubdomain.Split('.').First();
                return url.Replace(host, host.Replace(oldSubdomain, subdomain));
            }
            public static string HeaderImageLogoUrl
            {
                get { return ResolveUrl("~Content/images/topBg.jpg"); }
            }

            public static string FooterImageLogoUrl
            {
                get { return ResolveUrl("~Content/images/logoBg.jpg"); }
            }

            public static string Main
            {
                get
                {
                   return MvcApplication.RootPath;
                }
            }
            public static string BuildMainFromModel(CompanyViewModel model)
            {
                return $"https://{model.LayerSubDomain}.{ConfigurationManager.AppSettings["DomainSuffix"]}/";
            }

            public static string DomainSuffix
            {
                get { return Get("DomainSuffix"); }
            }

            public static string Marketting
            {
                get { return "www." + DomainSuffix; }
            }

            public static string NotifyTestReportJob
            {
                get { return ResolveUrl("~/NotifyTestReportJob"); }
            }
        }
    }
}