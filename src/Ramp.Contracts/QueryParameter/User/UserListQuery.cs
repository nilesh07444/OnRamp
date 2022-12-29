using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.User
{
    public class UserListQuery
    {
        public IEnumerable<string> Ids { get; set; } = new List<string>();
        public IEnumerable<string> Emails { get; set; } = new List<string>();

    }
}
