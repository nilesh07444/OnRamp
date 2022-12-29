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
    public class AddCreatorToCollaboratorsListForCustomerCompanyCommandHandler : ICommandHandlerBase<AddCreatorToCollaboratorsListForCustomerCompanyCommand>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public AddCreatorToCollaboratorsListForCustomerCompanyCommandHandler(IRepository<StandardUser> standardUserRepository, IRepository<TrainingGuide> trainingGuideRepository)
        {
            _standardUserRepository = standardUserRepository;
            _trainingGuideRepository = trainingGuideRepository;
        }

        public CommandResponse Execute(AddCreatorToCollaboratorsListForCustomerCompanyCommand command)
        {
            foreach (var trainingGuide in _trainingGuideRepository.List)
            {
                var user = _standardUserRepository.Find(trainingGuide.CreatedBy);
                if (user != null)
                    trainingGuide.Collaborators.Add(user);
            }

            return null;
        }
    }
}