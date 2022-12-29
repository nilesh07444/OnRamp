using Domain.Customer.Models.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer;

namespace Ramp.Contracts.ViewModel
{
    public class ContentFeedbackViewModel
    {
        public DocumentType DocumentType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Text { get; set; }
        public List<UserFeedbackViewModelShort> UserFeedback { get; set; }
    }
}
