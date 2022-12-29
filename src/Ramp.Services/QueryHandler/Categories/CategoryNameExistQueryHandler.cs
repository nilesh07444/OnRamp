using Common.Data;
using Common.Query;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.Catagories;
using Ramp.Contracts.ViewModel;
using System;
using System.Linq;

namespace Ramp.Services.QueryHandler.Categories
{
    public class CategoryNameExistQueryHandler :
        QueryHandlerBase<CategoryNameExistsQueryParameter, RemoteValidationResponseViewModel>
    {
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;

        public CategoryNameExistQueryHandler(IRepository<Domain.Customer.Models.Categories> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public override RemoteValidationResponseViewModel ExecuteQuery(CategoryNameExistsQueryParameter queryParameters)
        {
            Func<Domain.Customer.Models.Categories, bool> categoryNameAlreadyInUsePredicate;

            categoryNameAlreadyInUsePredicate =
                    u => u.CategoryTitle.Equals(queryParameters.CategoryName) && u.ParentCategoryId.Equals(queryParameters.CategoryId);

            return (new RemoteValidationResponseViewModel
            {
                Response = _categoryRepository.GetAll().ToList().Any(categoryNameAlreadyInUsePredicate)
            });
        }
    }
}