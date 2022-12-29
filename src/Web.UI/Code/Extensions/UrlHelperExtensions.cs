using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;
using Web.UI.Code.Helpers;
using Web.UI.Controllers;

namespace Web.UI.Code.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string ActionLink<TController>(this UrlHelper helper, Expression<Action<TController>> action,
            object extraRouteValues = null) where TController : Controller
        {
            var routeValuesFromExpression = Microsoft.Web.Mvc.Internal.ExpressionHelper.GetRouteValuesFromExpression(action);

            routeValuesFromExpression.Mergewith(extraRouteValues);

            return helper.Action(null, routeValuesFromExpression);
        }

        private static void Mergewith(this RouteValueDictionary dictionary, object values)
        {
            if (values == null)
                return;

            var valueDictionary = new RouteValueDictionary(values);

            foreach (var pair in valueDictionary)
            {
                dictionary[pair.Key] = pair.Value;
            }
        }
        public static string GetImageFromId(this UrlHelper helper,string id,bool? mainContext)
        {
            Guid gId;
            if (Guid.TryParse(id, out gId))
                return helper.Action("Get", "Upload", new { Area = "", id = gId, mainContext = mainContext });
            else
                return string.Empty;
        }
    }
}