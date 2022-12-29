using System;

namespace Ramp.Contracts.ViewModel {
	public	class CheckListUserResultViewModel {
		public string AssignedDocumentId { get; set; }
		public bool Status { get; set; }
		public DateTime SubmittedDate { get; set; }
		public string Id { get; set; }
	}
}
