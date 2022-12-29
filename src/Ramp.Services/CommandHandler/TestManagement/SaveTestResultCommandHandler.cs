using Common.Command;
using Common.Data;
using Common.Events;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.Events.TestManagement;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class SaveTestResultCommandHandler :
        CommandHandlerBase<SaveTestResultCommand>
    {
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly ICommandDispatcher _commandDispacther;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IQueryExecutor _executor;
        public SaveTestResultCommandHandler(
            IRepository<TrainingTest> trainingTestRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<TrainingGuide> trainingGuideRepository,
            ICommandDispatcher commandDispacther,
            IEventPublisher eventPublisher,
            IRepository<StandardUser> userRepository,
            IQueryExecutor executor)
        {
            _testResultRepository = testResultRepository;
            _trainingGuideRepository = trainingGuideRepository;
            _trainingTestRepository = trainingTestRepository;
            _commandDispacther = commandDispacther;
            _eventPublisher = eventPublisher;
            _userRepository = userRepository;
            _executor = executor;
        }

        public override CommandResponse Execute(SaveTestResultCommand command)
        {
            var actualTest = _trainingTestRepository.List.Where(t => t.Id.Equals(command.TestResultViewModel.TestId)).ToList().SingleOrDefault();
            var actualGuide = _trainingGuideRepository.List.Where(g => g.Id.Equals(command.TestResultViewModel.TrainingGuideId)).ToList().SingleOrDefault();
            var id = command.Id != Guid.Empty ? command.Id : Guid.NewGuid();
            if (actualGuide != null && actualTest != null)
            {
                var total = actualTest.QuestionList.Sum(t => t.AnswerWeightage);
                if (total == 0)
                {
                    total = actualTest.QuestionList.Count;
                }
                var result = new TestResult
                {
                    Id = id,
                    CorrectAnswers = command.TestResultViewModel.NumberOfRightAnswers,
                    Points = actualTest.PassPoints,
                    TestDate = DateTime.Now,
                    TestResultStatus = command.TestResultViewModel.TestResult,
                    TestScore = command.TestResultViewModel.TotalMarksScored,
                    TestTakenByUserId = command.TestTakenByUserId,
                    TestTitle = actualTest.TestTitle,
                    Total = total,
                    TrainingGuideCategory = actualGuide.Categories.First().CategoryTitle,
                    TrainingGuideCategoryId = actualGuide.Categories.First().Id,
                    TrainingGuideId = actualGuide.Id,
                    TrainingGuideTitle = actualGuide.Title,
                    TrainingTestId = actualTest.Id,
                    TrophyData = actualTest.TestTrophy,
                    TrophyName = actualTest.TrophyName,
                    Version = actualTest.Version ?? 0,
                    WrongAnswers = command.TestResultViewModel.NumberOfWrongAnswers
                };
                _testResultRepository.Add(result);
                command.TestResultViewModel.TestResultId = id;
                _commandDispacther.Dispatch(new LogTestResultCommand
                {
                    ResultId = id,
                    Test = command.TestViewModel
                });
                if (result.TestResultStatus)
                {
                    var cert = _executor.Execute<TestCertificateForResultQueryParameter, FileUploadResultViewModel>(
                  new TestCertificateForResultQueryParameter
                  {
                      PortalContext = command.PortalContext,
                      UserId = command.TestTakenByUserId,
                      ResultVm = command.TestResultViewModel,
                      BasePreviewPath = command.BasePreviewPath,
                      DefaultCertPath = HostingEnvironment.MapPath("~/Content/images/Certificate.jpg"),
                      ResultId = result.Id
                  });
                   if (cert != null)
                        command.TestResultViewModel.CertificateUrl = cert.PreviewPath;
                }
                _eventPublisher.Publish(new TrainingTestCompletedEvent
                {
                    CompanyViewModel = command.PortalContext?.UserCompany,
                    Subject = TrainingTestCompletedEvent.DefaultSubject,
                    TestResultViewModel = command.TestResultViewModel,
                    UserViewModel = Projection.Project.UserViewModelFrom(_userRepository.Find(command.TestTakenByUserId))
                });
            }
            return null;
        }
    }
}