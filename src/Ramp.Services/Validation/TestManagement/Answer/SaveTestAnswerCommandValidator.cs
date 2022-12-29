using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement.Answer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Validation.TestManagement.Answer
{
    public class SaveTestAnswerCommandValidator : IValidator<SaveTestAnswerCommand>
    {
        private readonly IRepository<TestAnswer> _answerRepository;
        private readonly IRepository<TrainingQuestion> _questionRepository;
        public SaveTestAnswerCommandValidator(IRepository<TestAnswer> answerRepository,
            IRepository<TrainingQuestion> questionRepository)
        {
            _answerRepository = answerRepository;
            _questionRepository = questionRepository;
        }
        public IEnumerable<IValidationResult> Validate(SaveTestAnswerCommand argument)
        {
            var result = new List<ValidationResult>();
            if (_questionRepository.Find(argument.Model.TrainingQuestionId) == null && _questionRepository.Find(argument.QuestionId) == null)
                result.Add(new ValidationResult("No Question found to bound to"));
            return result;
        }
    }
}
