using Common.Command;
using Common.Data;
using Common.Events;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.Events.TestManagement;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class PrepareTestExpiryNotificationCommandHandler : CommandHandlerBase<PrepareTestExpiryNotificationCommand>
    {
        private readonly IRepository<TestAssigned> _testAssignedRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<TrainingTest> _testRepository;
        private readonly IRepository<StandardUserCorrespondanceLog> _correspondanceLogRepository;
        private readonly IQueryExecutor _executor;
        private readonly IEventPublisher _eventPublisher;
        public PrepareTestExpiryNotificationCommandHandler(IRepository<TestAssigned> testAssignedRepository,
                                                           IRepository<StandardUser> userRepository,
                                                           IRepository<TrainingTest> testRepository,
                                                           IRepository<StandardUserCorrespondanceLog> correspondanceLogRepository,
                                                           IQueryExecutor executor,
                                                           IEventPublisher  eventPublisher
                                                          )
        {
            _testAssignedRepository = testAssignedRepository;
            _userRepository = userRepository;
            _testRepository = testRepository;
            _correspondanceLogRepository = correspondanceLogRepository;
            _executor = executor;
            _eventPublisher = eventPublisher;

        }
        public override CommandResponse Execute(PrepareTestExpiryNotificationCommand command)
        {
            var users = command.Users.Any() ? command.Users : _userRepository.List.AsQueryable();
            var testAssigned = command.TestsAssigned.Any() ? command.TestsAssigned : _testAssignedRepository.List.AsQueryable();
            var tests = command.Tests.Any() ? command.Tests : _testRepository.List.AsQueryable();
            users.Where(x => x.Group != null).ToList().ForEach(delegate (StandardUser user)
            {
                if (testAssigned.Any(x => x.TestId == command.TestId && x.UserId.HasValue && x.UserId.Value == user.Id))
                {
                    var canTakeTest = _executor.Execute<CheckUserHasAlreadyAppearedForTestQueryParameter, CheckUserHasAlreadyAppearedForTestViewModel>(new CheckUserHasAlreadyAppearedForTestQueryParameter
                    {
                        CurrentlyLoggedInUserId = user.Id,
                        TrainingTestId = command.TestId,
                        TestResults = command.TestResults,
                        Tests = command.Tests
                    });

                    if (canTakeTest.IsUserEligibleToTakeTest)
                    {
                        var testE = tests.FirstOrDefault(x => x.Id == command.TestId);
                        var test = Project.TrainingTestViewModelFrom(testE);
                        var subject = $"{SendTestExpiryNotificationEvent.DefaultSubject} {test.ReferenceId}";
                        if (!testE.ExpiryNotificationSentOn.HasValue ||
                            (testE.ExpiryNotificationSentOn.HasValue && testE.ExpiryNotificationSentOn.Value.Date != DateTime.Today.Date))
                        {
                            _eventPublisher.Publish(new SendTestExpiryNotificationEvent
                            {
                                UserViewModel = Project.UserViewModelFrom(user),
                                CompanyViewModel = Project.CompanyViewModelFrom(command.Company),
                                TrainingTestViewModel = test,
                                Subject = subject
                            });
                            testE.ExpiryNotificationSentOn = DateTime.Today;
                            _testRepository.SaveChanges();
                        }
                    }
                }
            });
            return null;
        }
    }
}
