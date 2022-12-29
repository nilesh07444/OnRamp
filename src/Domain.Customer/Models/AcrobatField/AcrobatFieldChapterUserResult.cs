using Common.Data;
using Domain.Customer.Models.Document;
using Domain.Customer.Models;
using System;

namespace Domain.Customer.Models {
	public class AcrobatFieldChapterUserResult : IdentityModel<string> {
		public string AssignedDocumentId { get; set; }
		public virtual AssignedDocument AssignedDocument { get; set; }
		public string AcrobatFieldChapterId { get; set; }
		public virtual AcrobatFieldContentBox AcrobatFieldContentBox { get; set; }
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
