using Common.Events;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using System.Collections.Generic;

namespace Ramp.Contracts.Events.VirtualClassroom {
	public	class CancelVirtualRoomEvent : IEvent {
		public const string DefaultSubject = "Cancelled Meeting Notification";
		public string Subject { get; set; }
		public UserViewModel UserViewModel { get; set; }
		public CompanyViewModel CompanyViewModel { get; set; }
		public IEnumerable<DocumentTitlesAndTypeQuery> DocumentTitles { get; set; }
		public string MeetingName { get; set; }
		public string AdditionNote { get; set; }
		public string StartDate { get; set; }
		public string EndDate { get; set; }
		public bool IsConfirmed { get; set; } = false;
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
	}

}
