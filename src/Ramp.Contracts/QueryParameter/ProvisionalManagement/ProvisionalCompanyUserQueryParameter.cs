using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.ProvisionalManagement
{
    public class ProvisionalCompanyUserQueryParameter : IQuery
    {
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
    }
}