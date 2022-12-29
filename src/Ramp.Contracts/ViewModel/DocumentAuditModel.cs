using System;

namespace Ramp.Contracts.ViewModel {
public	class DocumentAuditModel {
		public Guid Id { get; set; }
		public DateTime? LastEditDate { get; set; }
		public string LastEditedBy { get; set; }
		public string DocumentStatus { get; set; }
		public string DocumentType { get; set; }
		public string DocumentName { get; set; }
		public string DocumentId { get; set; }
		public Guid UserId { get; set; }
		public string UserName { get; set; }
	}
}
