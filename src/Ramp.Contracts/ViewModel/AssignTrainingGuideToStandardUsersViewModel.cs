using System.Collections.Generic;
using System;

namespace Ramp.Contracts.ViewModel
{
    public class AssignTrainingGuideToStandardUsersViewModel : IViewModel
    {
        public AssignTrainingGuideToStandardUsersViewModel()
        {
            CustomerStandardUsers = new List<UserViewModel>();
            Groups = new List<GroupViewModel>();
            TrainingGuides = new List<TrainingGuideViewModel>();
        }
        public string SelectedOption { get; set; }
        public List<GroupViewModel> Groups { get; set; }
        public List<UserViewModel> CustomerStandardUsers { get; set; }
        public List<TrainingGuideViewModel> TrainingGuides { get; set; }
        public bool AutoAssignTests { get; set; }
        public Guid Id { get; set; } 
    }
}