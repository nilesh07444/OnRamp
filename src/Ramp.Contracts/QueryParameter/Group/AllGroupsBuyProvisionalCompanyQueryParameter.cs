using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.Group
{
    public class AllGroupsBuyProvisionalCompanyQueryParameter : IQuery
    {
        public Guid ProvisionalCompanyId { get; set; }
        public Guid? GroupId { get; set; }
    }
}