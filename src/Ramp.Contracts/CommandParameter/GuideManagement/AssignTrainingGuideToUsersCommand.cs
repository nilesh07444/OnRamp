using Common.Command;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using System;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class AssignTrainingGuideToUsersCommand : ICommand
    {
        public Guid AssignedBy { get; set; }
        public AssignTrainingGuideToStandardUsersViewModel AssignTrainingGuideToStandardUsersViewModel { get; set; }
    }
}