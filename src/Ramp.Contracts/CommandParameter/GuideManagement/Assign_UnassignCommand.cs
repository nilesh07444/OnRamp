using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter
{
    public class Assign_UnassignCommand : ICommand
    {
        public IEnumerable<AssignViewModel> AssignViewModels { get; set; } = new List<AssignViewModel>();
        public Guid CurrentlUserId { get; set; }
        public Guid TrainingGuideId { get; set; }
        public Guid TrainingTestId { get; set; }
    }
}
