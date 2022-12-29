using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class AssignTrainingGuideToUsersOrGroupsViewModel
    {
        public IEnumerable<GroupViewModelShort> Groups { get; set; }
        public IEnumerable<UserViewModelShort> Users { get; set; }
        public IEnumerable<TrainingGuideAssignInfoViewModel> TrainingGuides { get; set; }
    }
}
