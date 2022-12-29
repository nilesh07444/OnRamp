using Common.Data;
using Common.Query;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.Catagories;
using Ramp.Contracts.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.Categories
{
    public class AllCategoriesQueryHandler : QueryHandlerBase<AllCategoriesQueryParameter,
        List<CategoryViewModel>>
    {
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;

        public AllCategoriesQueryHandler(IRepository<Domain.Customer.Models.Categories> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public override List<CategoryViewModel> ExecuteQuery(AllCategoriesQueryParameter queryParameters)
        {
            var categoryList = _categoryRepository.GetAll();
            return categoryList.Select(category => new CategoryViewModel
            {
                Id = category.Id,
                CategorieTitle = category.CategoryTitle,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId
            }).ToList();
        }
    }
}