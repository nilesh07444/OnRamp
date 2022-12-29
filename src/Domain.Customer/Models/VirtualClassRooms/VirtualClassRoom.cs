
using System;

namespace Domain.Customer.Models.VirtualClassRooms {
	public class VirtualClassRoom : Base.CustomerDomainObject {
		public string VirtualClassRoomName { get; set; }
		public string Description { get; set; }
		public bool IsPasswordProtection { get; set; }
		public string Password { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime CreatedOn { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? LastEditDate { get; set; }
		public string LastEditedBy { get; set; }
		public bool Deleted { get; set; }
		public bool IsPublicAccess { get; set; }
		public string ReferenceId { get; set; }
		public string JitsiServerName { get; set; }

	}
}
