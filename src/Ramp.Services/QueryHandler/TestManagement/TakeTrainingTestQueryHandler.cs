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
    public class TakeTrainingTestQueryHandler : QueryHandlerBase<TakeTrainingTestQueryParameter, TrainingTestViewModel>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public TakeTrainingTestQueryHandler(IRepository<TrainingTest> trainingTestRepository)
        {
            _trainingTestRepository = trainingTestRepository;
        }

        public override TrainingTestViewModel ExecuteQuery(TakeTrainingTestQueryParameter queryParameters)
        {
            var trainingTestViewModel = new TrainingTestViewModel();

            TrainingTest trainingTestModelToEdit = _trainingTestRepository.Find(queryParameters.TrainingTestId);

            if (trainingTestModelToEdit != null)
            {
                trainingTestViewModel.TestTitle = trainingTestModelToEdit.TestTitle;
                trainingTestViewModel.SelectedTrainingGuideId = trainingTestModelToEdit.TrainingGuideId;
                trainingTestViewModel.IntroductionContent = trainingTestModelToEdit.IntroductionContent;
                trainingTestViewModel.PassMarks = trainingTestModelToEdit.PassMarks;
                trainingTestViewModel.TestDuration = trainingTestModelToEdit.TestDuration;
                trainingTestViewModel.TrainingTestId = trainingTestModelToEdit.Id;
                trainingTestViewModel.ParentTrainingTestId = trainingTestModelToEdit.ParentTrainingTestId;
                trainingTestViewModel.ReferenceId = trainingTestModelToEdit.ReferenceId;
                trainingTestViewModel.TestDuration = trainingTestModelToEdit.TestDuration;
                trainingTestViewModel.DraftEditDate = trainingTestModelToEdit.DraftEditDate;
                trainingTestViewModel.TrainingGuideName = trainingTestModelToEdit.TrainingGuide.Title;
                trainingTestViewModel.TestExpiryDate = trainingTestModelToEdit.TestExpiryDate;
                trainingTestViewModel.PassPoints = trainingTestModelToEdit.PassPoints;

                var questionList = new List<TrainingTestQuestionViewModel>();
                foreach (TrainingQuestion trainingQuestion in trainingTestModelToEdit.QuestionList.Where(x => !string.IsNullOrWhiteSpace(x.CorrectAnswer) && x.TestAnswerList.Count > 0).ToList())
                {
                    Guid correctAnswerId = Guid.Empty;
                    if (!string.IsNullOrWhiteSpace(trainingQuestion.CorrectAnswer))
                        Guid.TryParse(trainingQuestion.CorrectAnswer, out correctAnswerId);
                    if (correctAnswerId != Guid.Empty)
                    {
                        var trainingTestQuestionViewModel = new TrainingTestQuestionViewModel
                        {
                            TestQuestion = trainingQuestion.TestQuestion,
                            CorrectAnswerId = System.Guid.Parse(trainingQuestion.CorrectAnswer),
                            ImageName = trainingQuestion.ImageName,
                            VideoName = trainingQuestion.VideoName,
                            AudioName = trainingQuestion.Audio != null ? trainingQuestion.Audio.Upload.Description : null,
                            TestQuestionNumber = trainingQuestion.TestQuestionNumber,
                        };
                        trainingTestQuestionViewModel.CorrectAnswerId = correctAnswerId;
                        foreach (TestAnswer answer in trainingQuestion.TestAnswerList.OrderBy(x => x.Position).ToList())
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
                }
                foreach (TrainingTestQuestionViewModel source in questionList.OrderBy(m => m.TestQuestionNumber))
                {
                    trainingTestViewModel.QuestionsList.Add(source);
                }
            }
            return trainingTestViewModel;
        }
    }
}