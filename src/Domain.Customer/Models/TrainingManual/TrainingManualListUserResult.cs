using Common.Data;
using Domain.Customer.Models.Document;
using System;

namespace Domain.Customer.Models.TrainingManuals {
	public class TrainingManualUserResult : IdentityModel<string> {

		public string AssignedDocumentId { get; set; }
		public string DocumentId { get; set; }
		public virtual AssignedDocument AssignedDocument { get; set; }
		public DateTime SubmittedDate { get; set; }
		public bool Status { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }

	}
}
