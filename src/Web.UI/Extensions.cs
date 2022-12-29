using System;
using Common.Query;
using Domain.Models;
using Newtonsoft.Json;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.Portal;
using Ramp.Contracts.ViewModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using Domain.Enums;

namespace Web.UI
{
    public static class Extensions
    {


        public static IList<Web.UI.Controllers.RampController.Notification> PopNotifications<T>(this HtmlHelper<T> helper, string context = "")
        {
            var notifications = (IList<Web.UI.Controllers.RampController.Notification>)helper.ViewContext.HttpContext.Session["Notifications"];
            if (notifications == null)
                return new List<Web.UI.Controllers.RampController.Notification>();

            var n = notifications.Where(c => c.Context == context).ToArray();
            n.ToList().ForEach(c => notifications.Remove(c));
            return n;
        }

        public static string GetBootstrapType(this Web.UI.Controllers.RampController.Notification notification)
        {
            var type = notification.Type;
            if (type.ToLower() == "error")
                type = "danger";

            return type;
        }

        public static string AsUrl(this string url)
        {
            if (!url.StartsWith("http"))
                url = "http://" + url;

            return url;
        }

        public static string AsUrlSecure(this string url)
        {
            if (!url.StartsWith("http"))
                url = "http://" + url;

            return url;
        }

        public static string RelativeImagePath(this PortalContextViewModel vm)
        {
            if (vm != null)
            {
                if (vm.Type.HasValue)
                {
                    if (vm.Type.Value == CompanyType.CustomerCompany)
                        return string.Format("~/LogoImages/CustomerLogo/{0}", vm.LogoFileName);
                    else
                        return string.Format("~/LogoImages/ProvisionalLogo/{0}", vm.LogoFileName);
                }

                return string.Format("~/Content/images/{0}", vm.LogoFileName);
            }
            return string.Empty;
        }

        public static PortalContextViewModel GetPortalContext<T>(this HtmlHelper<T> helper)
        {
            var context = helper.ViewContext.Controller.GetPortalContext();

            if (context == null)
            {
                context = new PortalContextViewModel
                {
                    Name = "Admin",
                    Reseller = string.Empty,
                    LogoFileName = "logo.png"
                };
            }

            return context;
        }

        public static PortalContextViewModel GetPortalContext(this ControllerBase context)
        {
            var portalName = context.GetRequestedPortalName();

            if (!string.IsNullOrEmpty(portalName))
            {
                if (!portalName.Equals("admin", StringComparison.InvariantCultureIgnoreCase))
                {
                    var queryExecutor = DependencyResolver.Current.GetService<IQueryExecutor>();
                    var portalContext =
                        queryExecutor.Execute<PortalQueryParameter, PortalContextViewModel>(new PortalQueryParameter
                        {
                            Subdomain = portalName
                        });

                    return portalContext;
                }
                else
                    return new PortalContextViewModel()
                    {
                        UserCompany = new CompanyViewModel
                        {
                            CompanyName = "Administration",
                            Id = Guid.Empty,
                            ClientSystemName = "admin",
                            LayerSubDomain = "admin"
                        },
                        Name = "Administration",
                        Reseller = "",
                        LogoFileName = "logo.png",
                        Type = null
                    };
            }
            return null;
        }

        public static string GetRequestedPortalName(this ControllerBase controller)
        {
            var subdomain = string.Empty;

            //if (controller.ControllerContext.RouteData.Values.ContainsKey("subdomain"))
            //    subdomain = controller.ControllerContext.RouteData.Values["subdomain"].ToString();

            var suffix = System.Configuration.ConfigurationManager.AppSettings["DomainSuffix"].ToLower();
            var host = controller.ControllerContext.HttpContext.Request.Url.Host.ToLower();

            if (host.Contains(suffix))
            {
                var match = host.Replace(suffix, string.Empty);
                match = match.Trim(new char[] {'.'});

                if (!string.IsNullOrEmpty(match))
                    subdomain = match;
            }

            return subdomain;
        }

        public static IEnumerable<SelectListItem> AsSelectList<TModel>(this IList<TModel> list,
            Expression<Antlr.Runtime.Misc.Func<TModel, string>> textSelector, Expression<Antlr.Runtime.Misc.Func<TModel, string>> valueSelector)
        {
            return list.Select(model => new SelectListItem { Text = textSelector.Compile().Invoke(model), Value = valueSelector.Compile().Invoke(model) });
        }
    }
}