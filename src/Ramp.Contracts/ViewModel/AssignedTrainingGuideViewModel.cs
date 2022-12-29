using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class AssignedTrainingGuideViewModel
    {
        public Guid UserId { get; set; }
        public Guid TrainingGuideId { get; set; }
    }
}