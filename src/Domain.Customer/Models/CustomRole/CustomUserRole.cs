using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.CustomRole
{
	public class CustomUserRoles : Base.CustomerDomainObject
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Title { get; set; }

		//public string Permissions { get; set; }
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
		public bool IsActive { get; set; }
		public bool IsDeleted { get; set; }

		public DateTime? DateCreated { get; set; }
		public DateTime? DateEdited { get; set; }
		public DateTime? DateDeleted { get; set; }
	}
}
