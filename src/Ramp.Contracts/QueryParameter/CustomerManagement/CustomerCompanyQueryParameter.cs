using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.CustomerManagement
{
    public class CustomerCompanyQueryParameter : IQuery
    {
        public Guid? Id { get; set; }
        public bool IsForAdmin { get; set; }
        public Guid ProvisionalCompanyId { get; set; }
    }
}