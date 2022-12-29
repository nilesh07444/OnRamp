using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Ramp.Services.Helpers
{
    public static class ViewToStringConverter
    {
        public static string Convert(string viewPath, object viewModel = null)
        {
            var controller = CreateController();

            return RenderViewToString(controller.ControllerContext, viewPath, viewModel);
        }

        private static string RenderViewToString(ControllerContext context, string viewPath, object model = null, bool partial = true)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(context, viewPath, null);

            if (viewEngineResult == null)
                throw new FileNotFoundException("View cannot be found.");

            // get the view and attach the model to view data
            var view = viewEngineResult.View;
            context.Controller.ViewData.Model = model;

            string result = null;

            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(context, view,
                                            context.Controller.ViewData,
                                            context.Controller.TempData,
                                            sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }

        private static GenericController CreateController(RouteData routeData = null)
        {
            // create a disconnected controller instance
            var controller = new GenericController();

            // get context wrapper from HttpContext if available
            HttpContextBase wrapper;
            if (HttpContext.Current != null)
                wrapper = new HttpContextWrapper(System.Web.HttpContext.Current);
            else
                throw new InvalidOperationException(
                    "Can't create Controller Context if no " +
                    "active HttpContext instance is available.");

            if (routeData == null)
                routeData = new RouteData();

            // add the controller routing if not existing
            if (!routeData.Values.ContainsKey("controller") &&
                !routeData.Values.ContainsKey("Controller"))
                routeData.Values.Add("controller",
                                     controller.GetType()
                                               .Name.ToLower().Replace("controller", ""));

            controller.ControllerContext = new ControllerContext(wrapper, routeData, controller);
            return controller;
        }
    }

    public class GenericController : Controller
    {
    }
}