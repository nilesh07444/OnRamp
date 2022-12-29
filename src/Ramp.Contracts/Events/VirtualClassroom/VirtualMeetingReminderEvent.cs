using Common.Events;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Events.VirtualClassroom {
	public class VirtualMeetingReminderEvent : IEvent {
		public const string DefaultSubject = "Meeting Reminder";
		public string Subject { get; set; }
		public UserViewModel UserViewModel { get; set; }
		public string MeetingUrl { get; set; }
		public string MeetingName { get; set; }
		public string StartDate { get; set; }
		public string EndDate { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
		public CompanyViewModel CompanyViewModel { get; set; }
	}
}
