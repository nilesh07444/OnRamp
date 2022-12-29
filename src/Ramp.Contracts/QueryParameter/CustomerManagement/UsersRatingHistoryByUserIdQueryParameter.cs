using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.CustomerManagement
{
    public class UsersRatingHistoryByUserIdQueryParameter : IQuery
    {
        public Guid UserId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}