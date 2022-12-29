using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.PackageManagement
{
    public class PackageQueryParameter : IQuery
    {
        public Guid? id { get; set; }
    }
}