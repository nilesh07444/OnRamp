using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Command
{
    public class CloneCommand<T> : IdentityModel<string> where T : class
    {
        public string SourceCompanyId { get; set; }
        public string TargetCompanyId { get; set; }
        public bool NewVersion { get; set; }
    }
    public class AuthorizeCloneCommand : IdentityModel<string>
    {
        public string Type { get; set; }
        public string SourceCompanyId { get; set; }
        public string TargetCompanyId { get; set; }
    }
}
