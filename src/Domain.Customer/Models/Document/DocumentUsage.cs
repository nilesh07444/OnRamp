using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;

namespace Domain.Customer.Models.Document
{
    public class DocumentUsage : IdentityModel<string>
    {
        public string UserId { get; set; }
        public string DocumentId { get; set; }
        public DocumentType DocumentType { get; set; }
        public DateTime ViewDate { get; set; }
        public TimeSpan Duration { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public DocumentUsageStatus? Status { get; set; }
		public string AssignedDocumentId { get; set; }
	}

	//public enum status {
	// Pending,
	// Viewed,
	// Incomplete,
	// Completed
	//}
}
