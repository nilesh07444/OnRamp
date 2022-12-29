using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.ProvisionalManagement
{
    public class UsersLoginStatsQueryParameter : IQuery
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Guid CurrentUserId { get; set; }
        public Guid CompanyId { get; set; }
    }
}