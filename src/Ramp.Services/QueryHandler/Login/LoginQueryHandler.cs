using Common.Data;
using Common.Query;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.Login;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.Login
{
    public class LoginQueryHandler : QueryHandlerBase<LoginQueryParameters, List<UserLoginStats>>
    {
        private readonly IRepository<UserLoginStats> _userLoginStatsRepository;

        public LoginQueryHandler(IRepository<UserLoginStats> userRepository)
        {
            _userLoginStatsRepository = userRepository;
        }

        public override List<UserLoginStats> ExecuteQuery(LoginQueryParameters queryParameters)
        {
            if (queryParameters.IsUserLoggedIn)
            {
                var userLoginStats = _userLoginStatsRepository.List.Where(em => em.LoggedInUserId == queryParameters.LoggedInUserId).ToList();
                return userLoginStats;
            }
            else
            {
                return null;
            }
        }
    }
}