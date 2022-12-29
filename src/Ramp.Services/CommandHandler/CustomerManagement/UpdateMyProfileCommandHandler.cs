using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class UpdateMyProfileCommandHandler : ICommandHandlerBase<UpdateMyProfileCommandParameter>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;

        public UpdateMyProfileCommandHandler(IRepository<StandardUser> standardUserRepository, IRepository<User> userRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
        }

        public CommandResponse Execute(UpdateMyProfileCommandParameter command)
        {
            if (_standardUserRepository.Find(command.EditUserProfileViewModel.Id) != null)
            {
                var standardUser = _standardUserRepository.Find(command.EditUserProfileViewModel.Id);
                standardUser.FirstName = command.EditUserProfileViewModel.FullName.GetFirstName();
                standardUser.LastName = command.EditUserProfileViewModel.FullName.GetLastName();
                if (command.EditUserProfileViewModel.Password != null)
                {
                    standardUser.Password = new EncryptionHelper().Encrypt(command.EditUserProfileViewModel.Password);
                }
                standardUser.MobileNumber = command.EditUserProfileViewModel.MobileNumber;
                _standardUserRepository.SaveChanges();
            }
            else if (_userRepository.Find(command.EditUserProfileViewModel.Id) != null)
            {
                var user = _userRepository.Find(command.EditUserProfileViewModel.Id);
                user.FirstName = command.EditUserProfileViewModel.FullName.GetFirstName();
                user.LastName = command.EditUserProfileViewModel.FullName.GetLastName();
                if (command.EditUserProfileViewModel.Password != null)
                {
                    user.Password = new EncryptionHelper().Encrypt(command.EditUserProfileViewModel.Password);
                }
                user.MobileNumber = command.EditUserProfileViewModel.MobileNumber;
                _userRepository.SaveChanges();
            }

            return null;
        }
    }
}