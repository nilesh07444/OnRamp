using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.CommandParameter.Upload;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class CloneTestCommandHandler :
        CommandHandlerBase<CloneTestCommand>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TrainingQuestion> _questionRepository;
        private readonly IRepository<Domain.Customer.Models.QuestionUpload> _questionUploadRepository;
        private readonly IRepository<TestAnswer> _answerRepository;
        private readonly IRepository<FileUploads> _uploadRepository;
        private readonly IRepository<TrainingGuide> _guideRepository;

        public CloneTestCommandHandler(IRepository<TrainingTest> trainingTestRepository,
                                        IRepository<TrainingQuestion> questionRepository,
                                        IRepository<Domain.Customer.Models.QuestionUpload> questionUploadRepository,
                                        IRepository<TestAnswer> answerRepository,
                                        IRepository<FileUploads> uploadRepository,
                                        IRepository<TrainingGuide> guideRepository)
        {
            _trainingTestRepository = trainingTestRepository;
            _questionRepository = questionRepository;
            _questionUploadRepository = questionUploadRepository;
            _answerRepository = answerRepository;
            _uploadRepository = uploadRepository;
            _guideRepository = guideRepository;
        }

        public override CommandResponse Execute(CloneTestCommand command)
        {
            var originalTest = _trainingTestRepository.Find(command.TrainingTestViewModel.TrainingTestId);
            originalTest.DraftEditDate = DateTime.Now;
            _trainingTestRepository.SaveChanges();
            var cloneId = Guid.NewGuid();
            command.newDraftId = cloneId;
            CloneTest(originalTest, command.CurrentlyLoggedInUserId, cloneId);
            var newT = _trainingTestRepository.Find(cloneId);
            var guide = _guideRepository.Find(newT.TrainingGuideId);
            guide.TestVersion.Versions.Add(newT);
            guide.TestVersion.CurrentVersion = newT;
            guide.TestVersion.LastPublishedVersion = originalTest;
            _guideRepository.SaveChanges();
            return null;
        }

        private void CloneTest(TrainingTest test, Guid UserId, Guid newId)
        {
            var newTest = new TrainingTest
            {
                Id = newId,
                DraftEditDate = DateTime.Now,
                ActiveStatus = false,
                AssignMarksToQuestions = test.AssignMarksToQuestions,
                CreateDate = DateTime.Now,
                CreatedBy = UserId,
                DraftStatus = true,
                IntroductionContent = test.IntroductionContent,
                ParentTrainingTestId = test.Id,
                PassMarks = test.PassMarks,
                PassPoints = test.PassPoints,
                ReferenceId = test.ReferenceId,
                TestDuration = test.TestDuration,
                TestExpiryDate = test.TestExpiryDate,
                TestReview = test.TestReview,
                TestTitle = test.TestTitle,
                TestTrophy = test.TestTrophy,
                TrainingGuide = test.TrainingGuide,
                TrainingGuideId = test.TrainingGuideId,
                TrophyName = test.TrophyName,
                Version = test.Version + 1,
                MaximumNumberOfRewites = test.MaximumNumberOfRewites,
                DisableQuestionRandomization = test.DisableQuestionRandomization,
                EmailSummaryOnCompletion = test.EmailSummaryOnCompletion,
                HighlightAnswersOnSummary = test.HighlightAnswersOnSummary
            };
            _trainingTestRepository.Add(newTest);
            _trainingTestRepository.SaveChanges();
            foreach (var q in test.QuestionList.ToList())
            {
                CloneQuestion(q, newTest.Id);
            }
            _trainingTestRepository.SaveChanges();
        }
        
        private void CloneQuestion(TrainingQuestion q, Guid TestId)
        {
            var newQ = new TrainingQuestion
            {
                AnswerWeightage = q.AnswerWeightage,
                TestQuestion = q.TestQuestion,
                TestQuestionNumber = q.TestQuestionNumber,
                TrainingTestId = TestId,
                Id = Guid.NewGuid()
            };
            _questionRepository.Add(newQ);
            _questionRepository.SaveChanges();
            Domain.Customer.Models.QuestionUpload Image = null;
            Domain.Customer.Models.QuestionUpload Video = null;
            Domain.Customer.Models.QuestionUpload Audio = null;
            if (q.Image != null)
                Image = CloneUpload(q.Image, newQ.Id);
            if (q.Video != null)
                Video = CloneUpload(q.Video, newQ.Id);
            if (q.Audio != null)
                Audio = CloneUpload(q.Audio, newQ.Id);
            newQ.Image = Image;
            newQ.Video = Video;
            newQ.Audio = Audio;
            _questionRepository.SaveChanges();
            Guid ca;
            if (Guid.TryParse(q.CorrectAnswer, out ca))
            {
                foreach (var a in q.TestAnswerList.Where(x => x.Option != null && !x.Option.Equals("[Option]")).OrderBy(x => x.Position).ToList())
                {
                    a.Correct = false;
                    if (ca.Equals(a.Id))
                        a.Correct = true;
                    var ac = CloneAnswer(a, q.Id);
                    newQ.TestAnswerList.Add(ac);
                }
                _questionRepository.SaveChanges();
            }
            else
                throw new Exception($"No answer found for question : {q.Id}");
            newQ.CorrectAnswer = newQ.TestAnswerList.FirstOrDefault(x => x.Correct)?.Id.ToString();
            _questionRepository.SaveChanges();
        }
        
        private Domain.Customer.Models.QuestionUpload CloneUpload(Domain.Customer.Models.QuestionUpload qUpload, Guid qId)
        {
            var upload = new FileUploads
            {
                Id = Guid.NewGuid(),
                ContentType = qUpload.Upload.ContentType,
                Data = qUpload.Upload.Data,
                Name = qUpload.Upload.Name,
                Description = qUpload.Upload.Description,
                Type = qUpload.Upload.Type
            };
            var result = new CommandDispatcher().Dispatch(new SaveUploadCommand
            {
                FileUploadV = new UploadModel
                {
                    Id = upload.Id.ToString(),
                    ContentType = upload.ContentType,
                    Data = upload.Data,
                    Name = upload.Name,
                    Description = upload.Description,
                    Type = upload.Type
                }
            });
            if (result.Validation.Any())
                throw new Exception(result.Validation.First().Message);

            var model = new Domain.Customer.Models.QuestionUpload
            {
                Id = Guid.NewGuid(),
                Upload = _uploadRepository.Find(upload.Id),
                TrainingQuestion = _questionRepository.Find(qId)
            };
            _questionUploadRepository.Add(model);
            _questionUploadRepository.SaveChanges();
            return model;
        }

        private TestAnswer CloneAnswer(TestAnswer answer, Guid qId)
        {
            var model = new TestAnswer
            {
                Correct = answer.Correct,
                Option = answer.Option,
                TrainingQuestionId = qId,
                Position = answer.Position,
                Id = Guid.NewGuid()
            };
            _answerRepository.Add(model);
            _answerRepository.SaveChanges();
            return model;
        }
    }
}
