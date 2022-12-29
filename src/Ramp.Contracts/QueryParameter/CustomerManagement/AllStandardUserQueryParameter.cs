using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.CustomerManagement
{
    public class AllStandardUserQueryParameter : IQuery
    {
        public Guid CompanyId { get; set; }
    }
}