using Domain.Customer;
using System;

namespace Ramp.Contracts.ViewModel {
	public	class AssignedDocumentModel {
		public string Id { get; set; }
		public string DocumentId { get; set; }
		public DocumentType DocumentType { get; set; }
		public string UserId { get; set; }
		public string AdditionalMsg { get; set; }
		public string AssignedBy { get; set; }
		public DateTime? AssignedDate { get; set; }
		public bool IsRecurring { get; set; }
	}
}
