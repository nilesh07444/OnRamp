using Common.Query;
using Domain.Customer;
using Ramp.Contracts.Command.DocumentUsage;
using Ramp.Contracts.ViewModel;
using System;
using System.Threading;
using System.Web.Mvc;
using Web.UI.Code.Extensions;

namespace Web.UI.Controllers {
	public class MeetingController : RampController
    {
        // GET: Meeting
        public ActionResult Index()
        {
            return View();
        }

		/// <summary>
		/// this is used to join the meeting
		/// </summary>
		/// <param name="id">this is virtual classroom id</param>
		/// <returns></returns>
		public ActionResult VirtualMeeting(string id) {

			ExecuteCommand(new CreateOrUpdateDocumentUsageCommand {
				DocumentId = id.ToString(),
				DocumentType = DocumentType.VirtualClassRoom,
				UserId = Thread.CurrentPrincipal.GetId().ToString(),
				ViewDate = DateTime.UtcNow
			});
			var model = ExecuteQuery<FetchByIdQuery, VirtualClassModel>(new FetchByIdQuery { Id = id }) ?? new VirtualClassModel();
			ViewData["JitsiServerName"] = string.IsNullOrEmpty(model.JitsiServerName) ? "meet.jit.si" : PortalContext.Current.UserCompany.JitsiServerName;
			return View(model);
		}
	}
}