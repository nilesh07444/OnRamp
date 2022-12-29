using System;
using System.Collections.Generic;
using Domain.Customer;

namespace Ramp.Contracts.Query.Reporting
{
    public class PointsStatementQuery
    {
        public Guid? ProvisionalCompanyId { get; set; }
        public Guid? CompanyId { get; set; }
        public List<Guid> GroupId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CategoryId { get; set; }
        public IEnumerable<DocumentType> DocumentTypes { get; set; } = new List<DocumentType>();
        public IEnumerable<string> TrainingLabels { get; set; } = new List<string>();
		public IEnumerable<int> GlobalAccess { get; set; } = new List<int>();
		public bool WithData { get; set; } = true;
		public bool EnableGlobalAccessDocuments { get; set; } = false;
		public bool IsChecklistEnable { get; set; }
		public IEnumerable<string> UserIds { get; set; } = new List<string>();
	}
}