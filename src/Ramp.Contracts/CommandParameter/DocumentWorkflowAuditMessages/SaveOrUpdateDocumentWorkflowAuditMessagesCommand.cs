

using Common.Command;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.CommandParameter {
	public class SaveOrUpdateDocumentWorkflowAuditMessagesCommand : ICommand {

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
