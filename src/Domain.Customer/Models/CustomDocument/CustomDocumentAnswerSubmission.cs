using Common.Data;
using Domain.Customer.Models.Memo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class CustomDocumentAnswerSubmission : IdentityModel<Guid>
    {
        public Guid CustomDocumentID { get; set; }
        public Guid TestQuestionID { get; set; }
        public string TestSelectedAnswer { get; set; }
        public Guid StandarduserID { get; set; }
        public DocumentType documentType { get; set; }
        public DateTime? CreatedOn { get; set; }

    }
}
