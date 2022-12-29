using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class UpdateTrainingGuideUserCommandHandler : ICommandHandlerBase<UpdateTrainingGuideUserCommand>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public UpdateTrainingGuideUserCommandHandler(IRepository<StandardUser> standardUserRepository,
             IRepository<Domain.Models.User> userRepository,
             IRepository<TrainingGuide> trainingGuideRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
            _trainingGuideRepository = trainingGuideRepository;
        }

        public CommandResponse Execute(UpdateTrainingGuideUserCommand command)
        {
            var user = _userRepository.Find(command.PreviousUserId);
            if (user != null)
            {
                var newUser = _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(user.EmailAddress));

                if (newUser != null)
                {
                    var guides = _trainingGuideRepository.List.Where(g => g.CreatedBy.Equals(user.Id)).ToList();
                    if (guides.Count > 0)
                    {
                        guides.ForEach(g => g.CreatedBy = newUser.Id);
                    }
                }
                _trainingGuideRepository.SaveChanges();
            }
            return null;
        }
    }
}