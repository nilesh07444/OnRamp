using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;
using Ramp.Contracts.QueryParameter.Reporting;
using Common.Data;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Test;
using VirtuaCon;

namespace Ramp.Services.QueryHandler.Reporting
{
    public class FocusAreaReportQueryHandler : IQueryHandler<FocusAreaReportQuery, FocusAreaReportDataSources>
    {
        private readonly IRepository<Test> _testRepository;
        private readonly IRepository<TestQuestion> _testQuestionRepository;
        private readonly IRepository<TestQuestionAnswer_Result> _testQuestionAnswerResultRepository;

        public FocusAreaReportQueryHandler(
            IRepository<Test> testRepository,
            IRepository<TestQuestion> testQuestionRepository,
            IRepository<TestQuestionAnswer_Result> testQuestionAnswerResultRepository)
        {
            _testRepository = testRepository;
            _testQuestionRepository = testQuestionRepository;
            _testQuestionAnswerResultRepository = testQuestionAnswerResultRepository;
        }
        public FocusAreaReportDataSources ExecuteQuery(FocusAreaReportQuery query)
        {
            var model = new FocusAreaReportDataSources();
            if (!string.IsNullOrEmpty(query.TestId))
            {
                model.SelectedTestId = query.TestId;
                var test = _testRepository.Find(query.TestId);
                if (test != null)
                {
                    model.Test = new[] { new FocusAreaReportTest { Id = test.Id, TestTitle = test.Title } };
                    model.Questions = test.Questions.OrderBy(x => x.Number).Select(s => new FocusAreaReportQuestion
                    {
                        TestTitle = test.Title,
                        Id = s.Id,
                        Question = s.Question,
                        Rank = s.Number,
                    }).ToList();
                    foreach(var q in model.Questions)
                    {
                        var question = _testQuestionRepository.Find(q.Id);
                        var options = question.Answers.Select(a => new FocusAreaReportOption
                        {
                            Correct = question.CorrectAnswerId == a.Id,
                            Id = a.Id,
                            Count = _testQuestionAnswerResultRepository.List.Count(x => x.AnswerId.Equals(a.Id) && x.Selected),
                            Option = a.Option,
                            Rank = a.Number
                        }).ToList();
                        q.Options = options.OrderBy(x => x.Rank).ToList();
                        var incorrectAnswers = 0;
                        var correctAnswers = 0;
                        foreach (var r in options)
                        {
                            if (!r.Correct)
                            {
                                incorrectAnswers += r.Count;
                            }
                            else
                            {
                                correctAnswers += r.Count;
                            }
                        }
                        q.IncorrectAnswers = incorrectAnswers;
                        q.CorrectAnswers = correctAnswers;
                    }
                }
            }

            model.TestDropDown = _testRepository.List
                .Where(t => t.Deleted == false && t.DocumentStatus != DocumentStatus.Draft)
                .OrderBy(x => x.ReferenceId).ThenBy(x => x.Title).Select(
                    x => new SerializableSelectListItem
                    {
                        Selected = false,
                        Text = x.Title,
                        Value = x.Id
                    }).ToList();
            return model;
        }
    }
}
