using Common.Query;
using Domain.Customer.Models.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.Feedback
{
    public class GetBulkFeedbackQueryParameter : IQuery
    {
        public FeedbackType Type { get; set; }
        public string Text { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
