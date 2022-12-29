using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.ProvisionalManagement
{
    public class AllCustomerCompanyQueryParameter : IQuery
    {
        public Guid ProvisionalCompanyId { get; set; }
    }
}