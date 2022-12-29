using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.User
{
    public class GetAllAdminsFromSameCompanyQueryParameter : IQuery
    {
        public Guid CompanyId { get; set; }

    }
}