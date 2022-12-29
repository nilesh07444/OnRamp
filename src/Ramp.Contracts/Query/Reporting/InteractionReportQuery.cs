using Domain.Customer;
using System;

namespace Ramp.Contracts.Query.Reporting
{
    public class InteractionReportQuery
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string[] GroupIds { get; set; }
        public string[] Departments { get; set; }
        public string[] CategoryIds { get; set; }
		public string TrainingLabels { get; set; }
        public DocumentType[] DocumentTypes { get; set; }
    }
}