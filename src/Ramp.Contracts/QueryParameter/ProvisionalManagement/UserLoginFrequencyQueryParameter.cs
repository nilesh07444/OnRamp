using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.ProvisionalManagement
{
    public class UserLoginFrequencyQueryParameter : IQuery
    {
        public Guid SelectedUserId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}