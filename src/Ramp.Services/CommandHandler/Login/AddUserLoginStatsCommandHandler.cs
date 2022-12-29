using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Login;
using System;

namespace Ramp.Services.CommandHandler.Login
{
    public class AddUserLoginStatsCommandHandler : CommandHandlerBase<AddUserLoginStatsCommand>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<UserLoginStats> _userLoginStatsRepository;
        private readonly IRepository<StandardUserLoginStats> _standardUserLoginStatsRepository;

        public AddUserLoginStatsCommandHandler(IRepository<StandardUser> standardUserRepository,
            IRepository<UserLoginStats> userLoginStatsRepository,
            IRepository<StandardUserLoginStats> standardUserLoginStatsRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userLoginStatsRepository = userLoginStatsRepository;
            _standardUserLoginStatsRepository = standardUserLoginStatsRepository;
        }

        public override CommandResponse Execute(AddUserLoginStatsCommand command)
        {
            if (command.IsUserLoggedIn && !command.UserLoginStatsId.Equals(Guid.Empty))
            {
                if (_userLoginStatsRepository.Find(command.UserLoginStatsId) != null)
                {
                    var userLoginStatistic = _userLoginStatsRepository.Find(command.UserLoginStatsId);
                    userLoginStatistic.LogOutTime = DateTime.Now;
                    _userLoginStatsRepository.SaveChanges();
                }
                else if (_standardUserLoginStatsRepository.Find(command.UserLoginStatsId) != null)
                {
                    var standardUserLoginStatistic = _standardUserLoginStatsRepository.Find(command.UserLoginStatsId);
                    standardUserLoginStatistic.LogOutTime = DateTime.Now;
                    _standardUserLoginStatsRepository.SaveChanges();
                }
            }
            else
            {
                if (!command.StandardUser)
                {
                    var userLoginStats = new UserLoginStats
                    {
                        Id = command.UserLoginStatsId,
                        LogInTime = command.LogInTime,
                        LoggedInUserId = command.LoggedInUserId
                    };
                    _userLoginStatsRepository.Add(userLoginStats);
                    _userLoginStatsRepository.SaveChanges();
                }
                else
                {
                    var standardUser = _standardUserRepository.Find(command.LoggedInUserId);
                    if (standardUser != null)
                    {
                        var standardUserLoginStats = new StandardUserLoginStats()
                        {
                            Id = command.UserLoginStatsId,
                            LogInTime = command.LogInTime,
                            LoggedInUser = standardUser
                        };
                        _standardUserLoginStatsRepository.Add(standardUserLoginStats);
                        _standardUserLoginStatsRepository.SaveChanges();
                    }
                }
            }
            return null;
        }
    }
}