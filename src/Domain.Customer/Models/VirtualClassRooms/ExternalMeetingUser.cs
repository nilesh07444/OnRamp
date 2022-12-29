using System;

namespace Domain.Customer.Models.VirtualClassRooms {
	public class ExternalMeetingUser : Base.CustomerDomainObject {
		public string MeetingId { get; set; }
		public string EmailAddress { get; set; }
		public string UserId { get; set; }
		public DateTime CreatedOn { get; set; }
	}
}
