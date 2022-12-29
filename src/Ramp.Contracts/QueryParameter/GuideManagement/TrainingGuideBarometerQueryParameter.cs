using System;
using Common.Query;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class TrainingGuideBarometerQueryParameter : IQuery
    {
        public Guid UserId { get; set; }
    }
}