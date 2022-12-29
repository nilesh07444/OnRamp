using Common.Command;
using Common.Data;
using Ramp.Contracts.CommandParameter.Categories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.CommandHandler.Categories
{
    public class DeleteCategoryCommandHandler : CommandHandlerBase<DeleteCategoryCommandParamter>
    {
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;

        public DeleteCategoryCommandHandler(IRepository<Domain.Customer.Models.Categories> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public override CommandResponse Execute(DeleteCategoryCommandParamter command)
        {
            var category = _categoryRepository.Find(command.Id);
            if (category != null)
            {
                var IdsToDelete = new List<Guid>();
                var searchDomain = _categoryRepository.List.ToList();
                FindChildren(category, IdsToDelete, searchDomain);
                IdsToDelete.Add(category.Id);
                foreach (var categoryId in IdsToDelete)
                {
                    var c = _categoryRepository.Find(categoryId);
                    if (c != null)
                    {
                        _categoryRepository.Delete(c);
                    }
                }
            }
            return null;
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