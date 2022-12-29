using Common.Data;
using Domain.Customer.Models.Memo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class ConditionalTable : IdentityModel<string>
    {
        public Guid CustomDocumentID { get; set; }
        public string TestQuestion { get; set; }
        public string TestAnswer { get; set; }
        public Guid ChapterID { get; set; }
        public DocumentType documentType { get; set; }
        public DateTime? CreatedOn { get; set; }
      
    }
}
