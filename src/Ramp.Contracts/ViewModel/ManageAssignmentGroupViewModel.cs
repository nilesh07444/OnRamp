using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class ManageAssignmentGroupViewModel
    {
        public bool AllMembersAssignedTest { get; set; }
        public bool AllMemebersAssignedTrainingGuide { get; set; }
        public GroupViewModelShort Group { get; set; }
    }
}
