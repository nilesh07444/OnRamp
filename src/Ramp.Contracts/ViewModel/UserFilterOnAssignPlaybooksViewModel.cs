using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel {
    public class UserFilterOnAssignPlaybooksViewModel:IViewModel {
        public UserFilterOnAssignPlaybooksViewModel() {
            TrainingGuides = new List<TrainingGuideViewModel>();
            UserIds = new List<Guid>();
            Groups = new List<GroupViewModel>();
        }

        public List<Guid> UserIds { get; set; }
        public List<TrainingGuideViewModel> TrainingGuides { get; set; }
        public List<GroupViewModel> Groups { get; set; }
        
    }
}
