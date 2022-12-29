using Common.Command;
using Common.Data;
using Ramp.Contracts.CommandParameter.TestManagement.QuestionUpload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Validation.TestManagement.QuestionUpload
{
    public class DeleteQuestionUploadCommandValidator : IValidator<DeleteQuestionUploadCommand>
    {
        private readonly IRepository<Domain.Customer.Models.QuestionUpload> _questionUploadRepository;
        public DeleteQuestionUploadCommandValidator(IRepository<Domain.Customer.Models.QuestionUpload> questionUploadRepository)
        {
            _questionUploadRepository = questionUploadRepository;
        }
        public IEnumerable<IValidationResult> Validate(DeleteQuestionUploadCommand argument)
        {
            var result = new List<ValidationResult>();
            if (_questionUploadRepository.Find(argument.QuestionUploadId) == null)
                result.Add(new ValidationResult($"No questionUpload found with id :{argument.QuestionUploadId}"));

            return result;
        }
    }
}
