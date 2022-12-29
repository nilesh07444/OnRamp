using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel {
	public class AddCustomUserRoleViewModel: IViewModel {
		//public AddCustomUserRoleViewModel()
		//{
		//	RoleName = null;
		//	ContentCreator = false;
		//	ContentApprover = false;
		//	ContentAdmin = false;
		//	PortalAdmin = false;
		//	Publisher = false;
		//	Reporter = false;
		//	UserAdmin = false;
		//	ManageTags = false;
		//	ManageVirtualMeetings = false;
		//}
		public string Id { get; set; }
		public string RoleName { get; set; }

		public bool StandardUser { get; set; }
		public bool ContentCreator { get; set; }
		public bool ContentApprover { get; set; }
		public bool ContentAdmin { get; set; }
		public bool PortalAdmin { get; set; }
		public bool Publisher { get; set; }
		public bool Reporter { get; set; }
		public bool UserAdmin { get; set; }
		public bool ManageTags { get; set; }
		public bool ManageVirtualMeetings { get; set; }
		public bool ManageAutoWorkflow { get; set; }
		public bool ManageReportSchedule { get; set; }

	}
}
