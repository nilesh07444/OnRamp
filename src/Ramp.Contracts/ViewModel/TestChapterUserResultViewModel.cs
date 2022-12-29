
using System;

namespace Ramp.Contracts.ViewModel {
	public class TestChapterUserResultViewModel {

		public string AssignedDocumentId { get; set; }
		public string TestChapterId { get; set; }
		public bool IsChecked { get; set; }
		public DateTime CreatedDate { get; set; }
		public string IssueDiscription { get; set; }

	}
	public class TestUserResultViewModel
	{
		public string AssignedDocumentId { get; set; }
		public bool Status { get; set; }
		public DateTime SubmittedDate { get; set; }
		public string Id { get; set; }
	}
}
