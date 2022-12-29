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
    public class UpdateQuestionUploadCommandValidator : IValidator<UpdateQuestionUploadCommand>
    {
        private readonly IRepository<FileUploads> _fileUploadRepository;
        private readonly IRepository<Domain.Customer.Models.QuestionUpload> _questionUploadRepository;
        public UpdateQuestionUploadCommandValidator(IRepository<FileUploads> fileUploadRepository,
            IRepository<Domain.Customer.Models.QuestionUpload> questionUploadRepository)
        {
            _fileUploadRepository = fileUploadRepository;
            _questionUploadRepository = questionUploadRepository;
        }
        public IEnumerable<IValidationResult> Validate(UpdateQuestionUploadCommand argument)
        {
            var result = new List<ValidationResult>();
            if (_fileUploadRepository.Find(argument.Model.Id) == null)
                result.Add(new ValidationResult($"No upload found with id : {argument.Model.Id}"));
            var qUpload = _questionUploadRepository.Find(argument.QuestionUploadId);
            if (qUpload == null)
                result.Add(new ValidationResult($"No questionUpload found with id: {argument.QuestionUploadId}"));
            else if (qUpload != null && qUpload.Upload == null)
                result.Add(new ValidationResult($"No linked upload found on questionUpload with id: {argument.QuestionUploadId}"));

            return result;
        }
    }
}
