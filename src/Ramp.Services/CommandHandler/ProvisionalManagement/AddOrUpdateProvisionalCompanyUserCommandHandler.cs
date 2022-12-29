using Common.Command;
using Common.Data;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts.CommandParameter.ProvisionalManagement;
using Ramp.Services.Helpers;
using System;
using System.Linq;

namespace Ramp.Services.CommandHandler.ProvisionalManagement
{
    public class AddOrUpdateProvisionalCompanyUserCommandHandler :
        CommandHandlerBase<AddOrUpdateProvisionalCompanyUserCommand>
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<User> _userRepository;

        public AddOrUpdateProvisionalCompanyUserCommandHandler(IRepository<Role> roleRepository, IRepository<User> userRepository)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        public override CommandResponse Execute(AddOrUpdateProvisionalCompanyUserCommand command)
        {
            User userModel = _userRepository.Find(command.UserId);
            if (userModel == null)
            {
                var resellerRole = _roleRepository.List.First(r => r.RoleName == EnumHelper.GetDescription(UserRole.Reseller));

                var reseller = new User
                {
                    Id = Guid.NewGuid(),
                    CompanyId = command.CompanyId,
                    EmailAddress = command.EmailAddress,
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    IsActive = true,
                    MobileNumber = command.MobileNumber,
                    ParentUserId = command.ParentUserId,
                    ContactNumber = command.ContactNumber,
                    Password = new EncryptionHelper().Encrypt(command.Password),
                    IsConfirmEmail = true,
                    IsUserExpire = false,
                    IsFromSelfSignUp = false,
                    CreatedOn = DateTime.UtcNow,
                    IDNumber = command.IDNumber
                };
                resellerRole.Users.Add(reseller);
                _roleRepository.SaveChanges();
                _userRepository.SaveChanges();
            }
            else
            {
                userModel.CompanyId = command.CompanyId;
                userModel.EmailAddress = command.EmailAddress;
                userModel.FirstName = command.FirstName;
                userModel.LastName = command.LastName;
                userModel.ContactNumber = command.ContactNumber;
                userModel.MobileNumber = command.MobileNumber;
                userModel.IsConfirmEmail = true;
                userModel.IsUserExpire = userModel.IsUserExpire;
                userModel.CreatedOn = userModel.CreatedOn;
                userModel.IsFromSelfSignUp = false;
                userModel.IDNumber = command.IDNumber;
                if (!string.IsNullOrEmpty(command.Password))
                    userModel.Password = new EncryptionHelper().Encrypt(command.Password);
                _userRepository.SaveChanges();
            }
            return null;
        }
    }
}