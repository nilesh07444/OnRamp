using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class AllAssignedGuidesToStandardUserQueryParameter : IQuery
    {
        public Guid UserId { get; set; }
    }
}