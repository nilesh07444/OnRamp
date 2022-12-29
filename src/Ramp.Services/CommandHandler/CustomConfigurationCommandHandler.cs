using Common.Command;
using Common.Data;
using Common.Events;
using Common.Query;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Settings;
using Ramp.Contracts.CommandParameter.Upload;
using Ramp.Contracts.QueryParameter.Upload;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Common;

namespace Ramp.Services.CommandHandler
{
    public class CustomConfigurationCommandHandler : ICommandHandlerBase<SaveCustomConfigurationCommand>
    {
        private readonly IRepository<Company> _repository;
        private readonly IQueryExecutor _executor;
        private readonly ICommandDispatcher _dispatcher;
        public CustomConfigurationCommandHandler(
            IRepository<Company> repository,
            IQueryExecutor executor,
            ICommandDispatcher dispatcher)
        {
            _repository = repository;
            _executor = executor;
            _dispatcher = dispatcher;
        }

        public CommandResponse Execute(SaveCustomConfigurationCommand command)
        {
            Guid id;
            if (!Guid.TryParse(command.CompanyId, out id))
                throw new ArgumentException();
            var company = _repository.Find(id);
            if (command.Certificate != null)
                company.CustomerConfigurations.Add(Convert(command.Certificate, company, CustomerConfigurationType.Certificate));
            if (command.CSS != null)
                company.CustomerConfigurations.Add(Convert(command.CSS, company, CustomerConfigurationType.CSS));
            if (command.DeleteDashboardLogo.HasValue && command.DeleteDashboardLogo.Value)
                company.CustomerConfigurations.Where(u => u.Type == CustomerConfigurationType.DashboardLogo).ToList().ForEach(u => u.Deleted = true);
            if (command.DashboardLogo != null)
                company.CustomerConfigurations.Add(Convert(command.DashboardLogo, company, CustomerConfigurationType.DashboardLogo));
            if (command.DeleteLoginLogo.HasValue && command.DeleteLoginLogo.Value)
                company.CustomerConfigurations.Where(u => u.Type == CustomerConfigurationType.LoginLogo).ToList().ForEach(u => u.Deleted = true);
            if (command.LoginLogo != null)
                company.CustomerConfigurations.Add(Convert(command.LoginLogo, company, CustomerConfigurationType.LoginLogo));

			if (command.NotificationHeaderLogo != null)
                company.CustomerConfigurations.Add(Convert(command.NotificationHeaderLogo, company, CustomerConfigurationType.NotificationHeaderLogo));

			if (command.DeleteNotificationHeaderLogo.HasValue && command.DeleteNotificationHeaderLogo.Value)
				company.CustomerConfigurations.Where(u => u.Type == CustomerConfigurationType.NotificationHeaderLogo).ToList().ForEach(u => u.Deleted = true);

			if (command.NotificationFooterLogo != null)
                company.CustomerConfigurations.Add(Convert(command.NotificationFooterLogo, company, CustomerConfigurationType.NotificationFooterLogo));

			if (command.DeleteNotificationFooterLogo.HasValue && command.DeleteNotificationFooterLogo.Value)
				company.CustomerConfigurations.Where(u => u.Type == CustomerConfigurationType.NotificationFooterLogo).ToList().ForEach(u => u.Deleted = true);

			if (command.DeleteFooterLogo.HasValue && command.DeleteFooterLogo.Value)
                company.CustomerConfigurations.Where(u => u.Type == CustomerConfigurationType.FooterLogo).ToList().ForEach(u => u.Deleted = true);
            if (command.FooterLogo != null)
                company.CustomerConfigurations.Add(Convert(command.FooterLogo, company, CustomerConfigurationType.FooterLogo));
            if (command.Tropy != null)
                company.CustomerConfigurations.Add(Convert(command.Tropy, company, CustomerConfigurationType.Trophy));
            _repository.SaveChanges();
            return null;
        }
        private CustomerConfiguration Convert(HttpPostedFileBase u,Company company,CustomerConfigurationType type)
        {
            return new CustomerConfiguration
            {
                Id = Guid.NewGuid(),
                Type = type,
                Upload = Convert(u),
                Version = company.CustomerConfigurations.Where(x => x.Type == type).Count() + 1
            };
        }
        private Domain.Models.FileUpload Convert(HttpPostedFileBase u)
        {
            var model = _executor.Execute<GetFileUploadFromPostedFileQuery, FileUploadViewModel>(new GetFileUploadFromPostedFileQuery { File = u, Id = Guid.NewGuid() });
            return new Domain.Models.FileUpload
            {
                MIMEType = model.ContentType,
                Data = model.Data,
                Id = model.Id,
                Name = model.Name
            };
        }
    }
}
