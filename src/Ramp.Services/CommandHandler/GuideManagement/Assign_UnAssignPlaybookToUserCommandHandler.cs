using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class Assign_UnAssignPlaybookToUserCommandHandler : ICommandHandlerBase<Assign_UnAssignPlaybookToUserCommand>
    {
        private readonly ICommandDispatcher _commandDispacther;
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuideRepository;
        private readonly IRepository<TrainingGuideusageStats> _trainingGuideUsageStats;

        public Assign_UnAssignPlaybookToUserCommandHandler(ICommandDispatcher commandDispacther,
            IRepository<AssignedTrainingGuides> assignedTrainingGuideRepository,
            IRepository<TrainingGuideusageStats> trainingGuideUsageStats)
        {
            _commandDispacther = commandDispacther;
            _assignedTrainingGuideRepository = assignedTrainingGuideRepository;
            _trainingGuideUsageStats = trainingGuideUsageStats;
        }

        public CommandResponse Execute(Assign_UnAssignPlaybookToUserCommand command)
        {
            var assignedGuide = _assignedTrainingGuideRepository.List.Any(g => g.TrainingGuideId.Equals(command.TrainingGuideId) && g.UserId.Equals(command.AssignViewModel.Id));
            if (command.AssignViewModel.Playbook.HasValue)
            {
                if (!assignedGuide && command.AssignViewModel.Playbook.Value)
                {
                    var assignTrainingGuideToUsersCommand = new AssignTrainingGuideToUsersCommand
                    {
                        AssignedBy = command.CurrentlyLoggedInUser,
                        AssignTrainingGuideToStandardUsersViewModel = new AssignTrainingGuideToStandardUsersViewModel
                        {
                            AutoAssignTests = false,
                            CustomerStandardUsers = new List<UserViewModel> { new UserViewModel { Id = command.AssignViewModel.Id } },
                            SelectedOption = "User",
                            TrainingGuides = new List<TrainingGuideViewModel> { new TrainingGuideViewModel { TrainingGuidId = command.TrainingGuideId } }
                        }
                    };
                    _commandDispacther.Dispatch(assignTrainingGuideToUsersCommand);
                }
                else if (!command.AssignViewModel.Playbook.Value && assignedGuide)
                {
                    var assignedGuideEntity = _assignedTrainingGuideRepository.List.FirstOrDefault(g => g.TrainingGuideId.Equals(command.TrainingGuideId) && g.UserId.Equals(command.AssignViewModel.Id));
                    if (assignedGuideEntity != null)
                    {
                        _assignedTrainingGuideRepository.Delete(assignedGuideEntity);
                        _assignedTrainingGuideRepository.SaveChanges();
                    }
                    var stats = _trainingGuideUsageStats.List.Where(x => x.UserId.Equals(command.AssignViewModel.Id) && x.TrainingGuidId.Equals(command.TrainingGuideId)).ToList();
                    if (stats.Any())
                    {
                        foreach (var s in stats)
                        {
                            var e = _trainingGuideUsageStats.Find(s.Id);
                            e.Unassigned = true;
                            _trainingGuideUsageStats.SaveChanges();
                        }
                    }
                }
            }
            return null;
        }
    }
}