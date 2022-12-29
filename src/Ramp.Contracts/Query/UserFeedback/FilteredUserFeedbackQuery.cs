using System;
using Domain.Customer;
using Ramp.Contracts.Query.Document;

namespace Ramp.Contracts.Query.UserFeedback
{
    public class FilteredUserFeedbackQuery
    {
        public string UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Text { get; set; }
        public DocumentType[] DocumentTypes { get; set; }
        public UserFeedbackContentType[] FeedbackTypes { get; set; }
        public DocumentIdentifier[] Documents { get; set; }
		public bool IsGlobalAccess { get; set; }

	}
}