using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class AssignTrainingTestToUsersOrGroupsViewModel
    {
        public IEnumerable<GroupViewModelShort> Groups { get; set; }
        public IEnumerable<UserViewModelShort> Users { get; set; }
        public IEnumerable<TrainingTestAssignInfoViewModel> TrainingTests { get; set; }
    }
}
