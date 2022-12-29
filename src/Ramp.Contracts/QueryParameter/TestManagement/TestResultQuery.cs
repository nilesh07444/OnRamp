using Common.Data;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class TestResultQuery : IdentityModel<string>
    {
        public string ResultId { get; set; }
    }
}
