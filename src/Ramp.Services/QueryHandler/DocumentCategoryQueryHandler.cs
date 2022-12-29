using Common;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Query.DocumentCategory;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler
{
    public class DocumentCategoryQueryHandler : IQueryHandler<FetchByIdQuery, DocumentCategory>,
                                                IQueryHandler<DocumentCategoryListQuery,IEnumerable<JSTreeViewModel>>,
                                                IQueryHandler<DocumentCategoryAndDescendantsIdQuery, IEnumerable<string>>
    {
        private readonly ITransientReadRepository<DocumentCategory> _repository;
        public DocumentCategoryQueryHandler(ITransientReadRepository<DocumentCategory> repository)
        {
            _repository = repository;
        }
        public DocumentCategory ExecuteQuery(FetchByIdQuery query)
        {
            if (query.Id == null)
                return null;
            return _repository.List.AsQueryable().FirstOrDefault(x => x.Id == query.Id.ToString());
        }

        public IEnumerable<JSTreeViewModel> ExecuteQuery(DocumentCategoryListQuery query)
        {
            var categories = _repository.List.AsQueryable().OrderBy(x => x.Title).Select(Project.DocumentCategory_JSTreeViewModel).ToList();
			categories.ForEach(x => x.text = WebUtility.HtmlDecode(x.text));
			var defaultCategory = categories.First(x => x.text == "Default");
            categories.Remove(defaultCategory);
            categories.Insert(0, defaultCategory);
            return categories;
        }

        public IEnumerable<string> ExecuteQuery(DocumentCategoryAndDescendantsIdQuery query)
        {
            var parent = _repository.List.AsQueryable().FirstOrDefault(x => x.Id == query.CategoryId);
            if (parent == null)
                return Enumerable.Empty<string>();
            var categoryIds = new List<string>();
            GetChildCategories(parent, categoryIds);
            return categoryIds;
        }
        public void GetChildCategories(DocumentCategory parent,List<string> categoryIds)
        {
            if (!categoryIds.Contains(parent.Id))
                categoryIds.Add(parent.Id);
            var children = _repository.List.AsQueryable().Where(x => x.ParentId == parent.Id).ToList();
            if (!children.Any())
                return;
            children.ForEach(c =>
            {
                categoryIds.Add(c.Id);
                GetChildCategories(c, categoryIds);
            });
        }
    }
}
namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<DocumentCategory, JSTreeViewModel>> DocumentCategory_JSTreeViewModel =
            x => new JSTreeViewModel
            {
                id = x.Id,
                parent = x.ParentId ?? "#",
                text = x.Title != null ? x.Title.Length > 100 ? x.Title.Substring(0, 100) : x.Title : null
            };
    }
}
