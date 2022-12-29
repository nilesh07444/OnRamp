using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter;
using Ramp.Contracts.CommandParameter.ProvisionalManagement;

namespace Ramp.Services.CommandHandler.ProvisionalManagement
{
    public class UpdateProvisionalCompanyUserStatusCommandHandler :
        CommandHandlerBase<UpdateProvisionalCompanyUserStatusCommand>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;

        public UpdateProvisionalCompanyUserStatusCommandHandler(IRepository<User> userRepository,
            IRepository<StandardUser> standardUserRepository)
        {
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
        }

        public override CommandResponse Execute(UpdateProvisionalCompanyUserStatusCommand command)
        {
            if (command.SentFromProvisionalManagement)
            {
                var user = _userRepository.Find(command.ProvisionalCompanyUserId);
                if (user != null)
                {
                    user.IsActive = command.ProvisionalCompanyUserStatus;
                    _userRepository.SaveChanges();
                }
            }
            else
            {
                var standardUser = _standardUserRepository.Find(command.ProvisionalCompanyUserId);
                if (standardUser != null)
                {
                    standardUser.IsActive = command.ProvisionalCompanyUserStatus;
                    _standardUserRepository.SaveChanges();
                }
            }
            return null;
        }
    }
}