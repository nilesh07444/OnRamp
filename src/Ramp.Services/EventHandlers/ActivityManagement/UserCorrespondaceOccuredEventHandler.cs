using Common.Data;
using Common.Events;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.Events.ActivityManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.EventHandlers.ActivityManagement
{
    public class UserCorrespondaceOccuredEventHandler : IEventHandler<UserCorrespondaceOccuredEvent>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<StandardUserCorrespondanceLog> _standardUserCorrespondaceLogRepository;
        private readonly IRepository<UserCorrespondenceLog> _userCorrespondaceLogRepository;

        public UserCorrespondaceOccuredEventHandler(IRepository<StandardUser> standardUserRepository, IRepository<User> userRepository,
            IRepository<StandardUserCorrespondanceLog> standardUserCorrespondaceLogRepository, IRepository<UserCorrespondenceLog> userCorrespondaceLogRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
            _standardUserCorrespondaceLogRepository = standardUserCorrespondaceLogRepository;
            _userCorrespondaceLogRepository = userCorrespondaceLogRepository;
        }

        public void Handle(UserCorrespondaceOccuredEvent @event)
        {
            var user = _userRepository.Find(@event.UserCorrespondenceLogViewModel.UserViewModel.Id);
            var standardUser = _standardUserRepository.Find(@event.UserCorrespondenceLogViewModel.UserViewModel.Id);

            if (user != null)
            {
                var entry = new UserCorrespondenceLog
                {
                    CorrespondenceType = @event.UserCorrespondenceLogViewModel.CorrespondenceType,
                    User = user,
                    Description = @event.UserCorrespondenceLogViewModel.Description,
                    CorrespondenceDate = @event.UserCorrespondenceLogViewModel.CorrespondenceDate,
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Content = @event.UserCorrespondenceLogViewModel.MessageContent
                };
                _userCorrespondaceLogRepository.Add(entry);
                _userCorrespondaceLogRepository.SaveChanges();
            }
            else if (standardUser != null)
            {
                var entry = new StandardUserCorrespondanceLog
                {
                    Id = Guid.NewGuid(),
                    User = standardUser,
                    Description = @event.UserCorrespondenceLogViewModel.Description,
                    CorrespondenceDate = @event.UserCorrespondenceLogViewModel.CorrespondenceDate,
                    Content = @event.UserCorrespondenceLogViewModel.MessageContent,
                    UserId = standardUser.Id
                };
                if (@event.UserCorrespondenceLogViewModel.CorrespondenceType == UserCorrespondenceEnum.Email)
                    entry.CorrespondenceType = StandardUserCorrespondenceEnum.Email;
                if (@event.UserCorrespondenceLogViewModel.CorrespondenceType == UserCorrespondenceEnum.Sms)
                    entry.CorrespondenceType = StandardUserCorrespondenceEnum.Sms;
                _standardUserCorrespondaceLogRepository.Add(entry);
                _standardUserCorrespondaceLogRepository.SaveChanges();
            }
        }
    }
}