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
    public class DeleteTestAnswerCommandValidator : IValidator<DeleteTestAnswerCommand>
    {
        private readonly IRepository<TestAnswer> _answerRepository;
        public DeleteTestAnswerCommandValidator(IRepository<TestAnswer> answerRepository)
        {
            _answerRepository = answerRepository;
        }
        public IEnumerable<IValidationResult> Validate(DeleteTestAnswerCommand argument)
        {
            var result = new List<ValidationResult>();
            if (_answerRepository.Find(argument.Model.TestAnswerId) == null)
                result.Add(new ValidationResult("Answer not found"));
            return result;
        }
    }
}
