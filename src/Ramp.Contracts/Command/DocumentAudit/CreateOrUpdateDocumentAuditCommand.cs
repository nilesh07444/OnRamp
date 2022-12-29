using Common.Data;
using System;

namespace Ramp.Contracts.Command.DocumentAudit {
	public class CreateOrUpdateDocumentAuditCommand : IdentityModel<string> {
		public DateTime? LastEditDate { get; set; }
		public string LastEditedBy { get; set; }
		public string DocumentStatus { get; set; }
		public string DocumentId { get; set; }
		public Guid UserId { get; set; }

	}
}