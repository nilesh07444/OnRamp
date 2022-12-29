using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.CustomerManagement
{
    // TODO: Delete when completely unused
    public class CustomerCompanyByPackageIdQueryParameter : IQuery
    {
        public Guid PackageId { get; set; }
    }
}