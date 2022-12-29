using Common.Query;
using System;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class TakeTrainingTestQueryParameter : IQuery
    {
        public Guid TrainingTestId { get; set; }
        public Guid CurrentlyLoggedInUserId { get; set; }
    }
}