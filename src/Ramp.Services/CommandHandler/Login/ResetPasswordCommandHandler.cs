using Common.Command;
using Common.Data;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.Login;
using Ramp.Services.Helpers;
using System;
using System.Linq;
using CompanyType = Domain.Enums.CompanyType;

namespace Ramp.Services.CommandHandler.Login
{
    public class ResetPasswordCommandHandler : ICommandHandlerBase<ResetPasswordCommand>
    {
        private readonly IRepository<User> _userRepository;
        private readonly ITransientRepository<StandardUser> _standardUserRepository;

        public ResetPasswordCommandHandler(IRepository<User> userRepository, ITransientRepository<StandardUser> standardUserRepository)
        {
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
        }

        public CommandResponse Execute(ResetPasswordCommand command)
        {
            var encryptionHelper = new EncryptionHelper();
            if (!string.IsNullOrWhiteSpace(command.Token))
            {
                var companyUser = encryptionHelper.Decrypt(command.Token).Split(':');
                if (companyUser.Length == 2)
                {
                    if (Guid.TryParse(companyUser[0], out var companyId))
                        _standardUserRepository.SetCustomerCompany(companyId.ToString());
                    if (Guid.TryParse(companyUser[1], out var userId))
                        command.Id = userId;
                }
            }
            User user = _userRepository.List.AsQueryable().FirstOrDefault(u => u.Id == command.Id);
            StandardUser standardUser = _standardUserRepository.List.AsQueryable().FirstOrDefault(u => u.Id == command.Id);
            if (user != null)
            {
                user.Password = encryptionHelper.Encrypt(command.Password);
                _userRepository.SaveChanges();
            }
            else if (standardUser != null)
            {
                standardUser.Password = encryptionHelper.Encrypt(command.Password);
                _standardUserRepository.SaveChanges();
            }

            return null;
        }
    }
}