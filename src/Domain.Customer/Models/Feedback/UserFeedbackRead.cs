using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.Feedback
{
    public class UserFeedbackRead : IdentityModel<string>
    {
        public DateTimeOffset Created { get; set; }
        public string UserFeedbackId { get; set; }
        public virtual UserFeedback UserFeedback { get; set; }
        public string UserId { get; set; }
        public bool Deleted { get; set; }
    }
}
