using Common.Data;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.TrainingManual;
using System;

namespace Domain.Customer.Models {
	public	class TrainingManualChapterUserUploadResult : IdentityModel<string> {
		public string AssignedDocumentId { get; set; }
		public virtual AssignedDocument AssignedDocument { get; set; }
		public string TrainingManualChapterId { get; set; }
		public virtual TrainingManualChapter TrainingManualChapter { get; set; }
		public string UploadId  { get; set; }
		public bool isSignOff { get; set; }
		public virtual Upload Upload { get; set; }
		public DateTime CreatedDate { get; set; }
		#region For Global access
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
		#endregion
	}
}
