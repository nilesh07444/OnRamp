using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.Feedback;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class EditTrainingTestQueryHandler : QueryHandlerBase<EditTrainingTestQueryParameter, TrainingTestViewModel>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TrainingGuide> _guideRepository;

        public EditTrainingTestQueryHandler(IRepository<TrainingTest> trainingTestRepository,IRepository<TrainingGuide> guideRepository)
        {
            _trainingTestRepository = trainingTestRepository;
            _guideRepository = guideRepository;
        }

        public override TrainingTestViewModel ExecuteQuery(EditTrainingTestQueryParameter queryParameters)
        {
            var trainingTestViewModel = new TrainingTestViewModel();

            var trainingTestModel = _trainingTestRepository.Find(queryParameters.TrainingTestId);

            var guide = _guideRepository.Find(trainingTestModel.TrainingGuideId);
            var testToEdit = guide.TestVersion.CurrentVersion;

            //Select the draft version if exists.
            TrainingTest trainingTestModelToEdit = testToEdit == null ? trainingTestModel : testToEdit;

            if (trainingTestModelToEdit != null)
            {
                trainingTestViewModel.TestTitle = trainingTestModelToEdit.TestTitle;
                trainingTestViewModel.SelectedTrainingGuideId = trainingTestModelToEdit.TrainingGuideId;
                trainingTestViewModel.IntroductionContent = trainingTestModelToEdit.IntroductionContent;
                trainingTestViewModel.PassMarks = trainingTestModelToEdit.PassMarks;
                trainingTestViewModel.PassPoints = trainingTestModelToEdit.PassPoints;
                trainingTestViewModel.TestDuration = trainingTestModelToEdit.TestDuration;
                trainingTestViewModel.TrainingTestId = trainingTestModelToEdit.Id;
                trainingTestViewModel.ParentTrainingTestId = trainingTestModelToEdit.ParentTrainingTestId;
                trainingTestViewModel.ReferenceId = trainingTestModelToEdit.ReferenceId;
                trainingTestViewModel.TestDuration = trainingTestModelToEdit.TestDuration;
                trainingTestViewModel.DraftEditDate = trainingTestModelToEdit.DraftEditDate;
                trainingTestViewModel.TrainingGuideName = trainingTestModelToEdit.TrainingGuide.Title;
                trainingTestViewModel.TestExpiryDate = trainingTestModelToEdit.TestExpiryDate;
                trainingTestViewModel.TrophyName = trainingTestModelToEdit.TrophyName;
                trainingTestViewModel.TestTrophy = trainingTestModelToEdit.TestTrophy;
                trainingTestViewModel.AssignMarksToQuestions = trainingTestModelToEdit.AssignMarksToQuestions;
                trainingTestViewModel.IsTestExpiryDate = !trainingTestModelToEdit.TestExpiryDate.HasValue;
                trainingTestViewModel.TestReview = trainingTestModelToEdit.TestReview;
                trainingTestViewModel.Version = trainingTestModelToEdit.Version ?? 0;
                trainingTestViewModel.ActivePublishDate = trainingTestModelToEdit.ActivePublishDate;
                trainingTestViewModel.EnableMaximumTestRewriteFunction = trainingTestModelToEdit.MaximumNumberOfRewites.HasValue;
                trainingTestViewModel.MaximumRewrites = trainingTestModelToEdit.MaximumNumberOfRewites.HasValue ? trainingTestModelToEdit.MaximumNumberOfRewites : null;
                trainingTestViewModel.DisableQuestionRandomization = trainingTestModelToEdit.DisableQuestionRandomization;
                trainingTestViewModel.EmailSummaryOnCompletion = trainingTestModelToEdit.EmailSummaryOnCompletion;
                trainingTestViewModel.HighlightAnswersOnSummary = trainingTestModelToEdit.HighlightAnswersOnSummary;
                var questionList = new List<TrainingTestQuestionViewModel>();
                foreach (TrainingQuestion trainingQuestion in trainingTestModelToEdit.QuestionList)
                {
                    var trainingTestQuestionViewModel = new TrainingTestQuestionViewModel
                    {
                        TrainingTestQuestionId = trainingQuestion.Id,
                        TestQuestion = trainingQuestion.TestQuestion,
                        TestQuestionNumber = trainingQuestion.TestQuestionNumber,
                        AnswerWeightage = trainingQuestion.AnswerWeightage
                    };
                    var correctAnswer = System.Guid.Empty;
                    if (System.Guid.TryParse(trainingQuestion.CorrectAnswer, out correctAnswer))
                    {
                        trainingTestQuestionViewModel.CorrectAnswerId = correctAnswer;
                    }
                    else
                    {
                        var correct = trainingQuestion.TestAnswerList.FirstOrDefault(a => a.Option.Equals(trainingQuestion.CorrectAnswer));
                        if (correct != null)
                            trainingTestQuestionViewModel.CorrectAnswerId = correct.Id;
                    }

                    if (trainingQuestion.Image != null)
                    {
                        var u = trainingQuestion.Image;
                        trainingTestQuestionViewModel.ImageContainer = new FileUploadResultViewModel
                        {
                            DeleteType = "DELETE",
                            Id = u.Upload.Id,
                            InProcess = false,
                            Name = u.Upload.Name,
                            Description = u.Upload.Description,
                            Type = u.Upload.ContentType,
                            Size = u.Upload.Data.Length,
                            Progress = "100%",
                            Url = string.IsNullOrWhiteSpace(queryParameters.UploadUrlBase) ? string.Empty : queryParameters.UploadUrlBase.Replace(Guid.Empty.ToString(), $"{u.Upload.Id}"),
                            ThumbnailUrl = string.IsNullOrWhiteSpace(queryParameters.UploadThumbnailUrlBase) ? string.Empty : queryParameters.UploadThumbnailUrlBase.Replace(Guid.Empty.ToString(), $"{u.Upload.Id}"),
                            DeleteUrl = string.IsNullOrWhiteSpace(queryParameters.UploadDeleteUrlBase) ? string.Empty : queryParameters.UploadDeleteUrlBase.Replace(Guid.Empty.ToString(), $"{u.Upload.Id}"),
                        };
                    }
                    if (trainingQuestion.Video != null)
                    {
                        var u = trainingQuestion.Video;
                        trainingTestQuestionViewModel.VideoContainer = new FileUploadResultViewModel
                        {
                            DeleteType = "DELETE",
                            Id = u.Upload.Id,
                            InProcess = false,
                            Name = u.Upload.Name,
                            Description = u.Upload.Description,
                            Type = u.Upload.ContentType,
                            Size = u.Upload.Data.Length,
                            Progress = "100%",
                            Url = string.IsNullOrWhiteSpace(queryParameters.UploadUrlBase) ? string.Empty : queryParameters.UploadUrlBase.Replace(Guid.Empty.ToString(), $"{u.Upload.Id}"),
                            ThumbnailUrl = string.IsNullOrWhiteSpace(queryParameters.UploadThumbnailUrlBase) ? string.Empty : queryParameters.UploadThumbnailUrlBase.Replace(Guid.Empty.ToString(), $"{u.Upload.Id}"),
                            DeleteUrl = string.IsNullOrWhiteSpace(queryParameters.UploadDeleteUrlBase) ? string.Empty : queryParameters.UploadDeleteUrlBase.Replace(Guid.Empty.ToString(), $"{u.Upload.Id}"),
                        };
                    }
                    if(trainingQuestion.Audio != null)
                    {
                        var u = trainingQuestion.Audio;
                        trainingTestQuestionViewModel.AudioContainer = new FileUploadResultViewModel
                        {
                            DeleteType = "DELETE",
                            Id = u.Upload.Id,
                            InProcess = false,
                            Name = u.Upload.Name,
                            Description = u.Upload.Description,
                            Type = u.Upload.ContentType,
                            Size = u.Upload.Data.Length,
                            Progress = "100%",
                            Url = string.IsNullOrWhiteSpace(queryParameters.UploadUrlBase) ? string.Empty : queryParameters.UploadUrlBase.Replace(Guid.Empty.ToString(), $"{u.Upload.Id}"),
                            ThumbnailUrl = string.IsNullOrWhiteSpace(queryParameters.UploadThumbnailUrlBase) ? string.Empty : queryParameters.UploadThumbnailUrlBase.Replace(Guid.Empty.ToString(), $"{u.Upload.Id}"),
                            DeleteUrl = string.IsNullOrWhiteSpace(queryParameters.UploadDeleteUrlBase) ? string.Empty : queryParameters.UploadDeleteUrlBase.Replace(Guid.Empty.ToString(), $"{u.Upload.Id}"),
                        };
                    }

                    foreach (var answer in trainingQuestion.TestAnswerList.OrderBy(x => x.Position).ToList())
                    {
                        if (!string.IsNullOrWhiteSpace(answer.Option))
                        {
                            var testAnswer = new TestAnswerViewModel
                            {
                                TestAnswerId = answer.Id,
                                Option = answer.Option,
                                TrainingQuestionId = trainingQuestion.Id,
                                Correct = answer.Correct,
                                Position = answer.Position
                            };
                            trainingTestQuestionViewModel.TestAnswerList.Add(testAnswer);
                        }
                    }

                    questionList.Add(trainingTestQuestionViewModel);
                }
                foreach (TrainingTestQuestionViewModel source in questionList.OrderBy(m => m.TestQuestionNumber))
                {
                    trainingTestViewModel.QuestionsList.Add(source);
                }
            }

            var feedbacks = new QueryExecutor().Execute<GetFeedbackForTestQueryParameter, List<FeedbackViewModel>>(new GetFeedbackForTestQueryParameter()
            {
                ReferenceId = trainingTestViewModel.ReferenceId
            });

            trainingTestViewModel.Feedback = feedbacks;
            return trainingTestViewModel;
        }
    }
}