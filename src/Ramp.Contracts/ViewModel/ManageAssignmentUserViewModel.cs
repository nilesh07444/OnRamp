using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class ManageAssignmentUserViewModel
    {
        public bool AssignedTrainingGuide { get; set; }
        public bool AssignedTrainingTest { get; set; }
        public bool AssignedPreviousVersionOfTest { get; set; }
        public bool PassedTest { get; set; }
        public bool PassedPreviousVersionOfTest { get; set; }
        public bool ForceRetake { get; set; }

        public UserModelShort User { get; set; }
    }
}