using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Linq;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class UsersLoginStatsQueryHandler :
        QueryHandlerBase<UsersLoginStatsQueryParameter, UserLoginFrequencyViewModel>
    {
        private readonly IRepository<UserLoginStats> _userLoginStatsRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<StandardUserLoginStats> _standardUserLoginStatsRepository;
        private readonly IRepository<Domain.Models.User> _userRepository;

        public UsersLoginStatsQueryHandler(IRepository<Domain.Models.User> userRepository,
            IRepository<UserLoginStats> userLoginStatsRepository,
            IRepository<StandardUser> standardUserRepository,
            IRepository<StandardUserLoginStats> standardUserLoginStatsRepository)
        {
            _userRepository = userRepository;
            _userLoginStatsRepository = userLoginStatsRepository;
            _standardUserRepository = standardUserRepository;
            _standardUserLoginStatsRepository = standardUserLoginStatsRepository;
        }

        public override UserLoginFrequencyViewModel ExecuteQuery(UsersLoginStatsQueryParameter queryParameters)
        {
            var startDate = queryParameters.FromDate;
            startDate = startDate.AddHours(-12);
            var endDate = queryParameters.ToDate;

            var userLoginData = _userLoginStatsRepository.List
                .Where(u => !u.LoggedInUserId.Equals(queryParameters.CurrentUserId) && u.LogInTime.Date >= startDate.Date && u.LogInTime.Date <= endDate.Date && u.LoggedInUser.CompanyId.Equals(queryParameters.CompanyId)).ToList();

            var standardUserLoginData = _standardUserLoginStatsRepository.List.Where(l => l.LogInTime.Date >= startDate.Date && l.LogInTime.Date <= endDate.Date).ToList();

            var totalNumberOfDaysSelected = queryParameters.ToDate.Subtract(queryParameters.FromDate).TotalDays;
            long count = userLoginData.Count + standardUserLoginData.Count;
            var userLoginFrequencyView = new UserLoginFrequencyViewModel
            {
                AllLoginsForSystem = count,
                NumberOfDaysInTimeSpan = totalNumberOfDaysSelected
            };

            return userLoginFrequencyView;
        }
    }
}