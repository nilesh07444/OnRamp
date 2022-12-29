using System;
using Domain.Customer;

namespace Ramp.Contracts.Query.Reporting
{
    public class InteractionReportDetailQuery
    {
        public string DocumentId { get; set; }
        public DocumentType DocumentType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string[] GroupIds { get; set; }
    }
}
