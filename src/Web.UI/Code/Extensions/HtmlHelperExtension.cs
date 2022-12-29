using Newtonsoft.Json;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Web.UI.App_Start;
using ExpressionHelper = Microsoft.Web.Mvc.Internal.ExpressionHelper;

namespace Web.UI.Code.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString ActionLinkFor<TController>(this HtmlHelper helper, string linkText,
            Expression<Action<TController>> action,
            object extraRouteValues = null,
            object htmlAttributes = null)
            where TController : Controller
        {
            RouteValueDictionary routeValuesFromExpression = ExpressionHelper.GetRouteValuesFromExpression(action);

            routeValuesFromExpression.Mergewith(extraRouteValues);

            return new MvcHtmlString(
                HtmlHelper.GenerateLink(
                    helper.ViewContext.RequestContext,
                    helper.RouteCollection,
                    linkText,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    routeValuesFromExpression,
                    new RouteValueDictionary(htmlAttributes)));
        }
        public static void RenderActionFor<TController>(this HtmlHelper helper, Expression<Action<TController>> action,
            object extraRouteValues = null)
            where TController : Controller
        {
            RouteValueDictionary routeValuesFromExpression = ExpressionHelper.GetRouteValuesFromExpression(action);

            string actionName = routeValuesFromExpression["Action"].ToString();
            string controllerName = routeValuesFromExpression["Controller"].ToString();

            helper.RenderAction(actionName, controllerName, routeValuesFromExpression);
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

        public static bool CanAccess<TController>(this HtmlHelper helper, Expression<Action<TController>> action) where TController : Controller
        {
            var routeValuesFromExpression = ExpressionHelper.GetRouteValuesFromExpression(action);
            string actionName = routeValuesFromExpression["Action"].ToString();
            string controllerName = routeValuesFromExpression["Controller"].ToString() + "Controller";
            string area = RampSecurity.findAreaForControllerType(typeof(TController));
            if (RampSecurity.RampSecurityRoles.ContainsKey(RampSecurity.MakeKey(area, controllerName, actionName)))
            {
                var authorizedRoles =
                        RampSecurity.RampSecurityRoles[RampSecurity.MakeKey(area, controllerName, actionName)];

                foreach (var userRole in SessionManager.GetCustomerRolesOfCurrentlyLoggedInUser())
                {
                    if (authorizedRoles.Any(r => r.Equals(userRole)))
                        return true;
                }
                return false;
            }
            return true;
        }

        public static MvcHtmlString AutoCompleteFor<T>(this HtmlHelper<T> helper, string controlIdentifier,bool typeahead,bool tags, params AutoCompleteSection[] sections)
        {
            return helper.Partial("_AutoCompleteScripts", new AutoCompleteModel(controlIdentifier, sections) { Tags = tags, Typeahead = typeahead });
        }

        public static string IsActive(this HtmlHelper helper, string action, string controller)
        {
            var routeData = helper.ViewContext.RouteData;

            var routeAction = (string) routeData.Values["action"];
            var routeController = (string) routeData.Values["controller"];

            return controller == routeController &&
                   action == routeAction
                ? "active"
                : string.Empty;
        }
    }    
}