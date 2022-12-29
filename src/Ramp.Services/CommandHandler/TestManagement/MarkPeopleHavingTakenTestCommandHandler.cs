using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Services.Helpers;
using System;
using System.Linq;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class MarkPeopleHavingTakenTestCommandHandler : CommandHandlerBase<MarkPeopleHavingTakenTestCommand>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IRepository<TestResult> _testResultRepository;

        public MarkPeopleHavingTakenTestCommandHandler(IRepository<TrainingTest> trainingTestRepository,
            IRepository<TestAssigned> assignedTestRepository,
            IRepository<TestResult> testResultRepository)
        {
            _trainingTestRepository = trainingTestRepository;
            _assignedTestRepository = assignedTestRepository;
            _testResultRepository = testResultRepository;
        }

        public override CommandResponse Execute(MarkPeopleHavingTakenTestCommand command)
        {
            var trainingTestActive = _trainingTestRepository.Find(command.TrainingTestId);
            var trainingTestDraft = _trainingTestRepository.GetAll().FirstOrDefault(draft => draft.ParentTrainingTestId == trainingTestActive.Id);

            var allUserWhoAlreadyTookTheActiveTest =
                _testResultRepository.GetAll()
                    .Where(tt => tt.TrainingTestId == command.TrainingTestId && !tt.TestResultStatus).ToList();

            if (trainingTestDraft != null)
            {
                foreach (TestResult result in allUserWhoAlreadyTookTheActiveTest)
                {
                    //Assign draft test to users
                    var testAssigned = new TestAssigned
                    {
                        Id = Guid.NewGuid(),
                        UserId = result.TestTakenByUserId,
                        TestId = trainingTestDraft.Id,
                        AssignedBy = command.CurrentUserId
                    };
                    _assignedTestRepository.Add(testAssigned);
                    _assignedTestRepository.SaveChanges();

                    //pass draft test for users
                    //var testResult = new TestResult
                    //{
                    //    Id = Guid.NewGuid(),
                    //    CorrectAnswers = trainingTestDraft.QuestionList.Count,
                    //    TestDate = DateTime.Now,
                    //    TestTakenByUserId = result.TestTakenByUserId,
                    //    TestResultStatus = true,
                    //    TestScore = trainingTestDraft.QuestionList.Count,
                    //    TrainingGuideId = trainingTestDraft.TrainingGuideId,
                    //    TrainingTestId = trainingTestDraft.Id,
                    //    WrongAnswers = 0,
                    //};
                    //_testResultRepository.Add(testResult);
                    //_testResultRepository.SaveChanges();
                }

                trainingTestDraft.ActiveStatus = true;
                trainingTestDraft.DraftEditDate = null;
                trainingTestDraft.ParentTrainingTestId = Guid.Empty;
                trainingTestDraft.ActivePublishDate = DateTime.Now;

                _trainingTestRepository.SaveChanges();
            }

            return null;
        }
    }
}