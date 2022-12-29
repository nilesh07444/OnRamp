using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.StandardUser
{
    public class NewExpiredUsersQuery
    {
        public Guid CompanyId { get; set; }
        public int? DefaultUserExpireDays { get; set; }
    }
}
