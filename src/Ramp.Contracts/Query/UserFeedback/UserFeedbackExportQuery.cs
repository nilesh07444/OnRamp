using System;
using Domain.Customer;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Query.UserFeedback
{
    public class UserFeedbackExportQuery : IContextQuery
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DocumentType[] DocumentTypes { get; set; }
        public UserFeedbackContentType[] FeedbackTypes { get; set; }
        public string Text { get; set; }
        public DocumentIdentifier[] Documents { get; set; }
        public PortalContextViewModel PortalContext { get; set; }
        public bool AddOnrampBranding { get; set; }
		public bool IsGlobalAccess { get; set; }
        public string ScheduleName { get; set; }
        public string Recepients { get; set; }

    }
}