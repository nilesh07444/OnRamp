using Common.Command;
using Common.Data;
using Ramp.Contracts.CommandParameter.Categories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.CommandHandler.Categories
{
    public class CategoriesCommandHandler : CommandHandlerBase<CategoriesCommandParameter>
    {
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;

        public CategoriesCommandHandler(IRepository<Domain.Customer.Models.Categories> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public override CommandResponse Execute(CategoriesCommandParameter command)
        {
            var categoryModel = _categoryRepository.Find(command.Id);

            List<Domain.Customer.Models.Categories> categoriList = _categoryRepository.GetAll().ToList();
            if (!categoriList.Any(u => u.CategoryTitle.Equals(command.CategoryTitle) && u.ParentCategoryId.Equals(command.ParentCategoryId)))
            {
                if (categoryModel == null)
                {
                    if (command.ParentCategoryId == Guid.Empty)
                    {
                        var categories = new Domain.Customer.Models.Categories
                        {
                            Id = command.Id,
                            CategoryTitle = command.CategoryTitle,
                            Description = command.Description,
                            CreatedUnderCompanyId = command.CreatedUnderCompanyId,
                            ParentCategoryId = null,
                        };
                        _categoryRepository.Add(categories);
                        _categoryRepository.SaveChanges();
                    }
                    else
                    {
                        var categories = new Domain.Customer.Models.Categories
                        {
                            Id = command.Id,
                            CategoryTitle = command.CategoryTitle,
                            Description = command.Description,
                            CreatedUnderCompanyId = command.CreatedUnderCompanyId,
                            ParentCategoryId = command.ParentCategoryId,
                        };
                        _categoryRepository.Add(categories);
                        _categoryRepository.SaveChanges();
                    }
                }
                else
                {
                    categoryModel.CategoryTitle = command.CategoryTitle;
                    categoryModel.Description = categoryModel.Description;
                    categoryModel.ParentCategoryId = categoryModel.ParentCategoryId;
                    _categoryRepository.SaveChanges();
                }
            }

            return null;
        }
    }
}