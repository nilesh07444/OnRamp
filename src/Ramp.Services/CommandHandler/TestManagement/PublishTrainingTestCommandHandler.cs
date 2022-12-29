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
    public class PublishTrainingTestCommandHandler : ICommandHandlerBase<PublishTrainingTestCommandParameter>
    {
        private readonly IRepository<TrainingTest> _testRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IRepository<TrainingGuide> _guideRepository;
        public PublishTrainingTestCommandHandler(
            IRepository<TrainingTest> testRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<StandardUser> userRepository,
            IRepository<TestAssigned> assignedTestRepository,
            IRepository<TrainingGuide> guideRepository)
        {
            _testRepository = testRepository;
            _testResultRepository = testResultRepository;
            _userRepository = userRepository;
            _assignedTestRepository = assignedTestRepository;
            _guideRepository = guideRepository;
        }

        public CommandResponse Execute(PublishTrainingTestCommandParameter command)
        {
            if (command.TrainingTestId.Equals(Guid.Empty))
            {
                throw new ArgumentNullException();
            }
            else
            {
                var test = _testRepository.Find(command.TrainingTestId);
                if (test == null)
                    throw new Exception($"No test found with Id: {command.TrainingTestId}");
                var guide = _guideRepository.List.SingleOrDefault(x => x.Id.Equals(test.TrainingGuideId));
                if (guide == null)
                    throw new Exception($"No guide found with Id: {test.TrainingGuideId}");
                test = guide.TestVersion.LastPublishedVersion;
                guide.TestVersion.CurrentVersion.ActivePublishDate = DateTime.Now;
                guide.TestVersion.CurrentVersion.ActiveStatus = true;
                guide.TestVersion.CurrentVersion.DraftStatus = false;
                guide.TestVersion.CurrentVersion.DraftEditDate = null;
                if (test == null)
                    guide.TestVersion.CurrentVersion.Version = 1;
                _testRepository.SaveChanges();
                guide.TestVersion.LastPublishedVersion = guide.TestVersion.CurrentVersion;
                _guideRepository.SaveChanges();
                if (!command.DoNotAssignTests && test != null)
                {
                    var allUsersThatHaveNotPassedTheTest = _testResultRepository.List.Where(r => !r.TestResultStatus && r.TrainingTestId.Equals(test.Id)).Select(u => u.TestTakenByUserId).ToList();
                    var allUsersThatHavePassedTheTest = _testResultRepository.List.Where(r => r.TestResultStatus && r.TrainingTestId.Equals(test.Id)).Select(u => u.TestTakenByUserId).ToList();
                    var usersAssigned = _assignedTestRepository.List.Where(t => t.Test.Id.Equals(test.Id) && t.UserId.HasValue).Select(u => u.UserId.Value).ToList();
                    var reassignedUsers = usersAssigned.Where(t => !allUsersThatHavePassedTheTest.Contains(t)).ToList();
                    foreach (var testResult in reassignedUsers)
                    {
                        var user = _userRepository.Find(testResult);
                        if (user != null)
                        {
                            new CommandDispatcher().Dispatch(new AssignTestsToUsersOrGroupsCommand
                            {
                                AssignedBy = command.CurrentlyLoggedInUser,
                                AssignTestToUsersOrGroupViewModel = new AssignTestToUsersOrGroupViewModel
                                {
                                    SelectedOption = "User",
                                    CustomerStandardUsers = new List<UserViewModel>() { Project.UserViewModelFrom(user) },
                                },
                                CompanyViewModel = command.Company,
                                TestIds = new List<System.Guid>() { guide.TestVersion.LastPublishedVersion.Id }
                            });
                            var previousAssigned = _assignedTestRepository.List.Where(a => a.Test.Id.Equals(test.Id) && a.UserId.Equals(testResult)).ToList();
                            previousAssigned.ForEach(a => _assignedTestRepository.Delete(a));
                        }
                    }
                }
            }
            return null;
        }
    }
}
