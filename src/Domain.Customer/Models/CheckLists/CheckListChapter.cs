using Common.Data;
using Domain.Customer.Models.Document;
using System.Collections.Generic;

namespace Domain.Customer.Models.CheckLists {
	public class CheckListChapter : IdentityModel<string>, IContentUploads {

		public CheckList CheckList { get; set; }
		public string CheckListId { get; set; }
		public string Title { get; set; }
		public int Number { get; set; }
		public string Content { get; set; }
		public bool Deleted { get; set; }
		public bool AttachmentRequired { get; set; }
		public bool CheckRequired { get; set; }
		public bool IsSignOff { get; set; }

		public bool IsChecked { get; set; } 
		public bool NoteAllow { get; set; }
		public int CustomDocumentOrder { get; set; } = 0;
		public virtual ICollection<Upload> ContentToolsUploads { get; set; } = new List<Upload>();
		public virtual ICollection<Upload> Uploads { get; set; } = new List<Upload>();

		//added by softude
		public bool IsConditionalLogic { get; set; } = false;
		public bool IsAttached { get; set; }

	}
}
