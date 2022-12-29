using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.Login
{
    public class UpdateUserLoginStatsCommandHandler : ICommandHandlerBase<UpdateUserLoginStatsCommand>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserLoginStats> _userLoginStatsRepository;
        private readonly IRepository<StandardUserLoginStats> _standardUserLoginStatsRepository;

        public UpdateUserLoginStatsCommandHandler(IRepository<StandardUser> standardUserRepository,
            IRepository<Domain.Models.User> userRepository,
            IRepository<UserLoginStats> userLoginStatsRepository,
            IRepository<StandardUserLoginStats> standardUserLoginStatsRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
            _userLoginStatsRepository = userLoginStatsRepository;
            _standardUserLoginStatsRepository = standardUserLoginStatsRepository;
        }

        public CommandResponse Execute(UpdateUserLoginStatsCommand command)
        {
            var user = _userRepository.Find(command.PreviousUserId);
            if (user != null)
            {
                var newUser = _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(user.EmailAddress));

                if (newUser != null)
                {
                    var userLoginStats =
                        _userLoginStatsRepository.List.Where(s => s.LoggedInUserId.Equals(user.Id)).ToList();
                    if (userLoginStats.Count > 0)
                    {
                        foreach (var entry in userLoginStats)
                        {
                            var newEntry = new StandardUserLoginStats
                            {
                                Id = Guid.NewGuid(),
                                LogInTime = entry.LogInTime,
                                LoggedInUser = newUser,
                                LogOutTime = entry.LogOutTime
                            };
                            _standardUserLoginStatsRepository.Add(newEntry);
                        }
                        _standardUserLoginStatsRepository.SaveChanges();
                    }
                }
            }
            return null;
        }
    }
}