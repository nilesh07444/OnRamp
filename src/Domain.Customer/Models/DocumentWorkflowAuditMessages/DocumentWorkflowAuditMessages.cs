using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models {
	public class DocumentWorkflowAuditMessages : Base.CustomerDomainObject {

		public Guid Id { get; set; }
		public Guid CreatorId { get; set; }
		public Guid ApproverId { get; set; }
		public string DocumentId { get; set; }
		public string Message { get; set; }
		//public string ApproverEmail { get; set; }
		//public string ApproverName { get; set; }

		public DateTime? DateCreated { get; set; }
		public DateTime? DateEdited { get; set; }
	}
}
