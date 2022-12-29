using System;
using Common.Query;
using System.Collections.Generic;
using Domain.Enums;

namespace Ramp.Contracts.QueryParameter.CustomerManagement
{
    public class AllCustomerCompanyByProvisionalCompanyParameter : IQuery
    {
        public Guid CompanyId { get; set; }
        public Guid CurrentUserId { get; set; }
        public List<UserRole> UserRole { get; set; }
    }
}
