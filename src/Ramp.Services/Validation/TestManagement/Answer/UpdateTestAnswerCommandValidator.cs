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
    public class UpdateTestAnswerCommandValidator : IValidator<UpdateTestAnswerCommand>
    {
        private readonly IRepository<TestAnswer> _answerRepository;
        public UpdateTestAnswerCommandValidator(IRepository<TestAnswer> answerRepository)
        {
            _answerRepository = answerRepository;
        }
        public IEnumerable<IValidationResult> Validate(UpdateTestAnswerCommand argument)
        {
            var result = new List<ValidationResult>();
            if (_answerRepository.Find(argument.Model.TestAnswerId) == null)
                result.Add(new ValidationResult("No Answer found"));
            return result;
        }
    }
}
