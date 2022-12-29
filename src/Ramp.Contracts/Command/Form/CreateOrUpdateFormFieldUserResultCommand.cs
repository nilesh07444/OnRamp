using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Command.Form
{
    public class CreateOrUpdateFormFieldUserResultCommand : IdentityModel<string>
    {
        public string FormFieldName { get; set; } 
        public string FormFieldDescription { get; set; } 
        public string FormChapterId { get; set; }
        public string FormFiledId { get; set; }
        public int Number { get; set; }
        public string AssignedId { get; set; }
    }
}
