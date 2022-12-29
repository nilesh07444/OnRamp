using Common.Data;
using Domain.Customer.Models.Document;
using System;

namespace Domain.Customer.Models.CheckLists {
	public	class CheckListChapterUserUploadResult : IdentityModel<string> {
		public string AssignedDocumentId { get; set; }
		public virtual AssignedDocument AssignedDocument { get; set; }
		public string CheckListChapterId { get; set; }
		public virtual CheckListChapter CheckListChapter { get; set; }
		public string UploadId  { get; set; }
		public virtual Upload Upload { get; set; }
		public bool isSignOff { get; set; }
		public DateTime CreatedDate { get; set; }
		#region For Global access
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
		#endregion
	}
}
