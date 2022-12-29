using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models.Custom_Fields;
using Ramp.Contracts.Query.CustomFields;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.CustomFields
{
    public class CustomFieldsQueryHandler : IQueryHandler<CustomFiledsQuery, IEnumerable<JSTreeViewModel>>
    {
        private readonly ITransientReadRepository<Fields> _repository;

        public CustomFieldsQueryHandler(ITransientReadRepository<Fields> repository)
        {
            _repository = repository;

        }
        public IEnumerable<JSTreeViewModel> ExecuteQuery(CustomFiledsQuery query)
        {
            var categories = _repository.List.AsQueryable().OrderBy(x => x.FieldType).Select(Project.Fields_JSTreeViewModel).ToList();
            categories.ForEach(x => x.text = WebUtility.HtmlDecode(x.text));
            var defaultCategory = categories.First(x => x.text == "Default");
            categories.Remove(defaultCategory);
            categories.Insert(0, defaultCategory);
            return categories;
        }
    }
}
namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<Fields, JSTreeViewModel>> Fields_JSTreeViewModel =
            x => new JSTreeViewModel
            {
                id = x.Id.ToString(),
                //parent = x.ParentId ?? "#",
                text = x.FieldType != null ? x.FieldType.Length > 100 ? x.FieldType.Substring(0, 100) : x.FieldType : null
            };
    }
}
