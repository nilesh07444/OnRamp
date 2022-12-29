using Common.Command;
using Common.Data;
using Ramp.Contracts.CommandParameter.TestManagement.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models;

namespace Ramp.Services.Validation.TestManagement.Question
{
    public class UpdateTrainingQuestionCommandValidator : IValidator<UpdateTrainingQuestionCommand>
    {
        private readonly IRepository<TrainingQuestion> _questionRepository;
        public UpdateTrainingQuestionCommandValidator(IRepository<TrainingQuestion> questionRepository)
        {
            _questionRepository = questionRepository;
        }
        public IEnumerable<IValidationResult> Validate(UpdateTrainingQuestionCommand argument)
        {
            var result = new List<ValidationResult>();
            if (_questionRepository.Find(argument.Model.TrainingTestQuestionId) == null)
                result.Add(new ValidationResult("Question not found"));
            return result;
        }
    }
}
