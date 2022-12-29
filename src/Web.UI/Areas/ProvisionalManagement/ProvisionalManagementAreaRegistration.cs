using System.Web.Mvc;

namespace Web.UI.Areas.ProvisionalManagement
{
    public class ProvisionalManagementAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ProvisionalManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ProvisionalManagement_default",
                "ProvisionalManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}