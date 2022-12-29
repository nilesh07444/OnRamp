using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class Assign_UnAssignPlaybooksAndTests
    {
        public IEnumerable<SerializableSelectListItem> TrainingGuideDropDown { get; set; }
        public IEnumerable<SerializableSelectListItem> TrainingTestDropDown { get; set; }
        public IEnumerable<SerializableSelectListItem> AssignModeDropDown { get; set; }
        public IEnumerable<ManageAssignmentGroupViewModel> ManageableGroups { get; set; }
        public IEnumerable<ManageAssignmentUserViewModel> ManageableUsers { get; set; }
        public bool HasTest { get; set; }
        public FunctionalMode FunctionalMode { get; set; }
        public Guid TrainingGuideId { get; set; }
        public AssignMode AssignMode { get; set; }
    }
}