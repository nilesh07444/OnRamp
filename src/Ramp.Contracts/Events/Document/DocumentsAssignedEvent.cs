using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Events;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Events.Document
{
    public class DocumentsAssignedEvent: IEvent
    {
        public const string DefaultSubject = "Documents Assigned Notification";
        public UserViewModel UserViewModel { get; set; }
        public CompanyViewModel CompanyViewModel { get; set; }
        public string Subject { get; set; }
		public string AdditionalMessage { get; set; }
		public IEnumerable<DocumentTitlesAndTypeQuery> DocumentTitles { get; set; }
		public byte[] NotificationHeaderLogo { get; set; }
		public byte[] NotificationFooterLogo { get; set; }
		public bool IsAssigned { get; set; }
	}
}
