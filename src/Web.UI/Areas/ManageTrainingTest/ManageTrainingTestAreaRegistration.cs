using System.Web.Mvc;

namespace Web.UI.Areas.ManageTrainingTest
{
    public class ManageTrainingTestAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ManageTrainingTest";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ManageTrainingTest_default",
                "ManageTrainingTest/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}