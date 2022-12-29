using Common.Data;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Memo;
using System;

namespace Domain.Customer.Models {
	public class MemoChapterUserResult : IdentityModel<string> {
		public string AssignedDocumentId { get; set; }
		public virtual AssignedDocument AssignedDocument { get; set; }
		public string MemoChapterId { get; set; }
		public virtual MemoContentBox MemoContentBox { get; set; }
		public  bool IsChecked { get; set; }
		public string IssueDiscription { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime DateCompleted { get; set; }
		public DateTime? ChapterTrackedDate { get; set; } 
		#region For Global access
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
		#endregion
	}
}
