using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.Group
{
    public class AllGroupsByCustomerGroupIdParameter : IQuery
    {
        public Guid CustomerId { get; set; }
        public Guid GroupId { get; set; }
    }
}