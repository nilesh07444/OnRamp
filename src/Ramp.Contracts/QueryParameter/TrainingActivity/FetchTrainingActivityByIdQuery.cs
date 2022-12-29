using Common.Data;
using Domain.Customer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.TrainingActivity
{
    public class FetchTrainingActivityByIdQuery : IdentityModel<string>
    {
        public bool? Create { get; set; }
        public TrainingActivityType? Type { get; set; }
    }
}
