using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.ProvisionalManagement
{
    public class ProvisionalCompanyQueryParameter : IQuery
    {
        public Guid? Id { get; set; }
    }
}