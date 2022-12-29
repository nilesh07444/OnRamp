using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.Login
{
    public class UserLoginHistoryQueryParameter : IQuery
    {
        public Guid UserId { get; set; }
    }
}