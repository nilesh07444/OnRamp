using System;
using System.ComponentModel;

namespace Ramp.Contracts.ViewModel {
	public class DocumentNotificationViewModel {
		public int Id { get; set; }
		public DateTime AssignedDate { get; set; }
		public bool IsViewed { get; set; }
		public string DocId { get; set; }
		public string UserId { get; set; }
		public string NotificationType { get; set; }
		public string Message { get; set; }
	}

	public enum DocumentNotificationType {
		[Description("Document Assigned")]
		Assign = 1,
		[Description("Document Unassigned")]
		Unassign = 2,
		[Description("Virtual Meeting Invite")]
		MeetingInvite = 3,
		[Description("Virtual Meeting Unassigned")]
		MeetingUnassign = 4,
		[Description("Document Approved")]
		Accepted = 5,
		[Description("Document Declined")]
		Declined = 6
	}
}
