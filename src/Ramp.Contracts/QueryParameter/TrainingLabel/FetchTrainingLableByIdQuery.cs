using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.TrainingLabel
{
    public class FetchTrainingLableByIdQuery : IdentityModel<string>
    {
        public bool? Create { get; set; }
    }
}
