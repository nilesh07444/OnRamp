using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models.Document;

namespace Domain.Customer.Models
{
    public class AcrobatFieldContentBox : IdentityModel<string>, IContentBox
    {
        public AcrobatField AcrobatField { get; set; }
        public string AcrobatFieldId { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public string Content { get; set; }
        public bool AttachmentRequired { get; set; }
        public bool IsAttached { get; set; }
        public bool IsSignOff { get; set; } = false;
        public bool NoteAllow { get; set; }
        public bool Deleted { get; set; }
        public int CustomDocumentOrder { get; set; } = 0;

        public virtual ICollection<Upload> ContentToolsUploads { get; set; } = new List<Upload>();
        public virtual ICollection<Upload> Uploads { get; set; } = new List<Upload>();
        public virtual ICollection<StandardUserAdobeFieldValues>  AdobeFieldValues { get; set; } = new List<StandardUserAdobeFieldValues>();

    }
}
