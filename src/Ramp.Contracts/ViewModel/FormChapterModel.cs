
//new file added by softude

using Common.Data;
using Domain.Customer.Models.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class FormChapterModel : IdentityModel<string>
    {
        public Form Form { get; set; }
        public string FormId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool Deleted { get; set; }
        public bool IsConditionalLogic { get; set; } = false;
        public bool CheckRequired { get; set; }
        public string FormFieldTitle { get; set; }
        public string FormFieldValue { get; set; }
        public int CustomDocumentOrder { get; set; } = 0;
        public string selectedTestQuestion { get; set; }
        public string selectedTestAnswer { get; set; }
        public int Number { get; set; }
        public List<string> TestQuestion { get; set; }
        public List<string> TestAnswer { get; set; }
        public IEnumerable<FormFieldModel> FormFields { get; set; } = new List<FormFieldModel>();

    }
}
