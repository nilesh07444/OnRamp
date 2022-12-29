using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.CommandHandler.Settings
{
    public class ClearCustomConfigurationCommadHandler : ICommandHandlerBase<ClearCustomConfigurationCommadParameter>
    {
        private readonly IRepository<CustomConfiguration> _customConfigurationRepository;
        private readonly IRepository<FileUploads> _uploadRepository;

        public ClearCustomConfigurationCommadHandler(
            IRepository<CustomConfiguration> customConfigurationRepository,
            IRepository<FileUploads> uploadRepository)
        {
            _customConfigurationRepository = customConfigurationRepository;
            _uploadRepository = uploadRepository;
        }

        public CommandResponse Execute(ClearCustomConfigurationCommadParameter command)
        {
            var active = _customConfigurationRepository.List.Where(x => !x.Deleted.HasValue || (x.Deleted.HasValue && !x.Deleted.Value));
            var itemsToMarkAsDeleted = new List<CustomConfiguration>();
            if (command.Certificate)
                itemsToMarkAsDeleted.AddRange(active.Where(x => x.Type == CustomType.Certificate));
            if (command.CSS)
                itemsToMarkAsDeleted.AddRange(active.Where(x => x.Type == CustomType.CSS));
            if (command.DashboardLogo)
                itemsToMarkAsDeleted.AddRange(active.Where(x => x.Type == CustomType.DashboardLogo));
            if (command.LoginLogo)
                itemsToMarkAsDeleted.AddRange(active.Where(x => x.Type == CustomType.LoginLogo));

            itemsToMarkAsDeleted.ForEach(x => x.Deleted = true);
            _customConfigurationRepository.SaveChanges();
            return null;
        }
    }
}