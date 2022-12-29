using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.CommandParameter.TestManagement.Question;
using Ramp.Contracts.QueryParameter.Feedback;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Handler
{
    public class TrainingTestHandler :
        IQueryHandler<GetTrainingTestByReferenceIdQueryParameter, TrainingTestViewModel>,
        ICommandHandlerBase<SaveTrainingTestCommand>,
        ICommandHandlerBase<DeleteTrainingTestCommandParameter>
    {
        private readonly IRepository<TrainingTest> _repository;
        private readonly IRepository<TestVersion> _testVersionRepository;
        private readonly IRepository<TrainingGuide> _guideRepository;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IRepository<TestAssigned> _assignedRepository;


        public TrainingTestHandler( IRepository<TrainingTest> repository,
                                    IRepository<TestVersion> testVersionRepository,
                                    ICommandDispatcher commandDispatcher,
                                    IRepository<TrainingGuide> guideRepository,
                                    IRepository<TestAssigned> assignedRepository)
        {
            _repository = repository;
            _testVersionRepository = testVersionRepository;
            _commandDispatcher = commandDispatcher;
            _guideRepository = guideRepository;
            _assignedRepository = assignedRepository;
        }

         public CommandResponse Execute(SaveTrainingTestCommand command)
        {
            var guide = _guideRepository.Find(command.TrainingTestViewModel.SelectedTrainingGuideId);
            var test = SaveTrainingTest(command.TrainingTestViewModel, guide, command.CurrentlyLoggedInUserId);
            guide.TestVersion.Versions.Add(test);
            guide.TestVersion.CurrentVersion = test;
            _testVersionRepository.SaveChanges();
            return null;
        }
        private TrainingTest SaveTrainingTest(TrainingTestViewModel model,TrainingGuide guide,Guid CreatorId)
        {
            var trainingTest = new TrainingTest
            {
                Id = model.TrainingTestId == Guid.Empty ? Guid.NewGuid() : model.TrainingTestId,
                CreateDate = DateTime.Now,
                CreatedBy = CreatorId,
                TrainingGuideId = guide.Id,
                PassMarks = model.PassMarks,
                PassPoints = model.PassPoints,
                TestTitle = model.TestTitle,
                TestDuration = model.TestDuration,
                IntroductionContent = model.IntroductionContent,
                ReferenceId = guide.ReferenceId + "T",
                DraftEditDate = DateTime.Now,
                TrophyName = model.TrophyName,
                TestTrophy = model.TestTrophy,
                AssignMarksToQuestions = model.AssignMarksToQuestions,
                TestExpiryDate = model.IsTestExpiryDate ? null : model.TestExpiryDate,
                ParentTrainingTestId = model.ParentTrainingTestId,
                TestReview = model.TestReview,
                MaximumNumberOfRewites = model.EnableMaximumTestRewriteFunction ? model.MaximumRewrites : null,
                ActiveStatus = false,
                DraftStatus = true,
                Version = 0,
                DisableQuestionRandomization = model.DisableQuestionRandomization,
                EmailSummaryOnCompletion = model.EmailSummaryOnCompletion,
                HighlightAnswersOnSummary = model.HighlightAnswersOnSummary
            };
            if (trainingTest.TestTrophy == null && !string.IsNullOrWhiteSpace(trainingTest.TrophyName) && !string.IsNullOrWhiteSpace(model.TrophyDataBase64Encoded))
                trainingTest.TestTrophy = Convert.FromBase64String(model.TrophyDataBase64Encoded);
            _repository.Add(trainingTest);
            var index = 1;
            foreach (var question in model.QuestionsList.OrderBy(x => x.TestQuestionNumber).ToList())
            {
                var qId = question.TrainingTestQuestionId.Equals(Guid.Empty) ? Guid.NewGuid() : question.TrainingTestQuestionId;
                question.TrainingTestQuestionId = qId;
                question.TestQuestionNumber = index++;
                var result = _commandDispatcher.Dispatch(new SaveTrainingQuestionCommand { Model = question, TestId = trainingTest.Id });
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            _repository.SaveChanges();
            return trainingTest;
        }
        public TrainingTestViewModel ExecuteQuery(GetTrainingTestByReferenceIdQueryParameter query)
        {
            var trainingTest = _guideRepository.List.AsQueryable().FirstOrDefault(x => x.ReferenceId.Equals(query.ReferenceId.Replace("T", string.Empty)))?.TestVersion.LastPublishedVersion;
            var vm = Project.TrainingTestViewModelFrom(trainingTest);

            var feedbacks = new QueryExecutor().Execute<GetFeedbackForTestQueryParameter, List<FeedbackViewModel>>(new GetFeedbackForTestQueryParameter()
            {
                ReferenceId = vm.TrainingTestId.ToString()
            });

            vm.Feedback = feedbacks;

            return vm;
        }

        public CommandResponse Execute(DeleteTrainingTestCommandParameter command)
        {
            var test = _repository.Find(command.TrainingTestId);
            var allTests = new List<Guid>();
            do
            {
                allTests.Add(test.Id);
                test = _repository.Find(test.ParentTrainingTestId);
            } while (test != null && test.ParentTrainingTestId != Guid.Empty);

            allTests.Distinct().ToList().ForEach(delegate (Guid id)
            {
                var guide = _guideRepository.List.AsQueryable().FirstOrDefault(x => x.TestVersion.Versions.Any(t => t.Id == id));
                if (guide != null)
                {
                    var version = guide.TestVersion.Versions.FirstOrDefault(x => x.Id == id);
                    CleanUp(version);
                    _testVersionRepository.SaveChanges();
                    _guideRepository.SaveChanges();

                    if (guide.TestVersion.CurrentVersion != null && guide.TestVersion.CurrentVersion.Id == id)
                        guide.TestVersion.CurrentVersion = null;
                    if (guide.TestVersion.LastPublishedVersion != null && guide.TestVersion.LastPublishedVersion.Id == id)
                        guide.TestVersion.LastPublishedVersion = null;
                }
            });
            return null;
        }
        private void CleanUp(TrainingTest v)
        {
            v.ActiveStatus = false;
            v.ActivePublishDate = null;
            v.Deleted = true;
            _repository.SaveChanges();
            var assigned = _assignedRepository.List.Where(x => x.TestId.Equals(v.Id)).ToList();
            assigned.ForEach(x => _assignedRepository.Delete(x));
            _assignedRepository.SaveChanges();
        }
    }
}
