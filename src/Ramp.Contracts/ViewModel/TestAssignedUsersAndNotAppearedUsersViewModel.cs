using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class TestAssignedUsersAndNotAppearedUsersViewModel : IViewModel
    {
        public TestAssignedUsersAndNotAppearedUsersViewModel()
        {
            TestAppearedUsers = new List<UserViewModel>();
            TestNotAppearedUsers = new List<UserViewModel>();
        }

        public TrainingTestViewModel TrainingTestViewModel { get; set; }
        public string RecipentUserName { get; set; }
        public List<UserViewModel> TestAppearedUsers { get; set; }
        public List<UserViewModel> TestNotAppearedUsers { get; set; }
    }
}