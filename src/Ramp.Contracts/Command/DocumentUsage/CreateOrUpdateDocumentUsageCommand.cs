using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer;

namespace Ramp.Contracts.Command.DocumentUsage
{
    public class CreateOrUpdateDocumentUsageCommand
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
	//	Pending,
	//	Viewed,
	//	Incomplete,
	//	Completed
	//}
}

