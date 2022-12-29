using System;
using Domain.Customer;

namespace Ramp.Contracts.ViewModel
{
    public class UserFeedbackViewModelShort
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTimeOffset Created { get; set; }
        public UserModelShort User { get; set; }
        public UserFeedbackContentType ContentType { get; set; }
        public string DocumentId { get; set; }
        public DocumentType? DocumentType { get; set; }
        public string DocumentTitle { get; set; }
    }
}