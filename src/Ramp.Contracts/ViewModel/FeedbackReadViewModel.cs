using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class FeedbackReadViewModel
    {
        public int Id { get; set; }
        public FeedbackViewModel Feedback { get; set; }
        public Guid UserId { get; set; }
        public UserViewModel User { get; set; }
        public DateTime ReadOn { get; set; }
    }
}
