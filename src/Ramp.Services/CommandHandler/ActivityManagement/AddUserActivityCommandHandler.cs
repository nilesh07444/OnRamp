using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.ActivityManagement;
using System;

namespace Ramp.Services.CommandHandler.ActivityManagement
{
    public class AddUserActivityCommandHandler : CommandHandlerBase<AddUserActivityCommand>
    {
        private readonly IRepository<UserActivityLog> _userActivityLogRepository;
        private readonly IRepository<StandardUserActivityLog> _standardUserActivityLogRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;

        public AddUserActivityCommandHandler(IRepository<UserActivityLog> userActivityLogRepository,
            IRepository<StandardUserActivityLog> standardUserActivityLogRepository,
            IRepository<User> userRepository,
            IRepository<StandardUser> standardUserRepository)
        {
            _userActivityLogRepository = userActivityLogRepository;
            _standardUserActivityLogRepository = standardUserActivityLogRepository;
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
        }

        public override CommandResponse Execute(AddUserActivityCommand command)
        {
            var user = _userRepository.Find(command.CurrentUserId);
            var standardUser = _standardUserRepository.Find(command.CurrentUserId);

            if (user != null)
            {
                var model = new UserActivityLog
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ActivityDate = DateTime.Now,
                    ActivityType = command.ActivityType,
                    Description = command.ActivityDescription,
                };
                _userActivityLogRepository.Add(model);
                _userActivityLogRepository.SaveChanges();
            }
            if (standardUser != null)
            {
                var model = new StandardUserActivityLog()
                {
                    Id = Guid.NewGuid(),
                    User = standardUser,
                    Description = command.ActivityDescription,
                    ActivityDate = DateTime.Now
                };
                model.ActivityType = Helpers.EnumHelper.GetDescription(command.ActivityType);
                _standardUserActivityLogRepository.Add(model);
                _standardUserActivityLogRepository.SaveChanges();
            }

            return null;
        }
    }
}