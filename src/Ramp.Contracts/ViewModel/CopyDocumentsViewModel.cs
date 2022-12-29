using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class CopyDocumentsViewModel
    {
        public IEnumerable<SerializableSelectListItem> Companies { get; set; } = new List<SerializableSelectListItem>();
        public string FromSelectedCustomerCompany { get; set; }
        public string ToSelectedCustomerCompany { get; set; }
    }
}
