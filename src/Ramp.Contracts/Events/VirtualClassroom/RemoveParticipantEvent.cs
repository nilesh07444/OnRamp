using Common.Events;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Events.VirtualClassroom {
public	class RemoveParticipantEvent : IEvent {
		public const string DefaultSubject = "Remove participants Notification";
		public string Subject { get; set; }
		public string MeetingName { get; set; }
		public UserViewModel UserViewModel { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }


	}
}
