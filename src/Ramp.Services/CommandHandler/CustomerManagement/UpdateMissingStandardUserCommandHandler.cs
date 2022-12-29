using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models.Groups;
using Domain.Customer.Models;
using Domain.Enums;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.CommandParameter.Group;
using Ramp.Contracts.Events.CustomerManagement;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Customer.Models.Document;
using VirtuaCon;
using Role = Ramp.Contracts.Security.Role;
using Domain.Models;
using Domain.Customer.Models.CustomRole;
using Ramp.Contracts.ViewModel;
using Ramp.Contracts.Command.Document;
using Domain.Customer;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class UpdateMissingStandardUserCommandHandler :
        CommandHandlerBase<UpdateStandardUserByAdUser>

    {
        private readonly IRepository<StandardUser> _userRepository;


        public UpdateMissingStandardUserCommandHandler(IRepository<StandardUser> userRepository)
        {
            _userRepository = userRepository;
        }




        public override CommandResponse Execute(UpdateStandardUserByAdUser command)
        {
            var adminUser = command.GroupName == null && Role.IsInAdminRole(command.Roles);
            var user = _userRepository.List.SingleOrDefault(u => u.Id.Equals(command.UserId) || u.EmailAddress.Equals(command.EmailAddress));

            var checkuser = new StandardUser();
            //if (adminUser)
            //{
                if (user != null)
                {
                user.IsActive = command.IsActive;
                    
                }

                _userRepository.SaveChanges();
           // }   
            return null;
        }


    }
}