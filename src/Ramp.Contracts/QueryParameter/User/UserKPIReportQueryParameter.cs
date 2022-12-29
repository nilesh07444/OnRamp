using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.User
{
    public class UserKpiReportQueryParameter : IQuery
    {
        public Guid? ProvisionalCompanyId { get; set; }
        public Guid? CustomerCompanyId { get; set; }
        public Guid? UserId { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
