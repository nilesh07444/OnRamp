using Common.Command;
using Common.Data;
using Common.Events;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.ProvisionalManagement;
using Ramp.Contracts.Events.Account;
using Ramp.Services.Helpers;
using System.Linq;

namespace Ramp.Services.CommandHandler.ProvisionalManagement
{
    public class DeleteProvisionalCompanyUserCommandHandler :
        CommandHandlerBase<DeleteProvisionalCompanyUserCommandParameter>
    {
        private readonly ITransientRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<StandardUserActivityLog> _standradUserActivityLogRepository;
        private readonly IEventPublisher _eventPublisher;

        public DeleteProvisionalCompanyUserCommandHandler(ITransientRepository<StandardUser> standardUserRepository, IRepository<User> userRepository, IRepository<StandardUserActivityLog> standradUserActivityLogRepository,IEventPublisher eventPublisher)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
            _standradUserActivityLogRepository = standradUserActivityLogRepository;
            _eventPublisher = eventPublisher;
        }

        public override CommandResponse Execute(DeleteProvisionalCompanyUserCommandParameter command)
        {
            var user = _userRepository.Find(command.ProvisionalComapanyUserId);
            var standardUser = _standardUserRepository.Find(command.ProvisionalComapanyUserId);

            if (user != null)
            {
                _userRepository.Delete(user);
                _userRepository.SaveChanges();
            }
            else if (standardUser != null)
            {
                _standardUserRepository.Delete(standardUser);
                _standardUserRepository.SaveChanges();
                _eventPublisher.Publish(new StandardUserDeletedEvent { Id = standardUser.Id.ToString() });
            }
            return null;
        }
    }
}