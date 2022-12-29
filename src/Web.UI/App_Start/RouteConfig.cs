using System.Web.Mvc;
using System.Web.Routing;
using Web.UI.Code.Extensions;

namespace Web.UI
{
    public class RouteConfigs
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            // AreaRegistration.RegisterAllAreas();
            routes.MapMvcAttributeRoutes();

            routes.Add("DomainRoute2", new DomainRoute(
                System.Configuration.ConfigurationManager.AppSettings["DomainSuffix"], // Domain with parameters
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Account", action = "Login", id = "", subdomain = "" } // Parameter defaults
                ));


            routes.MapRoute(
            name: "profile",
             url: "{controller}/{action}/{id}",
            defaults: new { controller = "EditMyProfile", action = "Account", id = UrlParameter.Optional }
     );

			//routes.MapRoute(
			//   "Workbook", // Route name
			//   "Workbook/{action}/{id}", // URL with parameters
			//   new { controller = "CheckList", action = "Edit", id = UrlParameter.Optional } // Parameter defaults
			//   );







		}
    }
}