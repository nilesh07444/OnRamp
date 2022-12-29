using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.Forms
{
    public class FormFiledUserResult : IdentityModel<string>
    {
        public string Value { get; set; }
        public int Number { get; set; }
        public bool Deleted { get; set; }
        public virtual FormField FormField { get; set; }
        public string FormFieldId { get; set; }
        public virtual FormChapter FormChapter { get; set; }
        public string FormChapterId { get; set; }
        public string UserId { get; set; }
        public string AssignedId { get; set; }
    }
}
