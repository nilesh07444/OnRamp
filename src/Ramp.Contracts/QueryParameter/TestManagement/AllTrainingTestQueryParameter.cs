using Common.Query;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class AllTrainingTestQueryParameter : IQuery
    {
        public Guid? Id { get; set; }
        public List<string> UserRole { get; set; }
        public Guid? CurrentlyLoggedInUserId { get; set; }
    }
}