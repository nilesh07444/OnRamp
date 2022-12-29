using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.QueryParameter.Login;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.Login
{
    public class UserLoginHistoryQueryHandler : IQueryHandler<UserLoginHistoryQueryParameter, LoginStatsViewModel>
    {
        private readonly IRepository<StandardUserLoginStats> _standardUserLoginStats;
        private readonly IRepository<UserLoginStats> _userLoginStats;

        public UserLoginHistoryQueryHandler(IRepository<StandardUserLoginStats> standardUserLoginStats,
            IRepository<UserLoginStats> userLoginStats)
        {
            _standardUserLoginStats = standardUserLoginStats;
            _userLoginStats = userLoginStats;
        }

        public LoginStatsViewModel ExecuteQuery(UserLoginHistoryQueryParameter query)
        {
            var result = new LoginStatsViewModel();
            var adminLogins = _userLoginStats.List.Where(s => s.LoggedInUser.Id.Equals(query.UserId)).ToList();
            var standardUserLogins = _standardUserLoginStats.List.Where(s => s.LoggedInUser.Id.Equals(query.UserId)).ToList();
            if (adminLogins.Count > 0)
            {
                int total = adminLogins.Count;
                result = new LoginStatsViewModel { Count = total };
            }
            else if (standardUserLogins.Count > 0)
            {
                int total = standardUserLogins.Count;
                result = new LoginStatsViewModel { Count = total };
            }
            return result;
        }
    }
}