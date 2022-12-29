using Common.Query;
using System;

namespace Ramp.Contracts.QueryParameter.ActivityManagement
{
    public class AllUserActivitiesInTimeSpanQueryParameter : IQuery
    {
        public const string ProvisionalUsers = "ProvisionalUsers", CompanyUsers = "CompanyUsers";
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Guid UserId { get; set; }
        public string SelectedOption { get; set; }
        public Guid CompanyId { get; set; }
        public Guid ProvisionalCompanyId { get; set; }
    }
}