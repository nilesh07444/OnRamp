using Common.Data;
using Domain.Customer.Models.Document;
using System.Collections.Generic;
using Domain.Customer.Models.Memo;

namespace Domain.Customer.Models {
	public class MemoChapter : IdentityModel<string>, IContentUploads {

		public Domain.Customer.Models.Memo.Memo Memo { get; set; }
		public string MemoId { get; set; }
		public string Title { get; set; }
		public int Number { get; set; }
		public string Content { get; set; }
		public bool Deleted { get; set; }
		public bool AttachmentRequired { get; set; }
		public bool CheckRequired { get; set; }
		public bool IsChecked { get; set; } 
		public bool NoteAllow { get; set; } 
		public virtual ICollection<Upload> ContentToolsUploads { get; set; } = new List<Upload>();
		public virtual ICollection<Upload> Uploads { get; set; } = new List<Upload>();
	}
}
