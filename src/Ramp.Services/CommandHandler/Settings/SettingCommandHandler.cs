using System;
using System.Linq;
using Common.Data;
using Common.Command;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Settings;

namespace Ramp.Services.CommandHandler.Settings
{
    public class SettingCommandHandler : CommandHandlerBase<SettingCommandParameter>
    {
        private readonly IRepository<Setting> _settingRepository;

        public SettingCommandHandler(IRepository<Setting> settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public override CommandResponse Execute(SettingCommandParameter command)
        {
            var settingModel = _settingRepository.List.FirstOrDefault();
            if (settingModel == null)
            {
                var setting = new Setting
                {
                    Id = Guid.NewGuid(),
                    PasswordPolicy = command.PasswordPolicy,
                };
                _settingRepository.Add(setting);
                _settingRepository.SaveChanges();
            }
            else
            {
                settingModel.PasswordPolicy = command.PasswordPolicy;
                _settingRepository.SaveChanges();
            }
            return null;
        }
    }
}
