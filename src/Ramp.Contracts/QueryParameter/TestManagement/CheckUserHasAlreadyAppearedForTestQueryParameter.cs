using Common.Query;
using Domain.Customer.Models;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class CheckUserHasAlreadyAppearedForTestQueryParameter : IQuery
    {
        public Guid TrainingTestId { get; set; }
        public Guid CurrentlyLoggedInUserId { get; set; }
        public IEnumerable<TestResult> TestResults { get; set; } = new List<TestResult>();
        public IEnumerable<TrainingTest> Tests { get; set; } = new List<TrainingTest>();
    }
}