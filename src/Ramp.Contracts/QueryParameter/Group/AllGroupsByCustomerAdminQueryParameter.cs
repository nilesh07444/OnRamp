using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.Group
{
    public class AllGroupsByCustomerAdminQueryParameter : IQuery
    {
        public Guid CompanyId { get; set; }
    }
}