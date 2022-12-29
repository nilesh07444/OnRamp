using Common.Data;
using Domain.Customer.Models.Document;
using System.Collections.Generic;
using Domain.Customer.Models;

namespace Domain.Customer.Models {
	public class AcrobatFieldChapter : IdentityModel<string>, IContentUploads {

		public AcrobatField AcrobatField { get; set; }
		public string AcrobatFieldId { get; set; }
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
