using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel {
	public class CustomDocumentChapters {

		public string Id { get; set; }
		public string ParentId { get; set; }
		public string Title { get; set; }
		public int Number { get; set; }
		public string Content { get; set; }
		public bool Deleted { get; set; }
		public bool New { get; set; }
		public object Upload { get; set; }
		public int DocLinkAndAttachmentCount { get; set; }
		public List<DocumentUrlViewModel> DocLinks { get; set; }
		public string ChapterDocLinks { get; set; }
		public string ChapterDocNames { get; set; }
		public IEnumerable<UploadResultViewModel> Attachments { get; set; } = new List<UploadResultViewModel>();
		public IEnumerable<UploadFromContentToolsResultModel> ContentToolsUploads { get; set; } = new List<UploadFromContentToolsResultModel>();

		public bool AttachmentRequired { get; set; }
		public bool IsChecked { get; set; } = false;
		public bool CheckRequired { get; set; }
		public bool NoteAllow { get; set; }
		public bool dynamicFields { get; set; }		
		public bool IsAttached { get; set; }
		public bool IsSignOff { get; set; } = false;
		public bool IsConditionalLogic { get; set; } = false;
		public bool IsShowPolicy { get; set; } = true;
		public bool IsStandardUserAttachements { get; set; } = false;
		public string IssueDiscription { get; set; }
		public int AttachmentsLength { get { return Attachments.ToList().Count; } }
		public List<string> TestQuestion { get; set; }
		public List<string> TestAnswer { get; set; }
		public string selectedTestQuestion { get; set; }
		public string selectedTestAnswer { get; set; }
		public string Option { get; set; }
		public int Marks { get; set; } = 1;
		public string Question { get; set; }
		public string CorrectAnswerId { get; set; }
		public int CustomDocumentOrder { get; set; }
		public IEnumerable<TestQuestionAnswerModel> Answers { get; set; } = new List<TestQuestionAnswerModel>();

		//added by softude
		public IEnumerable<FormFieldModel> FormFields { get; set; } = new List<FormFieldModel>();
	}
}
