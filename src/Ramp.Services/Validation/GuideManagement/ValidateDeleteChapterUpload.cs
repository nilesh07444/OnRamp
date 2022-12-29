using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Validation.GuideManagement
{
    public class ValidateDeleteChapterUpload : IValidator<DeleteChapterUploadCommand>
    {
        private readonly IRepository<ChapterUpload> _chapterUploadRepository;

        public ValidateDeleteChapterUpload(IRepository<ChapterUpload> chapterUploadRepository)
        {
            _chapterUploadRepository = chapterUploadRepository;
        }

        public IEnumerable<IValidationResult> Validate(DeleteChapterUploadCommand argument)
        {
            var result = new List<ValidationResult>();
            if (_chapterUploadRepository.Find(argument.ChapterUploadId) == null)
                result.Add(new ValidationResult("Error", "Chapter upload could not be found"));
            return result;
        }
    }
}