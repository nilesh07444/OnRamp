using System;

namespace Ramp.Contracts.ViewModel {
	public class ExternalMeetingUserModel {
		public Guid Id { get; set; }
		public string MeetingId { get; set; }
		public string EmailAddress { get; set; }
		public string UserId { get; set; }
		public DateTime CreatedOn { get; set; }
	}
}
