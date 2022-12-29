using Common.Command;
using Common.Data;
using Ramp.Contracts.CommandParameter.TestManagement.QuestionUpload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models;
namespace Ramp.Services.Validation.TestManagement.QuestionUpload
{
    public class SaveQuestionUploadCommandValidator : IValidator<SaveQuestionUploadCommand>
    {
        private readonly IRepository<FileUploads> _fileUploadRepository;
        private readonly IRepository<TrainingQuestion> _questionRepository;
        public SaveQuestionUploadCommandValidator(IRepository<FileUploads> fileUploadRepository,
            IRepository<TrainingQuestion> questionRepository)
        {
            _fileUploadRepository = fileUploadRepository;
            _questionRepository = questionRepository;
        }
        public IEnumerable<IValidationResult> Validate(SaveQuestionUploadCommand argument)
        {
            var result = new List<ValidationResult>();
            if (_fileUploadRepository.Find(argument.Model.Id) == null)
                result.Add(new ValidationResult($"No upload found with id :{argument.Model.Id}"));
            if (_questionRepository.Find(argument.QuestionId) == null)
                result.Add(new ValidationResult($"No question found with id :{argument.QuestionId}"));
            return result;
        }
    }
}
