using System;

namespace Ramp.Contracts.CommandParameter.ExternalMeetingUsers {
public	class CreateExternalMeetingUserCommand {
		public Guid Id { get; set; }
		public string MeetingId { get; set; }
		public string EmailAddress { get; set; }
		public string UserId { get; set; }
		public DateTime CreatedOn { get; set; }
	}
}
