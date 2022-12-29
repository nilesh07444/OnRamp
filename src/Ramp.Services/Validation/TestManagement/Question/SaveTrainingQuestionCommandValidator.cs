using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Validation.TestManagement.Question
{
    public class SaveTrainingQuestionCommandValidator : IValidator<SaveTrainingQuestionCommand>
    {
        private readonly IRepository<TrainingTest> _testRepository;
        private readonly IRepository<TrainingQuestion> _questionRepository;
        public SaveTrainingQuestionCommandValidator(IRepository<TrainingTest> testRepository,IRepository<TrainingQuestion> questionRepository)
        {
            _testRepository = testRepository;
            _questionRepository = questionRepository;
        }
        public IEnumerable<IValidationResult> Validate(SaveTrainingQuestionCommand argument)
        {
            var result = new List<ValidationResult>();
            if (_testRepository.Find(argument.TestId) == null)
                result.Add(new ValidationResult($"No test found with id :{argument.TestId}"));
            if(argument.Model.TrainingTestQuestionId != Guid.Empty)
                if(_questionRepository.Find(argument.Model.TrainingTestQuestionId) != null)
                    result.Add(new ValidationResult($"Duplicate Primary Key for Question with id :{argument.Model.TrainingTestQuestionId}"));
            return result;
        }
    }
}
