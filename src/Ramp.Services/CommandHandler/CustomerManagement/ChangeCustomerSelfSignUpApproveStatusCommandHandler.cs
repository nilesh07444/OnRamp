using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.Events.CustomerManagement;
using Ramp.Services.Helpers;
using Ramp.Services.QueryHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Services.Projection;
namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class ChangeCustomerSelfSignUpApproveStatusCommandHandler : CommandHandlerBase<ChangeCustomerSelfSignUpApproveStatusCommandParameter>
    {
        private readonly IRepository<StandardUser> _userRepository;

        public ChangeCustomerSelfSignUpApproveStatusCommandHandler(IRepository<StandardUser> userRepository)
        {
            _userRepository = userRepository;
        }

        public override CommandResponse Execute(ChangeCustomerSelfSignUpApproveStatusCommandParameter command)
        {
            var userModel = _userRepository.Find(command.UserId);
            if (userModel != null)
            {
                userModel.IsActive = command.ApproveStatus;
                _userRepository.SaveChanges();
                if (command.ApproveStatus)
                {
                    new EventPublisher().Publish(new CustomerUserSelfSignedUpApprovedEvent
                    {
                        UserViewModel = Project.UserViewModelFrom(userModel),
                        CompanyViewModel = command.CompanyViewModel,
                        Subject = CustomerUserSelfSignedUpApprovedEvent.DefaultSubject
                    });
                }
            }
            return null;
        }
    }
}