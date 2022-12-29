using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class MyPlaybookCategoryQueryHandler : IQueryHandler<MyPlaybookCategoryQueryParameter, CategoryViewModelLong>
    {
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuideRepository;

        public MyPlaybookCategoryQueryHandler(
            IRepository<Domain.Customer.Models.Categories> categoryRepository,
            IRepository<AssignedTrainingGuides> assignedTrainingGuideRepository)
        {
            _categoryRepository = categoryRepository;
            _assignedTrainingGuideRepository = assignedTrainingGuideRepository;
        }

        public CategoryViewModelLong ExecuteQuery(MyPlaybookCategoryQueryParameter query)
        {
            if (query.UserId.Equals(Guid.Empty))
                return null;
            var model = new CategoryViewModelLong();
            //var playbookCategories = _assignedTrainingGuideRepository.List.Where(g => g.UserId.Equals(query.UserId)).Select(c => c.TrainingGuide.Category.Id).ToList();

            //var tree = new List<Guid>();

            //playbookCategories.ForEach(cat => AllCategoriesInBranch(_categoryRepository.List.Where(c => c.Id.Equals(cat)).ToList().First(), tree, _categoryRepository.List.ToList()));

            //tree.ForEach(c => model.CategoryViewModelList.Add(Project.CategoryViewModelFrom(_categoryRepository.Find(c))));

            return model;
        }

        private void AllCategoriesInBranch(Domain.Customer.Models.Categories c, List<Guid> branch, List<Domain.Customer.Models.Categories> allCatagories)
        {
            if (c.ParentCategoryId.Equals(Guid.Empty) || c.ParentCategoryId == null)
            {
                if (!branch.Contains(c.Id))
                    branch.Add(c.Id);
                return;
            }
            else
            {
                var parents = allCatagories.Where(cat => cat.Id.Equals(c.ParentCategoryId)).Select(cat => cat.Id).ToList();
                foreach (var cat in parents)
                {
                    if (!branch.Contains(c.Id))
                        branch.Add(c.Id);
                    AllCategoriesInBranch(allCatagories.Single(x => x.Id.Equals(cat)), branch, allCatagories);
                }
            }
        }
    }
}