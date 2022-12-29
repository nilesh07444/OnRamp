using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer;

namespace Ramp.Contracts.Command.CustomDocument
{
    public class CreateOrUpdateCustomDocumentMessageCenterCommand
	{
		public string UserId { get; set; }
		public string DocumentId { get; set; }
		public DocumentType DocumentType { get; set; }
		public DateTime CreatedOn { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public DocumentUsageStatus? Status { get; set; }
		public string Messages { get; set; }
		public string AssignedDocumentId { get; set; }
	}

}

