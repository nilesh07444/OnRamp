using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.TestManagement
{
    public class GetTrainingTestByReferenceIdQueryParameter :
        IQuery
    {
        public string ReferenceId { get; set; }
    }
}
