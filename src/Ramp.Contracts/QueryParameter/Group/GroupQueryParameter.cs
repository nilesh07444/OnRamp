using Common.Query;
using System;

namespace Ramp.Contracts.QueryParameter.Group
{
    public class GroupQueryParameter : IQuery
    {
        public Guid? GroupId { get; set; }
        public Guid CompanyId { get; set; }
    }
}