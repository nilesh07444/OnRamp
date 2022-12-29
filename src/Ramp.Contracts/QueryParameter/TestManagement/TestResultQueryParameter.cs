using Common.Query;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class TestResultQueryParameter : IQuery
    {
        public TestViewModel TestViewModel { get; set; }
        public Guid TestTakenByUserId { get; set; }
    }
}