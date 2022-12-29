using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Categories;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.CommandParameter.Portal;
using Ramp.Contracts.Events.CustomerManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler;
using System;
using System.Linq;
using System.Text;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class SaveCustomerCompanySelfProvisionCommandHandler :
        CommandHandlerBase<SaveSelfProvisionalCustomerCompanyCommand>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Bundle> _bundleRepository;

        public SaveCustomerCompanySelfProvisionCommandHandler(IRepository<Company> companyRepository,
            IRepository<Bundle> bundleRepository)
        {
            _companyRepository = companyRepository;
            _bundleRepository = bundleRepository;
        }

        public override CommandResponse Execute(SaveSelfProvisionalCustomerCompanyCommand command)
        {
            var selectedBundle = _bundleRepository.List.FirstOrDefault(p => p.IsForSelfProvision);
            var selectedCompany = _companyRepository.List.FirstOrDefault(c => c.IsForSelfProvision);

            var companyId = Guid.NewGuid();

            var company = new Company
            {
                Id = companyId,
                CompanyName = command.CompanyName,
                LayerSubDomain = command.LayerSubDomain,
                PhysicalAddress = command.PhysicalAddress,
                PostalAddress = command.PostalAddress,
                ClientSystemName = command.ClientSystemName,
                TelephoneNumber = command.TelephoneNumber,
                WebsiteAddress = command.WebsiteAddress,
                CompanyType = CompanyType.CustomerCompany,
                LogoImageUrl = command.LogoImageUrl,
                UserList = null,
                BundleId = selectedBundle.Id,
                CreatedOn = DateTime.Now,
                IsChangePasswordFirstLogin = false,
                IsSendWelcomeSMS = false,
                IsActive = true,
                IsLock = false,
                IsSelfCustomer = true,
                ApplyCustomCss = false,
                customCssFile = null,
                IsForSelfSignUp = true,
                IsSelfSignUpApprove = true,
                ExpiryDate = DateTime.Now.AddMonths(1),
                AutoExpire = true,
                YearlySubscription = false,
                ProvisionalAccountLink = selectedCompany.Id,
                TestExpiryNotificationInterval = NotificationInterval.OneDayBefore
            };

            _companyRepository.Add(company);
            _companyRepository.SaveChanges();

            var names = command.FullName.Split(' ');
            command.FirstName = names[0];
            var lastNameBuilder = new StringBuilder();
            for (var i = 1; i < names.Length; i++)
            {
                if (i == 1)
                    lastNameBuilder.Append(names[i]);
                else
                    lastNameBuilder.AppendFormat(" {0}", names[i]);
            }
            command.LastName = lastNameBuilder.ToString();

            new CommandDispatcher().Dispatch(new OverridePortalContextCommand { CompanyId = companyId });
            var addUserCommand = new AddOrUpdateCustomerCompanyUserByCustomerAdminCommand
            {
                CompanyId = companyId,
                EmailAddress = command.EmailAddress,
                FirstName = command.FirstName,
                LastName = command.LastName,
                MobileNumber = command.MobileNumber,
                ParentUserId = command.ParentUserId,
                Password = command.Password,
                ClientSystemName = command.ClientSystemName,
                IsFromSelfSignUp = true,
                IDNumber = command.IDNumber,
                Gender = command.Gender
            };
            addUserCommand.Roles.Add(Contracts.Security.Role.CustomerAdmin);
            new CommandDispatcher().Dispatch(addUserCommand);
            new EventPublisher().Publish(new CustomerSelfProvisionedEvent
            {
                CompanyViewModel = Project.CompanyViewModelFrom(company),
                Subject = "Email confirmation",
                UserViewModel = new UserViewModel
                {
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    EmailAddress = command.EmailAddress,
                    MobileNumber = command.MobileNumber
                }
            });
            return null;
        }
    }
}