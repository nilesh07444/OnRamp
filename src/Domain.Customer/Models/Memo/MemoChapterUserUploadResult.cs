using Common.Data;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Memo;
using System;

namespace Domain.Customer.Models {
	public	class MemoChapterUserUploadResult : IdentityModel<string> {
		public string AssignedDocumentId { get; set; }
		public virtual AssignedDocument AssignedDocument { get; set; }
		public string MemoChapterId { get; set; }
		public virtual MemoContentBox MemoContentBox { get; set; }
		public string UploadId  { get; set; }
		public virtual Upload Upload { get; set; }
		public DateTime CreatedDate { get; set; }
		#region For Global access
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public bool isSignOff { get; set; }
		public string UserId { get; set; }
		#endregion
	}
}
