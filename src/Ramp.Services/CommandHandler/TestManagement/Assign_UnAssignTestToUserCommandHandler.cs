using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class Assign_UnAssignTestToUserCommandHandler : ICommandHandlerBase<Assign_UnAssignTestToUserCommand>
    {
        private readonly ICommandDispatcher _commandDispacther;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TestResult> _resultRepository;
        private readonly IRepository<TrainingTestUsageStats> _testUsageStatsRepository;
        private readonly IRepository<TrainingGuide> _guideRepository;
        public Assign_UnAssignTestToUserCommandHandler(ICommandDispatcher commandDispacther,
            IRepository<TestAssigned> assignedTestRepository,
            IRepository<TrainingTest> trainingTestRepository,
            IRepository<TestResult> resultRepository,
            IRepository<TrainingTestUsageStats> testUsageStatsRepository,
            IRepository<TrainingGuide> guideRepository)
        {
            _commandDispacther = commandDispacther;
            _assignedTestRepository = assignedTestRepository;
            _trainingTestRepository = trainingTestRepository;
            _resultRepository = resultRepository;
            _testUsageStatsRepository = testUsageStatsRepository;
            _guideRepository = guideRepository;
        }

        public CommandResponse Execute(Assign_UnAssignTestToUserCommand command)
        {
            var test = _guideRepository.Find(command.TrainingGuideId).TestVersion.LastPublishedVersion;
            if (test != null)
            {
                var assignedTest = _assignedTestRepository.List.ToList().Any(t => t.TestId.Equals(test.Id) && t.UserId.Equals(command.AssignViewModel.Id));
                if (command.AssignViewModel.Test.HasValue)
                {
                    if (!assignedTest && (command.AssignViewModel.Test.Value || command.AssignViewModel.ForceRetake))
                    {
                        var assignTestCommand = new AssignTestsToUsersOrGroupsCommand
                        {
                            AssignedBy = command.CurrentlyLoggedInUser,
                            AssignTestToUsersOrGroupViewModel = new AssignTestToUsersOrGroupViewModel
                            {
                                CustomerStandardUsers = new List<UserViewModel> { new UserViewModel { Id = command.AssignViewModel.Id } },
                                SelectedOption = "User",
                                TrainingTests = new List<TrainingTestViewModel> { Project.TrainingTestViewModelFrom(test) },
                            },
                            Force = command.AssignViewModel.ForceRetake
                        };
                        var result = _commandDispacther.Dispatch(assignTestCommand);
                        if (result.Validation.Any())
                            throw new Exception(result.Validation.First().Message);
                        var allResultsForTestAndUser = _resultRepository.List.Where(x => x.TestTakenByUserId.Equals(command.AssignViewModel.Id)
                                                                                  && x.TrainingTestId.Equals(test.Id)).ToList();
                        foreach (var r in allResultsForTestAndUser)
                        {
                            var rO = _resultRepository.Find(r.Id);
                            rO.MaximumTestRewritesReached = true;
                            _resultRepository.SaveChanges();
                        }
                    }
                    else if (assignedTest && !command.AssignViewModel.Test.Value)
                    {
                        var assignedTestEntity = _assignedTestRepository.List.FirstOrDefault(t => t.TestId.Equals(test.Id) && t.UserId.Equals(command.AssignViewModel.Id));
                        //if (command.AssignViewModel.ForceRetake)
                        //{
                        //    var g = _guideRepository.Find(assignedTestEntity.Test.TrainingGuideId);
                        //    if (g != null) {
                        //        var tIds = g.TestVersion.Versions.Select(t => t.Id);
                        //        _assignedTestRepository.List.Where(x => tIds.Contains(x.TestId) && x.UserId.Equals(command.AssignViewModel.Id)).ToList().ForEach(x => _assignedTestRepository.Delete(x));
                        //    }

                        //}
                        if (assignedTestEntity != null)
                        {
                            _assignedTestRepository.Delete(assignedTestEntity);
                            _assignedTestRepository.SaveChanges();
                        }
                        var stats = _testUsageStatsRepository.List.Where(x => x.UserId.Equals(command.AssignViewModel.Id) && x.TrainingTestId.Equals(test.Id)).ToList();
                        if (stats.Any())
                        {
                            foreach (var s in stats)
                            {
                                var e = _testUsageStatsRepository.Find(s.Id);
                                e.Unassigned = true;
                                _testUsageStatsRepository.SaveChanges();
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}