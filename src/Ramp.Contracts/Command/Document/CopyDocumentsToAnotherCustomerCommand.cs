using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Command.Document
{
    public class CopyDocumentsToAnotherCustomerCommand
    {
        public string ToCustomerCompanyId { get; set; }
        public string FromCustomerCompanyId { get; set; }
        public CopyDocumentsFromCustomerCompanyViewModel CopyDocumentsFromCustomerCompanyViewModel { get; set; }
    }
}
