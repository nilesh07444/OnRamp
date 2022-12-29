
//new file added by softude

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;
using Domain.Models;
using Domain.Customer.Models.Document;
using Domain.Enums;


namespace Domain.Customer.Models.Forms
{
    public class Form : IdentityModel<string>, IDocument
    {
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastEditDate { get; set; }
        public string LastEditedBy { get; set; }
        public Guid CustomDocummentId { get; set; }
        public virtual List<StandardUser> Collaborators { get; set; }
        public virtual DocumentCategory Category { get; set; }
        public string CategoryId { get; set; }
        public bool Printable { get; set; }
        public int Points { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
        public virtual Upload CoverPicture { get; set; }
        public string CoverPictureId { get; set; }
        public bool Deleted { get; set; }
        public string TrainingLabels { get; set; }
        public bool CheckRequired { get; set; }

        public virtual ICollection<FormChapter> FormChapters { get; set; } = new List<FormChapter>();
    }
}
