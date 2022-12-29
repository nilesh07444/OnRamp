using System;
using System.Collections.Generic;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.CustomerManagement
{
    public class AverageRatingByCustomerCompanyQueryParameter : IQuery
    {
        public AverageRatingByCustomerCompanyQueryParameter()
        {
            CompanyIds = new List<Guid>();
            UserIds = new List<Guid>();
        }
        public List<Guid> UserIds { get; set; }
        public List<Guid> CompanyIds { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}