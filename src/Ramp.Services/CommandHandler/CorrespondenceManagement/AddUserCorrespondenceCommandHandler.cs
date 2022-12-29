using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CorrespondenceManagement;
using System;

namespace Ramp.Services.CommandHandler.CorrespondenceManagement
{
    public class AddUserCorrespondenceCommandHandler : CommandHandlerBase<AddUserCorrespondenceCommand>
    {
        private readonly IRepository<UserCorrespondenceLog> _userCorrespondenceLogRepository;
        private readonly IRepository<StandardUserCorrespondanceLog> _standardUserCorrespondaceLogRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;

        public AddUserCorrespondenceCommandHandler(IRepository<UserCorrespondenceLog> userCorrespondenceLogRepository,
            IRepository<StandardUserCorrespondanceLog> standardUserCorrespondaceLogRepository,
            IRepository<User> userRepository,
            IRepository<StandardUser> standardUserRepository)
        {
            _userCorrespondenceLogRepository = userCorrespondenceLogRepository;
            _standardUserCorrespondaceLogRepository = standardUserCorrespondaceLogRepository;
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
        }

        public override CommandResponse Execute(AddUserCorrespondenceCommand command)
        {
            if (_userRepository.Find(command.CurrentUserId) != null)
            {
                var log = new UserCorrespondenceLog
                {
                    Id = Guid.NewGuid(),
                    UserId = command.CurrentUserId,
                    CorrespondenceDate = DateTime.Now,
                    CorrespondenceType = command.CorrespondenceType,
                    Description = command.CorrespondenceDescription,
                    Content = command.Content
                };
                _userCorrespondenceLogRepository.Add(log);
                _userCorrespondenceLogRepository.SaveChanges();
            }
            if (_standardUserRepository.Find(command.CurrentUserId) != null)
            {
                var log = new StandardUserCorrespondanceLog
                {
                    Id = Guid.NewGuid(),
                    UserId = command.CurrentUserId,
                    CorrespondenceDate = DateTime.Now,
                    Description = command.CorrespondenceDescription,
                    Content = command.Content
                };
                if (command.CorrespondenceType == UserCorrespondenceEnum.Email)
                    log.CorrespondenceType = StandardUserCorrespondenceEnum.Email;
                if (command.CorrespondenceType == UserCorrespondenceEnum.Sms)
                    log.CorrespondenceType = StandardUserCorrespondenceEnum.Sms;

                _standardUserCorrespondaceLogRepository.Add(log);
                _standardUserCorrespondaceLogRepository.SaveChanges();
            }

            return null;
        }
    }
}