using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class AllAssignedTestUsersByTestIdQueryParameter : IQuery
    {
        public Guid TestId { get; set; }
    }
}