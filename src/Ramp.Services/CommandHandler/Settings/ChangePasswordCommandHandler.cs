using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Settings;
using Ramp.Services.Helpers;

namespace Ramp.Services.CommandHandler.Settings
{
    public class ChangePasswordCommandHandler : CommandHandlerBase<ChangePasswordCommandParameter>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;

        public ChangePasswordCommandHandler(IRepository<User> userRepository, IRepository<StandardUser> standardUserRepository)
        {
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
        }

        public override CommandResponse Execute(ChangePasswordCommandParameter command)
        {
            var encryptionHelper = new EncryptionHelper();
            User user = _userRepository.Find(command.Id);
            StandardUser standardUser = _standardUserRepository.Find(command.Id);
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