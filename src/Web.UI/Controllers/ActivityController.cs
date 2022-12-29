using Ramp.Contracts.CommandParameter.ActivityManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Web.UI.Code.ActionFilters;

namespace Web.UI.Controllers
{
    [PortalContextActionFilter]
    public class ActivityController : RampController
    {
        [HttpPost]
        public ActionResult CreateUserDisclaimerActivity(CreateUserDiclaimerActivityLogEntryCommand command)
        {
            command.IPAddress = Request.UserHostAddress;
            var result = ExecuteCommand(command);
            if(!result.Validation.Any())
                return new HttpStatusCodeResult(HttpStatusCode.Accepted);
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
    }
}