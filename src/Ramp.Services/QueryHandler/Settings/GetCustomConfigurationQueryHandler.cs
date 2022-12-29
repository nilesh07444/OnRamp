using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.Settings;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.Settings
{
    public class GetCustomConfigurationQueryHandler : IQueryHandler<GetCustomConfigurationQueryParameter, CustomConfigurationViewModel>
    {
        private readonly IRepository<CustomConfiguration> _customConfigurationRepository;

        public GetCustomConfigurationQueryHandler(IRepository<CustomConfiguration> customConfigurationRepository)
        {
            _customConfigurationRepository = customConfigurationRepository;
        }

        public CustomConfigurationViewModel ExecuteQuery(GetCustomConfigurationQueryParameter query)
        {
            var active = _customConfigurationRepository.List.Where(x => !x.Deleted.HasValue || (x.Deleted.HasValue && !x.Deleted.Value)).ToList();
            var model = new CustomConfigurationViewModel();
            if(active.Any(x => x.Type == CustomType.Certificate))
                model.Certificate = Project.ToUploadModel.Compile().Invoke(active.OrderBy(x => x.Version).Last(x => x.Type == CustomType.Certificate).Upload);
            if(active.Any(x => x.Type == CustomType.CSS))
                model.CSS = Project.ToUploadModelWithoutData.Compile().Invoke(active.First(x => x.Type == CustomType.CSS).Upload);
            if (active.Any(x => x.Type == CustomType.LoginLogo))
                model.LoginLogo = Project.ToUploadModelWithoutData.Compile().Invoke(active.First(x => x.Type == CustomType.LoginLogo).Upload);

			if (active.Any(x => x.Type == CustomType.NotificationHeaderLogo))
				model.NotificationHeaderLogo = Project.ToUploadModelWithoutData.Compile().Invoke(active.First(x => x.Type == CustomType.NotificationHeaderLogo).Upload);

			if (active.Any(x => x.Type == CustomType.NotificationFooterLogo))
				model.NotificationFooterLogo = Project.ToUploadModelWithoutData.Compile().Invoke(active.First(x => x.Type == CustomType.NotificationFooterLogo).Upload);

			if (active.Any(x => x.Type == CustomType.DashboardLogo))
                model.DashboardLogo = Project.ToUploadModelWithoutData.Compile().Invoke(active.First(x => x.Type == CustomType.DashboardLogo).Upload);
            if (active.Any(x => x.Type == CustomType.Trophy))
                model.Trophies = active.Where(x => x.Type == CustomType.Trophy).AsQueryable().Select(x => x.Upload).Select(Project.ToUploadModelWithoutData).ToList();

            return model;
        }
    }
}