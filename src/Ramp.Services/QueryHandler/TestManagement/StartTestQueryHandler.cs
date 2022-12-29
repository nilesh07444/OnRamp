using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class StartTestQueryHandler : QueryHandlerBase<StartTestQueryParameter, TestViewModel>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public StartTestQueryHandler(IRepository<TrainingTest> trainingTestRepository)
        {
            _trainingTestRepository = trainingTestRepository;
        }

        public override TestViewModel ExecuteQuery(StartTestQueryParameter queryParameters)
        {
            var model = new TestViewModel();
            var trainingTest = _trainingTestRepository.Find(queryParameters.TrainingTestId);
            if (trainingTest != null)
            {
                model = new TestViewModel
                {
                    TestTitle = trainingTest.TestTitle,
                    SelectedTrainingGuideId = trainingTest.TrainingGuideId,
                    IntroductionContent = trainingTest.IntroductionContent,
                    PassMarks = trainingTest.PassMarks,
                    TestDuration = trainingTest.TestDuration,
                    TrainingTestId = trainingTest.Id,
                    ReferenceId = trainingTest.ReferenceId,
                    DraftEditDate = trainingTest.DraftEditDate,
                    TrainingGuideName = trainingTest.TrainingGuide.Title
                };

                var questionList = new List<QuestionsViewModel>();
                var list = trainingTest.QuestionList.Where(x => !string.IsNullOrWhiteSpace(x.CorrectAnswer) && x.TestAnswerList.Count > 0).OrderByDescending(x => x.TestQuestionNumber).ToList();
                var size = list.Count;

                while (size > 0)
                {
                    size--;
                    var index = trainingTest.DisableQuestionRandomization ? size : (new Random()).Next(0, size);
                    var trainingQuestion = list[index];
                    list[index] = list[size];
                    var trainingTestQuestionViewModel = new QuestionsViewModel
                    {
                        TrainingTestQuestionId = trainingQuestion.Id,
                        TestQuestion = trainingQuestion.TestQuestion,
                        CorrectAnswer = trainingQuestion.CorrectAnswer,
                        TestQuestionNumber = trainingQuestion.TestQuestionNumber,
                    };
                    if (trainingQuestion.Image != null)
                    {
                        trainingTestQuestionViewModel.ImageName = trainingQuestion.Image.Upload.Description;
                        trainingTestQuestionViewModel.ImageUrl = string.IsNullOrWhiteSpace(queryParameters.UploadThumbnailUrlBase) ? string.Empty : queryParameters.UploadThumbnailUrlBase.Replace(Guid.Empty.ToString(), trainingQuestion.Image.Upload.Id.ToString());
                    }
                    if (trainingQuestion.Video != null)
                    {
                        trainingTestQuestionViewModel.VideoName = trainingQuestion.Video.Upload.Description;
                        trainingTestQuestionViewModel.VideoUrl = string.IsNullOrWhiteSpace(queryParameters.UploadUrlBase) ? string.Empty : queryParameters.UploadUrlBase.Replace(Guid.Empty.ToString(), trainingQuestion.Video.Upload.Id.ToString());
                    }
                    if(trainingQuestion.Audio != null)
                    {
                        trainingTestQuestionViewModel.AudioName = trainingQuestion.Audio.Upload.Description;
                        trainingTestQuestionViewModel.AudioUrl = string.IsNullOrWhiteSpace(queryParameters.UploadUrlBase) ? string.Empty : queryParameters.UploadUrlBase.Replace(Guid.Empty.ToString(), trainingQuestion.Audio.Upload.Id.ToString());
                        trainingTestQuestionViewModel.AudioType = trainingQuestion.Audio.Upload.ContentType;
                    }
                    foreach (var answer in trainingTest.DisableQuestionRandomization ? trainingQuestion.TestAnswerList.OrderBy(x => x.Position).ToList() : trainingQuestion.TestAnswerList.ToList())
                    {
                        var testAnswer = new TestAnswerViewModel
                        {
                            TestAnswerId = answer.Id,
                            Option = answer.Option,
                            TrainingQuestionId = trainingQuestion.Id
                        };
                        trainingTestQuestionViewModel.TestAnswerList.Add(testAnswer);
                    }
                    questionList.Add(trainingTestQuestionViewModel);
                }
                model.QuestionsList = questionList;
            }
            return model;
        }
    }
}