using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class StandardUserDisclaimerActivityLog : Base.CustomerDomainObject
    {
        public DateTime Stamp { get; set; }
        public virtual StandardUser User { get; set; }
        public bool Accepted { get; set; }
        public string IPAddress { get; set; }
        public bool Deleted { get; set; }
    }
}
