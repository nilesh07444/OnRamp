using Common.Events;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Events.DocumentWorkflow {
	public class DocumentWorkflowEvent: IEvent {

		public const string DefaultSubject = "Documents Assigned For Approval Notification";
		public UserViewModel UserViewModel { get; set; }
		public CompanyViewModel CompanyViewModel { get; set; }
		public string Subject { get; set; }
		public string AdditionalMessage { get; set; }
		public IEnumerable<DocumentTitlesAndTypeQuery> DocumentTitles { get; set; }
		//public DocumentTitlesAndTypeQuery DocumentTitles { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
		public bool IsAssigned { get; set; }
		public string Message { get; set; }
	}
}
