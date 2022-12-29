using Common.Query;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class ForcePeopletoRetakeTestQueryParameter : IQuery
    {
        public Guid TrainingTestId { get; set; }
        public Guid CurrentlyLoggedInUser { get; set; }
        public CompanyViewModel Company { get; set; }
    }
}