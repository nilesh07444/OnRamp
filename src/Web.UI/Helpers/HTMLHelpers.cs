
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Web.UI.Helpers
{
    public static class HTMLHelpers
    {
        
        public static IHtmlString ActionLinkWithAbsoluteUrl(this HtmlHelper htmlHelper, string linkText, string action, string controller, 
        object routeValues, object htmlAttributes)
        {
            TagBuilder tagBuilder;
            UrlHelper urlHelper;

            urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            tagBuilder = new TagBuilder("a");

            tagBuilder.InnerHtml = linkText;

            var relativeURl = "~" + urlHelper.Action(action, controller, routeValues);

            tagBuilder.Attributes["href"] = AppSettings.Urls.ResolveUrl(relativeURl);

            return MvcHtmlString.Create(tagBuilder.ToString());
        }

       
        public static IHtmlString ActionLinkWithAbsoluteUrl(this HtmlHelper htmlHelper, string linkText, string action, string controller, 
        object routeValues)
        {
            TagBuilder tagBuilder;
            UrlHelper urlHelper;

            urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            tagBuilder = new TagBuilder("a");

            tagBuilder.InnerHtml = linkText;

            var relativeURl = "~" + urlHelper.Action(action, controller, routeValues);

            tagBuilder.Attributes["href"] = AppSettings.Urls.ResolveUrl(relativeURl);

            tagBuilder.MergeAttributes(new RouteValueDictionary());

            return MvcHtmlString.Create(tagBuilder.ToString());
        }
        public static string ActionWithAbsoluteUrl(this UrlHelper UrlHelper, string action, string controller,
        object routeValues)
        {
            return AppSettings.Urls.ResolveUrl(UrlHelper.Action(action, controller, routeValues));
        }

        public static string ActionWithAbsoluteUrl(this UrlHelper UrlHelper, string relativeUrl)
        {
            return AppSettings.Urls.ResolveUrl(relativeUrl);
        }
    }
}