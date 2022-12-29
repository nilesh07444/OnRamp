using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Web.Mvc.Html;

namespace Web.UI.Code.Extensions
{
    public static class LinkExtensions
    {
        public static string ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName,
            bool requireAbsoluteUrl)
        {
            return ActionLink(htmlHelper, linkText, actionName, null, new RouteValueDictionary(), new RouteValueDictionary(), requireAbsoluteUrl);
        }

        public static string ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName,
            object routeValues, bool requireAbsoluteUrl)
        {
            return ActionLink(htmlHelper, linkText, actionName, null, new RouteValueDictionary(routeValues), new RouteValueDictionary(), requireAbsoluteUrl);
        }

        public static string ActionLink(this HtmlHelper htmlHelper, string linkText2, string actionName,
            string controllerName, bool requireAbsoluteUrl)
        {
            return ActionLink1(htmlHelper, linkText2, actionName, controllerName, new RouteValueDictionary(), new RouteValueDictionary(), requireAbsoluteUrl);
        }

        public static string ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName,
            RouteValueDictionary routeValues, bool requireAbsoluteUrl)
        {
            return ActionLink(htmlHelper, linkText, actionName, null, routeValues, new RouteValueDictionary(), requireAbsoluteUrl);
        }

        public static string ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName,
            object routeValues, object htmlAttributes, bool requireAbsoluteUrl)
        {
            return ActionLink(htmlHelper, linkText, actionName, null, new RouteValueDictionary(routeValues), new RouteValueDictionary(htmlAttributes), requireAbsoluteUrl);
        }

        public static string ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName,
            RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes, bool requireAbsoluteUrl)
        {
            return ActionLink(htmlHelper, linkText, actionName, null, routeValues, htmlAttributes, requireAbsoluteUrl);
        }



        public static string ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName,
             string controllerName, object routeValues, object htmlAttributes, bool requireAbsoluteUrl)
        {
            return ActionLink1(htmlHelper, linkText, actionName, controllerName, new RouteValueDictionary(routeValues), new RouteValueDictionary(htmlAttributes), requireAbsoluteUrl);
        }

        public static string ActionLink1(this HtmlHelper htmlHelper, string linkText, string actionName,
            string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes,
            bool requireAbsoluteUrl)
        {
            if (requireAbsoluteUrl)
            {
                HttpContextBase currentContext = new HttpContextWrapper(HttpContext.Current);
                RouteData routeData = RouteTable.Routes.GetRouteData(currentContext);

                routeData.Values["controller"] = controllerName;
                routeData.Values["action"] = actionName;

                var domainRoute = routeData.Route as DomainRoute;
                if (domainRoute != null)
                {
                    DomainData domainData = domainRoute.GetDomainData(new RequestContext(currentContext, routeData),
                        routeData.Values);
                    return htmlHelper.ActionLink(linkText, actionName, controllerName, domainData.Protocol,
                        domainData.HostName, domainData.Fragment, routeData.Values, null).ToString();
                }
            }
            return null;//htmlHelper.ActionLink(linkText, actionName,controllerName,routeValues.Values,htmlAttributes,true);
        }
    }
}