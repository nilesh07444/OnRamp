using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.UI.Code.ActionFilters
{
    public class PortalContextActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var portalName = filterContext.Controller.GetRequestedPortalName();
            var portalContext = filterContext.Controller.GetPortalContext();

            if (PortalContext.Current == null && portalContext != null)
                PortalContext.Current = portalContext;

            if (!string.IsNullOrEmpty(portalName) && PortalContext.Current == null && !AppSettings.Urls.Marketting.StartsWith(portalName))
                filterContext.Result = new RedirectResult(AppSettings.Urls.Marketting.AsUrl());

            base.OnActionExecuting(filterContext);
        }
    }
}