using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.Events.CustomerManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class ChangeCompanyUserExpiryStatusCommandHandler : CommandHandlerBase<ChangeCompanyUserExpiryStatusCommandParameter>
    {
        private readonly IRepository<StandardUser> _userRepository;

        public ChangeCompanyUserExpiryStatusCommandHandler(IRepository<StandardUser> userRepository)
        {
            _userRepository = userRepository;
        }

        public override CommandResponse Execute(ChangeCompanyUserExpiryStatusCommandParameter command)
        {
            var userModel = _userRepository.Find(command.UserId);
            userModel.IsUserExpire = command.IsExpiryStatus;
            if (!command.IsExpiryStatus)
            {
                userModel.ExpireDays = null;
				if (command.IsSelfSignUp) {
					new EventPublisher().Publish(new CustomerSelfProvisionedEvent {
						UserViewModel = Project.UserViewModelFrom(userModel),
						CompanyViewModel = new CompanyViewModel {
							IsSendWelcomeSMS = false
						},
						Subject = CustomerUserSelfSignedUpApprovedEvent.DefaultSubject
					});
				}
				
			}

            _userRepository.SaveChanges();
            return null;
        }
    }
}