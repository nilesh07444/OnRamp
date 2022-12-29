using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.GuideManagement;
using Ramp.Contracts.Events.GuideManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Implementations;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class AssignTrainingGuideToUsersCommandHandler : CommandHandlerBase<AssignTrainingGuideToUsersCommand>
    {
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuidesRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<Company> _companyRepository;
        private CommandDispatcher _dispatcher;

        public AssignTrainingGuideToUsersCommandHandler(
            IRepository<StandardUser> userRepository,
            IRepository<CustomerGroup> groupRepository,
            IRepository<AssignedTrainingGuides> assignedTrainingGuidesRepository,
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<TrainingTest> trainingTestRepository,
            IRepository<Company> companyRepository)
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _assignedTrainingGuidesRepository = assignedTrainingGuidesRepository;
            _trainingGuideRepository = trainingGuideRepository;
            _trainingTestRepository = trainingTestRepository;
            _companyRepository = companyRepository;
            _dispatcher = new CommandDispatcher();
        }

        public override CommandResponse Execute(AssignTrainingGuideToUsersCommand command)
        {
            List<Guid> tests = new List<Guid>();

            if (command.AssignTrainingGuideToStandardUsersViewModel.SelectedOption == "Group")
            {
                foreach (var groupViewModel in command.AssignTrainingGuideToStandardUsersViewModel.Groups)
                {
                    foreach (var guidevm in command.AssignTrainingGuideToStandardUsersViewModel.TrainingGuides)
                    {
                        var guide = _trainingGuideRepository.Find(guidevm.TrainingGuidId);
                        var group = _groupRepository.Find(groupViewModel.GroupId);
                        var groupUsers = _userRepository.List.Where(u => u.Group != null && u.Group.Id.Equals(group.Id)).ToList();
                        foreach (var user in groupUsers)
                        {
                            var company = _companyRepository.Find(user.CompanyId);
                            if (!user.Roles.Any(r => r.RoleName.Equals(Contracts.Security.Role.CustomerAdmin)))
                            {
                                var previouslyAssigned = _assignedTrainingGuidesRepository
                                    .List
                                    .Where(a => a.TrainingGuideId == guide.Id && a.UserId == user.Id);
                                bool previouslyAssignedCached = previouslyAssigned.Any();
                                if (!previouslyAssignedCached)
                                {
                                    var guideAssigned = new AssignedTrainingGuides
                                    {
                                        Id = Guid.NewGuid(),
                                        TrainingGuideId = guide.Id,
                                        UserId = user.Id,
                                        AssignedBy = command.AssignedBy,
                                        AssignedDate = DateTime.UtcNow
                                    };
                                    _assignedTrainingGuidesRepository.Add(guideAssigned);
                                    _assignedTrainingGuidesRepository.SaveChanges();
                                    if (!command.AssignTrainingGuideToStandardUsersViewModel.AutoAssignTests)
                                    {
                                        new EventPublisher().Publish(new TrainingGuideAssignedEvent
                                        {
                                            CompanyViewModel = Project.CompanyViewModelFrom(company),
                                            UserViewModel = Project.UserViewModelFrom(user),
                                            TrainingGuideViewModel = Project.TrainingGuideViewModelFrom(guide),
                                            Subject = TrainingGuideAssignedEvent.DefaultSubject
                                        });
                                    }
                                }
                                if (command.AssignTrainingGuideToStandardUsersViewModel.AutoAssignTests &&
                                    !previouslyAssignedCached)
                                {
                                    var test =
                                        _trainingTestRepository.List.SingleOrDefault(
                                            t => t.TrainingGuideId.Equals(guide.Id) && t.ActiveStatus && !t.DraftEditDate.HasValue);
                                    if (test != null)
                                        tests.Add(test.Id);
                                }
                            }
                        }
                    }
                }
                if (command.AssignTrainingGuideToStandardUsersViewModel.AutoAssignTests && tests.Count > 0)
                {
                    var assignTestCommandParameter = new Ramp.Contracts.CommandParameter.TestManagement.AssignTestsToUsersOrGroupsCommand
                    {
                        AssignedBy = command.AssignedBy,
                        AssignTestToUsersOrGroupViewModel = new AssignTestToUsersOrGroupViewModel()
                        {
                            SelectedOption = command.AssignTrainingGuideToStandardUsersViewModel.SelectedOption,
                            Groups = command.AssignTrainingGuideToStandardUsersViewModel.Groups,
                        },
                        TestIds = tests
                    };
                    _dispatcher.Dispatch(assignTestCommandParameter);
                }
            }
            if (command.AssignTrainingGuideToStandardUsersViewModel.SelectedOption == "User")
            {
                foreach (UserViewModel user in command.AssignTrainingGuideToStandardUsersViewModel.CustomerStandardUsers)
                {
                    var selectedUser = _userRepository.Find(user.Id);
                    var company = _companyRepository.Find(selectedUser.CompanyId);
                    foreach (TrainingGuideViewModel guidevm in command.AssignTrainingGuideToStandardUsersViewModel.TrainingGuides)
                    {
                        var guide = _trainingGuideRepository.Find(guidevm.TrainingGuidId);
                        UserViewModel user1 = Project.UserViewModelFrom(selectedUser);
                        var previouslyAssigned = _assignedTrainingGuidesRepository.List
                            .Where(a => a.TrainingGuideId == guide.Id && a.UserId == user1.Id);
                        bool previouslyAssignedCached = previouslyAssigned.Any();
                        if (!previouslyAssignedCached)
                        {
                            var guideAssigned = new AssignedTrainingGuides
                            {
                                Id = Guid.NewGuid(),
                                TrainingGuideId = guide.Id,
                                UserId = user.Id,
                                AssignedBy = command.AssignedBy,
                                AssignedDate = DateTime.UtcNow
                            };

                            _assignedTrainingGuidesRepository.Add(guideAssigned);
                            _assignedTrainingGuidesRepository.SaveChanges();
                            if (!command.AssignTrainingGuideToStandardUsersViewModel.AutoAssignTests)
                            {
                                new EventPublisher().Publish(new TrainingGuideAssignedEvent
                                {
                                    CompanyViewModel = Project.CompanyViewModelFrom(company),
                                    UserViewModel = user1,
                                    TrainingGuideViewModel = Project.TrainingGuideViewModelFrom(guide),
                                    Subject = TrainingGuideAssignedEvent.DefaultSubject
                                });
                            }
                        }
                        if (command.AssignTrainingGuideToStandardUsersViewModel.AutoAssignTests && !previouslyAssignedCached)
                        {
                            var test = _trainingTestRepository.List.SingleOrDefault(t => t.TrainingGuideId.Equals(guide.Id) && t.ActiveStatus);
                            if (test != null)
                                tests.Add(test.Id);
                        }
                        if (command.AssignTrainingGuideToStandardUsersViewModel.AutoAssignTests && tests.Count > 0)
                        {
                            var assignTestCommandParameter = new Ramp.Contracts.CommandParameter.TestManagement.AssignTestsToUsersOrGroupsCommand
                            {
                                AssignedBy = command.AssignedBy,
                                AssignTestToUsersOrGroupViewModel = new AssignTestToUsersOrGroupViewModel()
                                {
                                    SelectedOption = command.AssignTrainingGuideToStandardUsersViewModel.SelectedOption,
                                    CustomerStandardUsers = command.AssignTrainingGuideToStandardUsersViewModel.CustomerStandardUsers,
                                },
                                TestIds = tests
                            };
                            _dispatcher.Dispatch(assignTestCommandParameter);
                        }
                    }
                }
            }

            return null;
        }
    }
}