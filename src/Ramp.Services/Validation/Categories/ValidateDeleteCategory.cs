using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Validation.Categories
{
    public class ValidateDeleteCategory : IValidator<DeleteCategoryCommandParamter>
    {
        private readonly IRepository<Domain.Customer.Models.Categories> _categoriesRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public ValidateDeleteCategory(IRepository<Domain.Customer.Models.Categories> categoriesRepository,
            IRepository<TrainingGuide> trainingGuideRepository)
        {
            _categoriesRepository = categoriesRepository;
            _trainingGuideRepository = trainingGuideRepository;
        }

        public IEnumerable<IValidationResult> Validate(DeleteCategoryCommandParamter argument)
        {
            var result = new List<ValidationResult>();
            var category = _categoriesRepository.Find(argument.Id);
            if (category == null)
                result.Add(new ValidationResult("Error", "Cannot find the spesified category"));
            else {
                var IdsToDelete = new List<Guid>();
                var searchDomain = _categoriesRepository.List.ToList();
                FindChildren(category, IdsToDelete, searchDomain);
                IdsToDelete.Add(category.Id);
                foreach (var categoryId in IdsToDelete)
                {
                    var c = _categoriesRepository.Find(categoryId);
                    if (c == null)
                    {
                        result.Add(new ValidationResult("Error", "Cannot find one or more of the category's siblings"));
                    }
                    else
                    {
                        //var linkedTrainingGuides = _trainingGuideRepository.List.Any(t => t.Category.Id.Equals(c.Id));
                        //if (linkedTrainingGuides)
                        //{
                        //    result.Add(new ValidationResult("Error", "Cannot delete category with linked playbooks"));
                        //}
                    }
                }
            }
            return result;
        }

        private void FindChildren(Domain.Customer.Models.Categories category, List<Guid> childIds, List<Domain.Customer.Models.Categories> searchDomain)
        {
            var children = searchDomain.Where(c => c.ParentCategoryId.Equals(category.Id)).ToList();
            if (children.Count > 0)
            {
                childIds.AddRange(children.Select(c => c.Id).ToList());
                children.ForEach(c => FindChildren(c, childIds, searchDomain));
            }
            else
            {
                return;
            }
        }
    }
}