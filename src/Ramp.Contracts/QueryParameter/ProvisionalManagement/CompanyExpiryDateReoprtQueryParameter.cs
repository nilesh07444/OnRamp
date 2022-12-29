using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.ProvisionalManagement
{
    public class CompanyExpiryDateReoprtQueryParameter : IQuery
    {
      public Guid CompanyId { get; set; }
        public int ExpireInXDays { get; set; }
        public bool IsMonthly { get; set; }
        public bool IsYearly { get; set; }
        public Guid IsReseller { get; set; }
        public bool AutoExpire { get; set; }

    }
}
