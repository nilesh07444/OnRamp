
using System;

namespace Ramp.Contracts.ViewModel {
	public class AcrobatFieldChapterUserResultViewModel {

		public string AssignedDocumentId { get; set; }
		public string AcrobatFieldChapterId { get; set; }
		public bool IsChecked { get; set; }
		public DateTime CreatedDate { get; set; }
		public string IssueDiscription { get; set; }

	}
	public class AcrobatFieldUserResultViewModel
	{
		public string AssignedDocumentId { get; set; }
		public bool Status { get; set; }
		public DateTime SubmittedDate { get; set; }
		public string Id { get; set; }
	}
}
