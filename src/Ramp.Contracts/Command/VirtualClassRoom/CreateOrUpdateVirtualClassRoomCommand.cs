using Common.Data;
using System;


namespace Ramp.Contracts.Command.VirtualClassRoom {
	public class CreateOrUpdateVirtualClassRoomCommand : IdentityModel<string> {
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
		public string JitsiServerName { get; set; }
	}
}
