
//new file added by softude

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Policy;



namespace Domain.Customer.Models.Forms
{
    public class FormChapterUserResult : IdentityModel<string>
    {
        public string AssignedDocumentId { get; set; }
        public virtual AssignedDocument AssignedDocument { get; set; }
        public string FormChapterId { get; set; }
        public virtual FormChapter FormChapter { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime CompletedOn { get; set; }
    }
}
