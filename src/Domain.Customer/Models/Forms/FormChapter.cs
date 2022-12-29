
//new file added by softude

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;
using Domain.Customer.Models.Document;


namespace Domain.Customer.Models.Forms
{
    public class FormChapter : IdentityModel<string>, IContentUploads
    {
        public Form Form { get; set; }
        public string FormId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool Deleted { get; set; }
        public string FormFieldTitle { get; set; }
        public string FormFieldValue { get; set; }
        public bool IsConditionalLogic { get; set; } = false;
        public bool CheckRequired { get; set; }
        public virtual ICollection<Upload> ContentToolsUploads { get; set; } = new List<Upload>();
        public virtual ICollection<Upload> Uploads { get; set; } = new List<Upload>();
        public int CustomDocumentOrder { get; set; } = 0;
        public int Number { get; set; }
        public virtual ICollection<FormField> FormFields { get; set; } = new List<FormField>();
    }
}
