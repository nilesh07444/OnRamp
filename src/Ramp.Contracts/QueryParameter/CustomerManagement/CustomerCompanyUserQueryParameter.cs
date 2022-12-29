using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.CustomerManagement
{
    public class CustomerCompanyUserQueryParameter : IQuery
    {
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public Guid UserId { get; set; }
        public Guid LoggedInUserId { get; set; }
    }
}