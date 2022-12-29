using Common.Data;
using System;

namespace Ramp.Contracts.Command.Test {
	public class CreateOrUpdateTestChapterUserUploadResultsCommand : IdentityModel<string> {
		public string AssignedDocumentId { get; set; }
		public string TestChapterId { get; set; }
		public DateTime CreatedDate { get; set; }
		public string UploadId { get; set; }
		#region Global accesssed
		public string DocumentId { get; set; }
		public bool IsGlobalAccessed { get; set; }
		public string UserId { get; set; }
		#endregion

	}
}
