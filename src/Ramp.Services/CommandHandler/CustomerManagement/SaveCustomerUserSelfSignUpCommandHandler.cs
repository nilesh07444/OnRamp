using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.Events;
using Ramp.Services.Helpers;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Role = Ramp.Contracts.Security.Role;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class SaveCustomerUserSelfSignUpCommandHandler : CommandHandlerBase<SaveCustomerUserSelfSignUpCommandParameter>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        private readonly IRepository<CustomerRole> _roleRepository;
        private readonly IEventPublisher _eventPublisher;
		private readonly IRepository<Domain.Customer.Models.Groups.StandardUserGroup> _standardGroupRepository;

		public SaveCustomerUserSelfSignUpCommandHandler(IRepository<Company> companyRepository,
            IRepository<StandardUser> userRepository,
            IRepository<CustomerGroup> groupRepository,
            IRepository<CustomerRole> roleRepository,
			IRepository<Domain.Customer.Models.Groups.StandardUserGroup> standardGroupRepository,

			IEventPublisher eventPublisher)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _roleRepository = roleRepository;
            _eventPublisher = eventPublisher;
			_standardGroupRepository = standardGroupRepository;
		}

        public override CommandResponse Execute(SaveCustomerUserSelfSignUpCommandParameter command)
        {
            var company = _companyRepository.Find(command.CompanyViewModel.Id);

            var group = _groupRepository.List.FirstOrDefault(g => g.IsforSelfSignUpGroup);

            if (company != null && company.DefaultUserExpireDays != 0)
            {
                if(!_roleRepository.List.Any(r => r.RoleName == Role.StandardUser))
                {
                    _roleRepository.Add(new CustomerRole
                    {
                        Description = Role.StandardUserDesc,
                        RoleName = Role.StandardUser,
                        Id = Guid.NewGuid()
                    });
                    _roleRepository.SaveChanges();
                }

                var userRole =
                    _roleRepository.List.FirstOrDefault(
                        r => r.RoleName == Contracts.Security.Role.StandardUser);
                if (userRole != null)
                {
                    command.CustomerSelfSignUpViewModel.FullName = command.CustomerSelfSignUpViewModel.FullName.Replace("\"", string.Empty);
                    var customer = new StandardUser
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = company.Id,
                        EmailAddress =
                            command.CustomerSelfSignUpViewModel.EmailAddress.TrimAllCastToLowerInvariant(),
                        FirstName = command.CustomerSelfSignUpViewModel.FullName.GetFirstName(),
                        LastName = command.CustomerSelfSignUpViewModel.FullName.GetLastName(),
                        EmployeeNo = command.CustomerSelfSignUpViewModel.EmployeeNo,
                        IsActive = false,
                        MobileNumber = command.CustomerSelfSignUpViewModel.MobileNumber,
                        Password =
                            new EncryptionHelper().Encrypt(command.CustomerSelfSignUpViewModel.Password),
                        IsConfirmEmail = true,
                        CreatedOn = DateTime.Now,
                        IsUserExpire = false,
                        IsFromSelfSignUp = true,
                        //Group = group,
                        IDNumber = command.CustomerSelfSignUpViewModel.IDNumber,
                        Gender = Domain.Enums.GenderEnum.GetGenderFromString(command.CustomerSelfSignUpViewModel.Gender)
                    };
                    customer.Roles.Add(userRole);

                    if (company.IsSelfSignUpApprove)
                    {
                        customer.IsActive = true;
                    }

					saveUserGroups(customer.Id, group.Title, true);
					_userRepository.Add(customer);
                    _userRepository.SaveChanges();
                    var customerAdmins = _userRepository.List.Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.CustomerAdmin) || r.RoleName.Equals(Role.UserAdmin))).ToList();
                    if (customerAdmins.Count > 0)
                    {
                        var @event = new CustomerSelfSignedUpEvent
                        {
                            UserViewModel = Project.UserViewModelFrom(customer),
                            CompanyViewModel = command.CompanyViewModel,
                            Subject = CustomerSelfSignedUpEvent.DefaultSubject
                        };
                        customerAdmins.ForEach(c => @event.CustomerAdminViewModelsList.Add(Project.UserViewModelFrom(c)));

                        _eventPublisher.Publish(@event);
                    }
                }
            }
            return null;
        }

		private void saveUserGroups(Guid userId, string groupName, bool add)
		{
			if (!add)
			{
				var groupList = _standardGroupRepository.List.Where(cc => cc.UserId == userId).AsQueryable().ToList();

				groupList.ForEach(x => {
					_standardGroupRepository.Delete(x);
				});
				_standardGroupRepository.SaveChanges();
			}

			var groupId = _groupRepository.List.Where(i => i.Title.ToLower() == groupName.ToLower()).Select(id => id.Id).FirstOrDefault();

			var data = new Domain.Customer.Models.Groups.StandardUserGroup
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				GroupId = groupId,
				DateCreated = DateTime.Now

			};

			_standardGroupRepository.Add(data);
			_standardGroupRepository.SaveChanges();

		}
	}
}