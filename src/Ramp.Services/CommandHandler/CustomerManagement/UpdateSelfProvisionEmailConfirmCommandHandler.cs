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
   public class UpdateSelfProvisionEmailConfirmCommandHandler :
        CommandHandlerBase<UpdateSelfProvisionEmailConfirm>
    {
       private readonly IRepository<User> _userRepository;

       public UpdateSelfProvisionEmailConfirmCommandHandler(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

       public override CommandResponse Execute(UpdateSelfProvisionEmailConfirm command)
        {

            var user = _userRepository.GetAll();

            User userModel = user.Where(u => u.EmailAddress == command.EmailId).FirstOrDefault();
           if(userModel != null)
           {
               userModel.IsConfirmEmail = true;
               _userRepository.SaveChanges();
           
           }
          

           // userModel.IsActive = command.ProvisionalCompanyUserStatus;
            //_userRepository.SaveChanges();
            return null;
        }
    }
}
