using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class MigrateTestResultCommandHandler : ICommandHandlerBase<MigrateTestResultCommandParameter>
    {
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TrainingTest> _testRepository;

        public MigrateTestResultCommandHandler(IRepository<TestResult> testResultRepository,
            IRepository<TrainingTest> testRepository)
        {
            _testResultRepository = testResultRepository;
            _testRepository = testRepository;
        }

        public CommandResponse Execute(MigrateTestResultCommandParameter command)
        {
            var results = _testResultRepository.List.ToList();
            foreach (var test in results)
            {
                var total = 0;
                if (test.TrainingTestId.HasValue)
                {
                    var actualTest = _testRepository.Find(test.TrainingTestId);
                    actualTest.QuestionList.ForEach(q => total += q.AnswerWeightage);
                    if (total == 0)
                    {
                        total = actualTest.QuestionList.Count;
                    }
                    var result = _testResultRepository.Find(test.Id);
                    result.Points = actualTest.PassPoints;
                    result.Total = total;
                    _testResultRepository.SaveChanges();
                }
            }

            return null;
        }
    }
}