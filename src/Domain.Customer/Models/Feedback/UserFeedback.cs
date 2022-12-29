using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.Feedback
{
    public class UserFeedback : IdentityModel<string>
    {
        public UserFeedbackType Type { get; set; }
        public UserFeedbackContentType ContentType { get; set; }
        public DateTimeOffset Created { get; set; }
        public string SystemLocation { get; set; }
        public string DocumentId { get; set; }
        public DocumentType? DocumentType { get; set; }
        public string Content { get; set; }
        public string CreatedById { get; set; }
        public ICollection<UserFeedbackRead> UserFeedbackReads { get; set; } = new List<UserFeedbackRead>();
        public bool Deleted { get; set; }
    }
}
