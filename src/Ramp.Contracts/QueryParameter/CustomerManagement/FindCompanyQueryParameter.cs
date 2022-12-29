using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.CustomerManagement
{
    public class FindCompanyQueryParameter : IQuery
    {
        public Guid Id { get; set; }
    }
}
