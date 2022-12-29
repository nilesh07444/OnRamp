using System;

namespace Ramp.Contracts.ViewModel {
	public class VirtualClassModel {
		public Guid Id { get; set; }
		public string VirtualClassRoomName { get; set; } = string.Empty;
		public string Description { get; set; }
		public bool IsPasswordProtection { get; set; }
		public string Password { get; set; } = string.Empty;
		public string StartDate { get; set; }
		public string EndDate { get; set; }
		public DateTime CreatedOn { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? LastEditDate { get; set; }
		public string LastEditedBy { get; set; }
		public bool Deleted { get; set; }
		public string ReferenceId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public bool IsPublicAccess { get; set; }
		public string JitsiServerName { get; set; }
		public string JoinMeetingLink { get; set; }
		public string JoinPublicLink { get; set; }
		public string Status { get; set; }
		public string StatusClass { get; set; }
	}
}
