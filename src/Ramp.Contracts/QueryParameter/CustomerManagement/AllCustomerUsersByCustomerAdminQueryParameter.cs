using Common.Query;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.QueryParameter.CustomerManagement
{
    public class AllCustomerUsersByCustomerAdminQueryParameter : IQuery
    {
        public Guid CustomerAdminId { get; set; }
        public Guid CompanyId { get; set; }
    }
}