using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class AddUserToColaboratorsCommandHandler : ICommandHandlerBase<AddUserToColaboratorsCommand>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;

        public AddUserToColaboratorsCommandHandler(IRepository<TrainingGuide> trainingGuideRepository, IRepository<StandardUser> standardUserRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
            _standardUserRepository = standardUserRepository;
        }

        public CommandResponse Execute(AddUserToColaboratorsCommand command)
        {
            TrainingGuide guide = null;
            if (!command.TrainingGuideViewModel.TrainingGuidId.Equals(Guid.Empty))
                guide = _trainingGuideRepository.Find(command.TrainingGuideViewModel.TrainingGuidId);
            if (!string.IsNullOrWhiteSpace(command.TrainingGuideViewModel.ReferenceId))
                guide =
                    _trainingGuideRepository.List.FirstOrDefault(
                        g => g.ReferenceId.Equals(command.TrainingGuideViewModel.ReferenceId));
            if (guide != null)
            {
                guide.Collaborators.Clear();
                if (command.UserViewModelList != null)
                {
                    foreach (var userViewModel in command.UserViewModelList)
                    {
                        guide.Collaborators.Add(_standardUserRepository.Find(userViewModel.Id));
                    }
                }
                _trainingGuideRepository.SaveChanges();
                _standardUserRepository.SaveChanges();
            }
            return null;
        }
    }
}