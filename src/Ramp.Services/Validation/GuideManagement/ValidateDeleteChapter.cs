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
    public class ValidateDeleteChapter : IValidator<DeleteChapterCommand>
    {
        private readonly IRepository<TraningGuideChapter> _trainingGuideChapterRepository;

        public ValidateDeleteChapter(IRepository<TraningGuideChapter> trainingGuideChapterRepository)
        {
            _trainingGuideChapterRepository = trainingGuideChapterRepository;
        }

        public IEnumerable<IValidationResult> Validate(DeleteChapterCommand argument)
        {
            var result = new List<ValidationResult>();
            if (!_trainingGuideChapterRepository.List.Any(c => c.Id.Equals(argument.TraningGuideChapterId)))
                result.Add(new ValidationResult("Error", "The Chapter could not be found"));
            else
            {
                var chapterListforTrainingGuide =
                    _trainingGuideChapterRepository.List.Where(c => c.TraningGuidId.Equals(argument.TrainingGuideId))
                        .ToList();
                if (chapterListforTrainingGuide.Count <= 2)
                    result.Add(new ValidationResult("Error", "Total Chapters Cannot be less than 2"));
            }
            return result;
        }
    }
}