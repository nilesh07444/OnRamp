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
    public class UpdateAssignedTrainingGuideCommandHandler : ICommandHandlerBase<UpdateAssignedTrainingGuideCommand>
    {
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuidesRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;

        public UpdateAssignedTrainingGuideCommandHandler(IRepository<AssignedTrainingGuides> assignedTrainingGuidesRepository,
            IRepository<StandardUser> standardUserRepository, IRepository<User> userRepository)
        {
            _assignedTrainingGuidesRepository = assignedTrainingGuidesRepository;
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
        }

        public CommandResponse Execute(UpdateAssignedTrainingGuideCommand command)
        {
            var user = _userRepository.Find(command.PreviousUserId);
            if (user != null)
            {
                var newUser = _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(user.EmailAddress));

                if (newUser != null)
                {
                    var assignedGuides =
                        _assignedTrainingGuidesRepository.List.Where(
                            g => g.UserId.HasValue && g.UserId.Value.Equals(user.Id));

                    foreach (var assignedGuide in assignedGuides)
                    {
                        assignedGuide.UserId = newUser.Id;
                    }
                }
                _assignedTrainingGuidesRepository.SaveChanges();
            }
            return null;
        }
    }
}