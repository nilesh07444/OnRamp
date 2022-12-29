using Common.Data;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Test;
using System;

namespace Domain.Customer.Models {
	public class TestChapterUserResult : IdentityModel<string> {
		public string AssignedDocumentId { get; set; }
		public virtual AssignedDocument AssignedDocument { get; set; }
		public string TestChapterId { get; set; }
		public virtual TestQuestion TestQuestion { get; set; }
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
