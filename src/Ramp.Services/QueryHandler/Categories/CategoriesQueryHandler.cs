using Common;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.Catagories;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;

namespace Ramp.Services.QueryHandler.Categories
{
    public class CategoriesQueryHandler : IQueryHandler<CategoriesQueryParameter, CategoryViewModelLong>,
                                          IQueryHandler<FetchByIdQuery,Domain.Customer.Models.Categories>
    {
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;

        public CategoriesQueryHandler(IRepository<Domain.Customer.Models.Categories> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public CategoryViewModelLong ExecuteQuery(CategoriesQueryParameter queryParameters)
        {
            var categoryViewModel = new CategoryViewModelLong();
            if (queryParameters.Id != null && queryParameters.Id != Guid.Empty)
            {
                var category = _categoryRepository.Find(queryParameters.Id);
                if (category != null)
                    categoryViewModel.CategorieViewModel = Project.CategoryViewModelFrom(category);
            }
            var categoryList = _categoryRepository.GetAll();

            foreach (var category in categoryList)
            {
                categoryViewModel.CategoryViewModelList.Add(Project.CategoryViewModelFrom(category));
            }

            return categoryViewModel;
        }

        public Domain.Customer.Models.Categories ExecuteQuery(FetchByIdQuery query)
        {
            if (query.Id != null)
                return _categoryRepository.Find(query.Id.ToString().ConvertToGuid());
            return null;
        }
    }
}