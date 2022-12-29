using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class AssignTestToUsersOrGroupViewModel : IViewModel
    {
        public AssignTestToUsersOrGroupViewModel()
        {
            CustomerStandardUsers = new List<UserViewModel>();
            Groups = new List<GroupViewModel>();
            TrainingTests = new List<TrainingTestViewModel>();
        }
        public string SelectedOption { get; set; }
        public List<UserViewModel> CustomerStandardUsers { get; set; }
        public List<GroupViewModel> Groups { get; set; }
        public List<TrainingTestViewModel> TrainingTests { get; set; }
    }
}