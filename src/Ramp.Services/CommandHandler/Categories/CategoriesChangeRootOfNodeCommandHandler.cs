using Common.Command;
using Common.Data;
using Common.Query;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.Categories
{
    public class CategoriesChangeRootOfNodeCommandHandler : CommandHandlerBase<CategoriesChangeRootOfNodeCommandParameter>
    {
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;

        public CategoriesChangeRootOfNodeCommandHandler(IRepository<Domain.Customer.Models.Categories> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public override CommandResponse Execute(CategoriesChangeRootOfNodeCommandParameter command)
        {
            var categoryModel = _categoryRepository.Find(command.Id);
            Guid? parentId;
            if (command.ParentCategoryId == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                parentId = null;
            else
                parentId = command.ParentCategoryId;
            List<Domain.Customer.Models.Categories> categoriList = _categoryRepository.GetAll().ToList();
            if (!categoriList.Any(u => u.CategoryTitle.Equals(command.CategoryTitle) && u.ParentCategoryId.Equals(parentId)))
            {
                if (categoryModel != null)
                {
                    categoryModel.CategoryTitle = categoryModel.CategoryTitle;
                    categoryModel.Description = categoryModel.Description;
                    categoryModel.ParentCategoryId = parentId;
                    _categoryRepository.SaveChanges();
                }
            }

            return null;
        }
    }
}