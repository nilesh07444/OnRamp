using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter;
using Ramp.Contracts.CommandParameter.GuideManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;
using Ramp.Contracts.CommandParameter.TestManagement;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class Assign_UnassignCommandHandler : ICommandHandlerBase<Assign_UnassignCommand>
    {
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        private readonly ICommandDispatcher _dispatcher;
        public Assign_UnassignCommandHandler(IRepository<StandardUser> userRepository,
            IRepository<CustomerGroup> groupRepository,
            ICommandDispatcher dispatcher)
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _dispatcher = dispatcher;
        }
        public CommandResponse Execute(Assign_UnassignCommand command)
        {
            foreach (var c in command.AssignViewModels.Where(x => x.AssignMode == AssignMode.Users))
            {
                AssignPlaybookToUser(command.CurrentlUserId, command.TrainingGuideId, c);
                AssignTestToUser(command.CurrentlUserId, command.TrainingGuideId, c);
            }
            foreach (var c in command.AssignViewModels.Where(x => x.AssignMode == AssignMode.Groups))
            {
                var users = _userRepository.List.Where(x => x.Roles.Any(q => q.RoleName.Equals(Ramp.Contracts.Security.Role.StandardUser) && x.Group != null && x.Group.Id.Equals(c.Id)));
                foreach (var u in users.ToList())
                {
                    AssignPlaybookToUser(command.CurrentlUserId, command.TrainingGuideId, new AssignViewModel
                    {
                        AssignMode = AssignMode.Users,
                        Id = u.Id,
                        Playbook = c.Playbook,
                        Test = c.Test
                    });
                    AssignTestToUser(command.CurrentlUserId, command.TrainingGuideId, new AssignViewModel
                    {
                        AssignMode = AssignMode.Users,
                        Id = u.Id,
                        Playbook = c.Playbook,
                        Test = c.Test
                    });
                }
            }
            return null;
        }
        private void AssignPlaybookToUser(Guid loggedInUser, Guid TrainingGuideId, AssignViewModel c)
        {
            var result = _dispatcher.Dispatch(new Assign_UnAssignPlaybookToUserCommand
            {
                CurrentlyLoggedInUser = loggedInUser,
                TrainingGuideId = TrainingGuideId,
                AssignViewModel = c
            });
            if (result.Validation.Any())
                throw new Exception(result.Validation.First().Message);
        }
        private void AssignTestToUser(Guid loggedInUser,Guid TrainingGuideId,AssignViewModel c)
        {
            var result = _dispatcher.Dispatch(new Assign_UnAssignTestToUserCommand
            {
                AssignViewModel = c,
                CurrentlyLoggedInUser = loggedInUser,
                TrainingGuideId = TrainingGuideId,
            });
            if (result.Validation.Any())
                throw new Exception(result.Validation.First().Message);
        }
    }
}
