using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.Events.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using Ramp.Contracts.Events.GuideAndTestManagement;
using Ramp.Services.QueryHandler;
using Ramp.Services.Projection;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class AssignTestsToUsersOrGroupsCommandHandler : CommandHandlerBase<AssignTestsToUsersOrGroupsCommand>
    {
        private readonly IRepository<CustomerGroup> _groupRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<TestResult> _resultRepository;

        public AssignTestsToUsersOrGroupsCommandHandler(IRepository<CustomerGroup> groupRepository,
            IRepository<TestAssigned> assignedTestRepository,
            IRepository<TrainingTest> trainingTestRepository,
            IRepository<StandardUser> userRepository,
            IRepository<Company> companyRepository,
            IRepository<TestResult> resultRepository)
        {
            _groupRepository = groupRepository;
            _assignedTestRepository = assignedTestRepository;
            _trainingTestRepository = trainingTestRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _resultRepository = resultRepository;
        }

        public override CommandResponse Execute(AssignTestsToUsersOrGroupsCommand command)
        {
            List<Guid> Tests = new List<Guid>();
            var fromAutoAssign = false;
            if (command.AssignTestToUsersOrGroupViewModel.SelectedOption == "Group")
            {
                foreach (var groupViewModel in command.AssignTestToUsersOrGroupViewModel.Groups)
                {
                    if (command.AssignTestToUsersOrGroupViewModel.TrainingTests.Count > 0)
                    {
                        command.AssignTestToUsersOrGroupViewModel.TrainingTests.ForEach(
                            tvm =>
                             Tests.Add(tvm.TrainingTestId));
                    }
                    else if (command.TestIds != null && command.TestIds.Count > 0)
                    {
                        Tests = command.TestIds;
                        fromAutoAssign = true;
                    }
                    if (Tests.Count > 0)
                    {
                        foreach (var testId in Tests)
                        {
                            var test = _trainingTestRepository.Find(testId);

                            var group = _groupRepository.Find(groupViewModel.GroupId);
                            var groupUsers = _userRepository.List.Where(u => u.Group != null && u.Group.Id.Equals(group.Id)).ToList();
                            foreach (var user in groupUsers)
                            {
                                var company = _companyRepository.Find(user.CompanyId);
                                if (!user.Roles.Any(r => r.RoleName.Equals(Contracts.Security.Role.CustomerAdmin)))
                                {
                                    var previouslyAssigned = _assignedTestRepository.List
                                        .Where(a => a.UserId == user.Id && a.TestId == test.Id);

                                    if (!previouslyAssigned.Any())
                                    {
                                        var testAssigned = new TestAssigned
                                        {
                                            Id = Guid.NewGuid(),
                                            //GroupId = groupViewModel.GroupId,
                                            UserId = user.Id,
                                            TestId = test.Id,
                                            AssignedBy = command.AssignedBy,
                                            AssignedDate = DateTime.UtcNow
                                        };
                                        _assignedTestRepository.Add(testAssigned);
                                        _assignedTestRepository.SaveChanges();
                                        if (!fromAutoAssign)
                                        {
                                            new EventPublisher().Publish(new TrainingTestAssignedEvent
                                            {
                                                UserViewModel = Project.UserViewModelFrom(user),
                                                Subject = TrainingTestAssignedEvent.DefaultSubject,
                                                TrainingTestViewModel = Project.TrainingTestViewModelFrom(test),
                                                CompanyViewModel = Project.CompanyViewModelFrom(company)
                                            });
                                        }
                                        else
                                        {
                                            new EventPublisher().Publish(new TrainingGuideAndTestAssignedEvent
                                            {
                                                User = Project.UserViewModelFrom(user),
                                                Subject = TrainingGuideAndTestAssignedEvent.DefaultSubject + " - " + test.TrainingGuide.Title,
                                                TrainingGuide = Project.TrainingGuideViewModelFrom(test.TrainingGuide),
                                                Company = Project.CompanyViewModelFrom(company),
                                                TrainingTest = Project.TrainingTestViewModelFrom(test)
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (command.AssignTestToUsersOrGroupViewModel.SelectedOption == "User")
            {
                foreach (UserViewModel user in command.AssignTestToUsersOrGroupViewModel.CustomerStandardUsers)
                {
                    if (command.AssignTestToUsersOrGroupViewModel.TrainingTests.Count > 0)
                    {
                        command.AssignTestToUsersOrGroupViewModel.TrainingTests.ForEach(
                            tvm => Tests.Add(tvm.TrainingTestId));
                    }
                    else if (command.TestIds != null && command.TestIds.Count > 0)
                    {
                        Tests = command.TestIds;
                        fromAutoAssign = true;
                    }
                    if (Tests.Count > 0)
                    {
                        foreach (var testId in Tests)
                        {
                            var test = _trainingTestRepository.Find(testId);
                            UserViewModel model = user;
                            var currentUser = _userRepository.Find(user.Id);
                            var company = _companyRepository.Find(currentUser.CompanyId);

                            if (command.Force)
                            {
                                var tests = _trainingTestRepository.List.Where(x => x.TrainingGuideId.Equals(test.TrainingGuideId)).Select(x => x.Id).ToList();
                                var previouslyAssignedt = _assignedTestRepository.List.Where(x => tests.Contains(x.TestId) && x.UserId.Equals(user.Id)).ToList();
                                previouslyAssignedt.ForEach(x => _assignedTestRepository.Delete(x));
                            }
                            IEnumerable<TestAssigned> previouslyAssigned = _assignedTestRepository.List.Where(a => a.UserId == model.Id && a.TestId == test.Id);
                            if (!previouslyAssigned.Any())
                            {
                              
                                var testAssigned = new TestAssigned
                                {
                                    Id = Guid.NewGuid(),
                                    UserId = user.Id,
                                    TestId = test.Id,
                                    AssignedBy = command.AssignedBy,
                                    AssignedDate = DateTime.UtcNow
                                };
                                _assignedTestRepository.Add(testAssigned);
                                _assignedTestRepository.SaveChanges();

                                if (!fromAutoAssign)
                                {
                                    new EventPublisher().Publish(new TrainingTestAssignedEvent
                                    {
                                        UserViewModel = Project.UserViewModelFrom(currentUser),
                                        Subject = TrainingTestAssignedEvent.DefaultSubject,
                                        TrainingTestViewModel = Project.TrainingTestViewModelFrom(test),
                                        CompanyViewModel = Project.CompanyViewModelFrom(company)
                                    });
                                }
                                else
                                {
                                    new EventPublisher().Publish(new TrainingGuideAndTestAssignedEvent
                                    {
                                        User = Project.UserViewModelFrom(currentUser),
                                        Subject =
                                            TrainingGuideAndTestAssignedEvent.DefaultSubject + " - " +
                                            test.TrainingGuide.Title,
                                        TrainingGuide = Project.TrainingGuideViewModelFrom(test.TrainingGuide),
                                        Company = Project.CompanyViewModelFrom(company),
                                        TrainingTest = Project.TrainingTestViewModelFrom(test)
                                    });
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}