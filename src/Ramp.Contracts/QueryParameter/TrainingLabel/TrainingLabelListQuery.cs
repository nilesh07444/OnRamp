using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.TrainingLabel
{
    public class TrainingLabelListQuery
    {
        public IEnumerable<string> Names = new List<string>();
    }
}
