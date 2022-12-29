using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.Feedback
{
    public class FeedbackRead
    {
        public int Id { get; set; }
        public virtual Feedback Feedback { get; set; }
        public Guid UserId { get; set; }
        public DateTime ReadOn { get; set; }
    }
}
