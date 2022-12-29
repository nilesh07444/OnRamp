using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.Settings;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler.Settings
{
    public class SettingQueryHandler : QueryHandlerBase<SettingQueryParameter, SettingViewModel>
    {
        private readonly IRepository<Setting> _settingRepository;

        public SettingQueryHandler(IRepository<Setting> settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public override SettingViewModel ExecuteQuery(SettingQueryParameter queryParameters)
        {
            var settingViewModel = new SettingViewModel();

            //if (queryParameters.Id != Guid.Empty)
            //if (queryParameters.Id != null && queryParameters.Id != Guid.Empty)
            if (queryParameters.Id == null)
            {
                //Setting settingModel = _settingRepository.Find(queryParameters.id);                
                Setting settingModel = _settingRepository.List.FirstOrDefault();
                if (settingModel != null)
                {
                    settingViewModel.SettingViewModelShort = new SettingViewModelShort
                    {
                        Id = settingModel.Id,
                        PasswordPolicy = settingModel.PasswordPolicy
                    };
                }
            }
            //setting result = _settingRepository.GetAll().FirstOrDefault();
            var settingList = _settingRepository.List;
            if (settingList != null)
            {
                foreach (var setting in settingList)
                {
                    var settingViewModelShort = new SettingViewModelShort
                    {
                        Id = setting.Id,
                        PasswordPolicy = setting.PasswordPolicy,
                    };
                    settingViewModel.SettingViewModelList.Add(settingViewModelShort);
                }
            }
            return settingViewModel;
        }
    }
}
