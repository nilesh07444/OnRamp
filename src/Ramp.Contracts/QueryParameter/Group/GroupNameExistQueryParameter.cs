using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.Group
{
    public class GroupNameExistQueryParameter : IQuery
    {
        public string GroupName { get; set; }
        public Guid CompanyId { get; set; }
    }
}