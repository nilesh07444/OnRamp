
using Common.Query;
using Ramp.Contracts.ViewModel;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Web.UI.Code.Extensions;

namespace Web.UI.Controllers
{
    public class CalendarController  : RampController {
        // GET: Calendar
        public ActionResult Index()
        {
			string id = Thread.CurrentPrincipal.GetId().ToString();
			string filter = "admin";

			if (!Thread.CurrentPrincipal.IsInAdminRole())
			{
				filter = "su";
			}
			

			CalendarData docs = new CalendarData();

			var x  = ExecuteQuery<FetchAllQuery, CalendarData> (new FetchAllQuery()
			{
				Id = id,
				Filters = filter
			});

			return View(x);
        }
    }
}