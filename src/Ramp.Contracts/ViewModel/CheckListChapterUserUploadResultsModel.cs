using System;

namespace Ramp.Contracts.ViewModel {
	public class CheckListChapterUserUploadResultsModel {
		public string AssignedDocumentId { get; set; }
		public string CheckListChapterId { get; set; }
		public string UploadId { get; set; }
		public DateTime CreatedDate { get; set; }
		public bool isSignOff { get; set; }
		public string Id { get; set; }
		#region For Global access
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
		#endregion
	}
}
