using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.ProvisionalManagement
{
    public class AllUsersByProvisionalCompanyUserQueryParameter : IQuery
    {
        public Guid CompanyId { get; set; }
        public Guid CurrentUserId { get; set; }
    }
}