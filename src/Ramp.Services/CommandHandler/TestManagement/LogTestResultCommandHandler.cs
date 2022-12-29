using Common.Command;
using Ramp.Contracts.CommandParameter.TestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;
using Domain.Customer.Models;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class LogTestResultCommandHandler : ICommandHandlerBase<LogTestResultCommand>
    {
        private readonly IRepository<TestAnswer> _testAnswerRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TestUserAnswer> _testUserAnswerRepository;
        public LogTestResultCommandHandler(IRepository<TestAnswer> testAnswerRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<TestUserAnswer> testUserAnswerRepository)
        {
            _testAnswerRepository = testAnswerRepository;
            _testResultRepository = testResultRepository;
            _testUserAnswerRepository = testUserAnswerRepository;
        }
        public CommandResponse Execute(LogTestResultCommand command)
        {
            if (!command.ResultId.Equals(Guid.Empty) && command.Test == null)
                return null;

            var testResult = _testResultRepository.Find(command.ResultId);
            if(testResult != null)
            {
                foreach(var userAnswer in command.Test.QuestionsList)
                {
                    if (!string.IsNullOrWhiteSpace(userAnswer.CorrectAnswer))
                    {
                        var answer = _testAnswerRepository.Find(Guid.Parse(userAnswer.CorrectAnswer));
                        if (answer != null)
                        {
                            var testUserAnswer = new TestUserAnswer
                            {
                                Id = Guid.NewGuid(),
                                Answer = answer,
                                Result = testResult
                            };
                            _testUserAnswerRepository.Add(testUserAnswer);
                            _testUserAnswerRepository.SaveChanges();
                        }
                    }
                }
            }
            return null;
        }
    }
}
