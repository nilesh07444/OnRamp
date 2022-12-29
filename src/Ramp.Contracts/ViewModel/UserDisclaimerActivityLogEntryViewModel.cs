using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class UserDisclaimerActivityLogEntryModel : UserDisclaimerActivityLogEntryModelShort
    {
        public UserModelShort User { get; set; }
    }
    public class UserDisclaimerActivityLogEntryModelShort
    {
        public Guid Id { get; set; }
        public DateTime Stamp { get; set; }
        public bool Accepted { get; set; }
        public string IPAddress { get; set; }
        public bool Deleted { get; set; }
    }
}
