using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.Feedback
{
    public class Feedback : Base.CustomerDomainObject
    {
        public FeedbackType Type { get; set; }
        public string Subject { get; set; }
        public string Option { get; set; }
        public string Message { get; set; }
        public DateTime FeedbackDate { get; set; }
        public Guid UserId { get; set; }
        public virtual ICollection<FeedbackRead> Reads { get; set; }
    }

    public enum FeedbackType
    {
        Playbook = 1,
        Test = 2
    }
}
