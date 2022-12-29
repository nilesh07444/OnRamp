using System.Web.Mvc;
using System.Web.Routing;

namespace Web.UI.Areas.ManageTrainingGuides
{
    public class ManageTrainingGuidesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ManageTrainingGuides";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(null, "PlayBook/{id}",
                new { area = "ManageTrainingGuides", controller = "ManageTrainingGuides", action = "PreviewByReferenceId" });

            context.MapRoute(
                "ManageTrainingGuides_default",
                "ManageTrainingGuides/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}