using Common.Events;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using System.Collections.Generic;

namespace Ramp.Contracts.Events.VirtualClassroom {
	public class VirtualClassroomEvent: IEvent{
		public const string DefaultSubject = "Meeting Notification";
		public string Subject { get; set; }
		public UserViewModel UserViewModel { get; set; }
		public CompanyViewModel CompanyViewModel { get; set; }
		public IEnumerable<DocumentTitlesAndTypeQuery> DocumentTitles { get; set; }
		public string MeetingUrl { get; set; }
		public string MeetingName { get; set; }
		public string StartDate { get; set; }
		public string EndDate { get; set; }
		public string Password { get; set; }
		public string Id { get; set; }
		public string JoinMeetingUrl { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
	}

}
