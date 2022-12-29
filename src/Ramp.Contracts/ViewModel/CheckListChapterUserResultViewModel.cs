
using System;

namespace Ramp.Contracts.ViewModel {
	public class CheckListChapterUserResultViewModel {

		public string AssignedDocumentId { get; set; }
		public string CheckListChapterId { get; set; }
		public bool IsChecked { get; set; }
		public DateTime CreatedDate { get; set; }
		public string IssueDiscription { get; set; }

	}
}
