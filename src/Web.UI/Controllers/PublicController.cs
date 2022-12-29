using Common.Query;
using Ramp.Contracts.ViewModel;
using System.Web.Mvc;

namespace Web.UI.Controllers {

	public class PublicController : RampController {
		[AllowAnonymous]
		public ActionResult Meeting(string id) {
			var model = ExecuteQuery<FetchByIdQuery, VirtualClassModel>(new FetchByIdQuery { Id = id }) ?? new VirtualClassModel();

			ViewData["JitsiServerName"] = string.IsNullOrEmpty(model.JitsiServerName) ? "meet.jit.si" : PortalContext.Current.UserCompany.JitsiServerName;

			return View(model);
		}
	}
}