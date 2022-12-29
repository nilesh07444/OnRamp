using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Web.UI.Code.Extensions;

namespace Web.UI.Code.Extensions
{
    public class DomainRoute : Route
    {
        public string Suffix { get; set; }

        public DomainRoute(string suffix, string url, RouteValueDictionary defaults)
            : base(url, defaults, new MvcRouteHandler())
        {
            Suffix = suffix;
        }

        public DomainRoute(string suffix, string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(url, defaults, routeHandler)
        {
            Suffix = suffix;
        }

        public DomainRoute(string suffix, string url, object defaults)
            : base(url, new RouteValueDictionary(defaults), new MvcRouteHandler())
        {
            Suffix = suffix;
        }

        public DomainRoute(string suffix, string url, object defaults, IRouteHandler routeHandler)
            : base(url, new RouteValueDictionary(defaults), routeHandler)
        {
            Suffix = suffix;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var data = base.GetRouteData(httpContext);

            if (string.IsNullOrEmpty(Suffix))
                return data;

            var url = httpContext.Request.Headers["HOST"];

            var suffixLowered = Suffix.ToLower();
            url = url.ToLower();

            if (url.Contains(suffixLowered))
            {
                var match = url.Replace(suffixLowered, string.Empty);
                match = match.Replace(".", string.Empty);

                if (!string.IsNullOrEmpty(match))
                    if (data != null)
                        data.Values["subdomain"] = match;
            }

            return data;
        }

        public DomainData GetDomainData(RequestContext requestContext, RouteValueDictionary values)
        {
            // Build hostname
            object subdomain = "www";

            if (values.ContainsKey("subdomain"))
                subdomain = values["subdomain"];

            return new DomainData
            {
                Protocol = "http",
                HostName = string.Format("{0}.{1}", subdomain, Suffix),
                Fragment = ""
            };
        }
    }
}