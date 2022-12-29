using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.Forms
{
    public class FormField : IdentityModel<string>
    {
        public string FormFieldName { get; set; } = "";
        public string FormFieldDescription { get; set; } = "";
        public int Number { get; set; }
        public bool Deleted { get; set; }
        public virtual FormChapter FormChapter { get; set; }
        public string FormChapterId { get; set; }
    }
}
