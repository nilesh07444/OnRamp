using Common.Data;
using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class UserFeedbackListModel : IdentityModel<string>
    {
        public UserFeedbackType Type { get; set; }
        public UserFeedbackContentType ContentType { get; set; }
        public DateTimeOffset Created { get; set; }
        public string SystemLocation { get; set; }
        public string DocumentId { get; set; }
        public DocumentType? DocumentType { get; set; }
        public string Content { get; set; }
        public string CreatedById { get; set; }
        public UserModelShort CreatedByModel { get; set; }
    }
    public class UserFeedbackModel : UserFeedbackListModel
    {
        public IEnumerable<UserFeedbackReadistModel> Reads { get; set; } = new List<UserFeedbackReadistModel>();
    }
}
