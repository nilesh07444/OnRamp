using Common.Data;
using System;

namespace Ramp.Contracts.Command.Test {

public	class CreateOrUpdateTestChapterUserResultCommand : IdentityModel<string> {

		public string AssignedDocumentId { get; set; }
		public string TestChapterId { get; set; }
		public bool IsChecked { get; set; }
		public string IssueDiscription { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime ChapterTrackedDate { get; set; }

		#region Global accesssed
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
		#endregion
	}
}
