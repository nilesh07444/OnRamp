using System;
using System.Collections.Generic;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.CustomerManagement
{
    public class AllCustomerUserQueryParameter : IQuery
    {
        public Guid LoginUserCompanyId { get; set; }
        public List<string> LoginUserRole { get; set; }
    }
}
