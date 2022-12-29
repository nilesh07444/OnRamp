using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class UpdateCustomerUserExpiryStatusCommandHandler : CommandHandlerBase<UpdateCustomerUserExpiryStatusCommandParameter>
    {

         private readonly IRepository<User> _userRepository;

         public UpdateCustomerUserExpiryStatusCommandHandler(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

         public override CommandResponse Execute(UpdateCustomerUserExpiryStatusCommandParameter command)
        {
            User userModel = _userRepository.Find(command.UserId);
            userModel.IsUserExpire = true;
            _userRepository.SaveChanges();
            return null;
        }
    

    }
}
