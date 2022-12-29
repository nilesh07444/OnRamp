using Common.Query;
using System;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class TestNotApperUsersParameter : IQuery
    {
        public Guid? TestId { get; set; }
    }
}