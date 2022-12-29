using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models.Document;

namespace Domain.Customer.Models.Policy
{
    public class PolicyContentBox : IdentityModel<string>, IContentBox
    {
        public Policy Policy { get; set; }
        public string PolicyId { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public string Content { get; set; }
        public bool Deleted { get; set; }
        public int CustomDocumentOrder { get; set; } = 0;
        public virtual ICollection<Upload> ContentToolsUploads { get; set; } = new List<Upload>();
        public virtual ICollection<Upload> Uploads { get; set; } = new List<Upload>();

        //added by softude
        public bool AttachmentRequired { get; set; }
        public bool IsAttached { get; set; }
        public bool NoteAllow { get; set; }
        public bool IsSignOff { get; set; }
        public bool IsConditionalLogic { get; set; } = false;
        public bool CheckRequired { get; set; }

        

    }
}
