using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter.IconSet;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Web.Mvc;
using Ramp.Services.Projection;

namespace Ramp.Services.QueryHandler
{
    public class IconQueryHandler
        : IQueryHandler<IconSetListQuery, IEnumerable<IconSetModel>>,
            IQueryHandler<AvaiableIconTypesQuery, IEnumerable<SelectListItem>>,
            IQueryHandler<FindIconSetQuery, IconSetModel>
    {
        private readonly IRepository<IconSet> _setRepository;
        public IconQueryHandler(IRepository<IconSet> setRepository)
        {
            _setRepository = setRepository;
        }

        public IconSetModel ExecuteQuery(FindIconSetQuery query)
        {
            Guid id;
            if (!Guid.TryParse(query.Id, out id))
                return null;
            var entity = _setRepository.Find(id);
            var model = Project.ToIconSetModel.Compile().Invoke(entity);
            model.AvailableIcons = getavaiableIconTypes();
            return model;
        }

        public IEnumerable<SelectListItem> ExecuteQuery(AvaiableIconTypesQuery query)
        {

            return getavaiableIconTypes();
        }
        private IEnumerable<SelectListItem> getavaiableIconTypes()
        {
            var result = new List<SelectListItem>();
            Enum.GetNames(typeof(IconType)).ToList().ForEach(e => result.Add(new SelectListItem
            {
                Value = ((int)Enum.Parse(typeof(IconType), e)).ToString(),
                Text = ((IconType)Enum.Parse(typeof(IconType), e)).GetFriendlyName()
            }));
            return result.OrderBy(x => x.Text).ToList();
        }

        public IEnumerable<IconSetModel> ExecuteQuery(IconSetListQuery query)
        {
            var enties = _setRepository.List.AsQueryable();
            Guid id;
            if (Guid.TryParse(query.Id, out id))
                enties = enties.Where(x => x.Id == id);
            if (!string.IsNullOrWhiteSpace(query.Name))
                enties = enties.Where(x => x.Name == query.Name);
            var result = enties.AsQueryable().Where(x => !x.Deleted.HasValue || (x.Deleted.HasValue && !x.Deleted.Value)).Select(Project.ToIconSetListModel).ToList();
            result.ForEach(delegate (IconSetModel x)
            {
                x.Icons.ToList().ForEach(q => q.Description = q.IconType.GetFriendlyName());

            });

            return result;
        }
    }
}
namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<Icon, IconModel>> ToIconModel
            = x => new IconModel
            {
                IconType = x.IconType,
                Id = x.Id.ToString(),
                UploadModel = x.Upload != null ? ToUploadModel_Domain.Compile().Invoke(x.Upload) : null
            };
        public static readonly Expression<Func<IconSet, IconSetModel>> ToIconSetModel
        = x => new IconSetModel
        {
            Id = x.Id.ToString(),
            Name = x.Name,
            Icons = x.Icons.AsQueryable().Select(ToIconModel).ToList(),
            Master = x.Master
        };
        public static readonly Expression<Func<IconSet, IconSetModel>> ToIconSetListModel
      = x => new IconSetModel
      {
          Id = x.Id.ToString(),
          Name = x.Name,
          Master = x.Master
      };
    }
}
namespace Ramp.Services
{
    public static partial class Extensions
    {
        public static string GetFriendlyName(this IconType type)
        {
            return VirtuaCon.EnumUtility.GetFriendlyName<IconType>(type);
        }
    }

}
