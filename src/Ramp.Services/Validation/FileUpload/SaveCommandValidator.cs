using Common;
using Common.Command;
using Common.Data;
using Ramp.Contracts.CommandParameter.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models;

namespace Ramp.Services.Validation.FileUpload
{
    public class SaveCommandValidator : IValidator<SaveUploadCommand>,
                                        IValidator<SaveFileUploadCommand>
    {
        private readonly IRepository<Upload> _uploadRepository;
        private readonly IRepository<Domain.Customer.Models.FileUploads> _fileUploadRepository;
        public SaveCommandValidator(
            IRepository<Upload> uploadRepository,
            IRepository<Domain.Customer.Models.FileUploads> fileUploadRepository)
        {
            _uploadRepository = uploadRepository;
            _fileUploadRepository = fileUploadRepository;
        }
        public IEnumerable<IValidationResult> Validate(SaveUploadCommand argument)
        {
            var result = new List<ValidationResult>();
            if (argument.FileUploadV == null)
                result.Add(new ValidationResult("Error", "No Upload"));
            else if (argument.FileUploadV.Data == null)
                result.Add(new ValidationResult("Error", "No Upload Data"));
            else if (string.IsNullOrWhiteSpace(argument.FileUploadV.Id))
                result.Add(new ValidationResult("Error", "No Upload Id"));
            else if (string.IsNullOrWhiteSpace(argument.FileUploadV.ContentType)
                || string.IsNullOrWhiteSpace(argument.FileUploadV.Type))
                result.Add(new ValidationResult("Error", "No Upload Type"));
            else if (_uploadRepository.Find(argument.FileUploadV.Id) != null)
                result.Add(new ValidationResult("Error", "Upload Id not unique"));
            return result;
        }

        public IEnumerable<IValidationResult> Validate(SaveFileUploadCommand argument)
        {
            var result = new List<ValidationResult>();
            if (argument.FileUploadV == null)
                result.Add(new ValidationResult("Error", "No Upload"));
            else if (argument.FileUploadV.Data == null)
                result.Add(new ValidationResult("Error", "No Upload Data"));
            else if (argument.FileUploadV.Id.Equals(Guid.Empty))
                result.Add(new ValidationResult("Error", "No Upload Id"));
            else if (string.IsNullOrWhiteSpace(argument.FileUploadV.ContentType)
                     || string.IsNullOrWhiteSpace(argument.FileUploadV.Type))
                result.Add(new ValidationResult("Error", "No Upload Type"));
            else if (_fileUploadRepository.Find(argument.FileUploadV.Id) != null)
                result.Add(new ValidationResult("Error", "Upload Id not unique"));
            return result;
        }
    }
}
