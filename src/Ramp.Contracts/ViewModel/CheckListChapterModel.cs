using System.Collections.Generic;
using System.Linq;

namespace Ramp.Contracts.ViewModel {
	public class CheckListChapterModel {

		public string Id { get; set; }
		public string ParentId { get; set; }
		public string Title { get; set; }
		public int Number { get; set; }
		public string Content { get; set; }
		public bool Deleted { get; set; }
		public bool IsConditionalLogic { get; set; } = false;
		public bool New { get; set; }
		public object Upload { get; set; }
		public bool AttachmentRequired { get; set; } 
		public bool IsChecked { get; set; } = false;

		//added by softude
		public bool IsAttached { get; set; }

		public bool CheckRequired { get; set; }
		public bool IsSignOff { get; set; }
		public string SignatureUploadId { get; set; }
		public string SignatureThumbnail { get; set; }
		public bool NoteAllow { get; set; }
		public bool IsStandardUserAttachements { get; set; } = false;
		public int DocLinkAndAttachmentCount { get; set; }
		public List<DocumentUrlViewModel> DocLinks { get; set; }
		public string IssueDiscription { get; set; }
		public string ChapterDocLinks { get; set; }
		public string ChapterDocNames { get; set; }
		public string selectedTestQuestion { get; set; }
		public string selectedTestAnswer { get; set; }
		public List<string> TestQuestion { get; set; }
		public List<string> TestAnswer { get; set; }
		public int CustomDocumentOrder { get; set; } = 0;
		public IEnumerable<UploadResultViewModel> Attachments { get; set; } = new List<UploadResultViewModel>();
		public IEnumerable<UploadResultViewModel> StandardUserAttachments { get; set; } = new List<UploadResultViewModel>();
		public IEnumerable<UploadFromContentToolsResultModel> ContentToolsUploads { get; set; } = new List<UploadFromContentToolsResultModel>();
		public int AttachmentsLength { get { return Attachments.ToList().Count; } }
		
	}
}
