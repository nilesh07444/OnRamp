using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class TestResultQueryHandler : IQueryHandler<TestResultQueryParameter, TestResultViewModel>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public TestResultQueryHandler(IRepository<TrainingTest> trainingTestRepository)
        {
            _trainingTestRepository = trainingTestRepository;
        }

        public TestResultViewModel ExecuteQuery(TestResultQueryParameter query)
        {
            var testResultViewModel = new TestResultViewModel();

            var trainingTest = _trainingTestRepository.Find(query.TestViewModel.TrainingTestId);
            int numberRightAnswers = 0;
            int numberWrongAnswers = 0;
            var numberOfPointScored = 0;
            int numberOfUnattemptedQuestions = 0;
            int maximumMarks = 0;
            var unAnsweredQuestions = new List<Guid>();
            var incorrectQuestions = new List<Guid>();
            var correctQuestions = new List<Guid>();
            var questionResults = new List<QuestionResultViewModel>();

            var list = trainingTest.QuestionList.Select(trainingQuestion => new TrainingTestQuestionViewModel
            {
                CorrectAnswerId = System.Guid.Parse(trainingQuestion.CorrectAnswer),
                AnswerWeightage = trainingQuestion.AnswerWeightage,
                TrainingTestQuestionId = trainingQuestion.Id,
                TestQuestionNumber = trainingQuestion.TestQuestionNumber,
                TestQuestion = trainingQuestion.TestQuestion
            }).ToList();

            var trainingQuestionlist = list.OrderBy(m => m.TestQuestionNumber).ToList();

            foreach (var testQuestionViewModel in trainingQuestionlist)
            {
                maximumMarks += testQuestionViewModel.AnswerWeightage;

                foreach (var userQuestionsAnswers in query.TestViewModel.QuestionsList)
                {
                    if (userQuestionsAnswers.TrainingTestQuestionId.Equals(testQuestionViewModel.TrainingTestQuestionId))
                    {
                        if (userQuestionsAnswers.CorrectAnswer != null && userQuestionsAnswers.CorrectAnswer.Equals(testQuestionViewModel.CorrectAnswerId.ToString()))
                        {
                            numberOfPointScored += testQuestionViewModel.AnswerWeightage;
                            numberRightAnswers++;
                            correctQuestions.Add(testQuestionViewModel.TrainingTestQuestionId);
                            questionResults.Add(new QuestionResultViewModel { Correct = true, Question = testQuestionViewModel.TestQuestion });
                        }
                        else
                        {
                            if (userQuestionsAnswers.CorrectAnswer == null)
                            {
                                numberOfUnattemptedQuestions++;
                                unAnsweredQuestions.Add(testQuestionViewModel.TrainingTestQuestionId);
                                questionResults.Add(new QuestionResultViewModel { Correct = false, Question = testQuestionViewModel.TestQuestion });
                            }
                            else
                            {
                                numberWrongAnswers++;
                                incorrectQuestions.Add(testQuestionViewModel.TrainingTestQuestionId);
                                questionResults.Add(new QuestionResultViewModel { Correct = false, Question = testQuestionViewModel.TestQuestion });
                            }
                        }
                    }
                }
            }

            decimal numberOfPointScoredby = 0;
            if (maximumMarks > 0)
            {
                numberOfPointScoredby = ((decimal.Parse(numberOfPointScored.ToString(CultureInfo.InvariantCulture)) /
                    (decimal.Parse(maximumMarks.ToString(CultureInfo.InvariantCulture))))) * 100;
            }
            else
            {
                numberOfPointScoredby = (numberOfPointScored) * 100;
            }
            testResultViewModel.TestResult = Math.Round(numberOfPointScoredby, 2) >= trainingTest.PassMarks;

            testResultViewModel.TestId = trainingTest.Id;
            testResultViewModel.TrainingGuideReferenceId = trainingTest.TrainingGuide.ReferenceId;
            testResultViewModel.NumberOfRightAnswers = numberRightAnswers;
            testResultViewModel.NumberOfWrongAnswers = numberWrongAnswers;
            testResultViewModel.TestTitle = trainingTest.TestTitle;
            testResultViewModel.TrainingGuideTitle = trainingTest.TrainingGuide.Title;
            testResultViewModel.TotalMarksScored = numberOfPointScored;
            testResultViewModel.NumberOfUnattemptedQuestions = numberOfUnattemptedQuestions;
            testResultViewModel.TestResultPers = numberOfPointScoredby;
            testResultViewModel.TrainingGuideId = trainingTest.TrainingGuide.Id;
            testResultViewModel.TrainingGuideTitle = trainingTest.TrainingGuide.Title;
            testResultViewModel.TestSnapshot = Project.TrainingTestViewModelFrom(trainingTest);
            testResultViewModel.TestReview = trainingTest.TestReview;
            testResultViewModel.QuestionResults = questionResults;
            testResultViewModel.DisableQuestionRandomization = trainingTest.DisableQuestionRandomization;
            testResultViewModel.PassPoints = testResultViewModel.TestResult ?  trainingTest.PassPoints : 0;
            testResultViewModel.EmailSummaryOnCompletion = trainingTest.EmailSummaryOnCompletion;
            testResultViewModel.MarksOutOff = maximumMarks;
            testResultViewModel.HighlightAnswersOnSummary = trainingTest.HighlightAnswersOnSummary;
            return testResultViewModel;
        }
    }
}