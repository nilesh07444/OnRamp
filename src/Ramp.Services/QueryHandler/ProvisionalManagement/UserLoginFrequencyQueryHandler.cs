using Common;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class UserLoginFrequencyQueryHandler :
        QueryHandlerBase<UserLoginFrequencyQueryParameter, UserLoginFrequencyReportViewModel>
    {
        private readonly IRepository<UserLoginStats> _userLoginStatsRepository;
        private readonly IRepository<Domain.Models.User> _userRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<StandardUserLoginStats> _standardUserLoginStatsRepository;

        public UserLoginFrequencyQueryHandler(IRepository<UserLoginStats> userLoginStatsRepository,
            IRepository<Domain.Models.User> userRepository,
            IRepository<StandardUser> standardUserRepository,
            IRepository<StandardUserLoginStats> standardUserLoginStatsRepository)
        {
            _userLoginStatsRepository = userLoginStatsRepository;
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
            _standardUserLoginStatsRepository = standardUserLoginStatsRepository;
        }

        public override UserLoginFrequencyReportViewModel ExecuteQuery(UserLoginFrequencyQueryParameter queryParameters)
        {
            queryParameters.FromDate = queryParameters.FromDate.AtBeginningOfDay();
            queryParameters.ToDate = queryParameters.ToDate.AtEndOfDay();
            var user = _userRepository.Find(queryParameters.SelectedUserId);
            var standardUser = _standardUserRepository.Find(queryParameters.SelectedUserId);
            var userLoginFrequencyView = new UserLoginFrequencyReportViewModel();
            double noOfDays = queryParameters.ToDate.Subtract(queryParameters.FromDate).TotalDays;
            double noWeek = (noOfDays / 7);

            if (user != null)
            {
                var userLoginData = _userLoginStatsRepository.List
                    .Where(
                        u =>
                            u.LoggedInUserId == queryParameters.SelectedUserId &&
                            u.LogInTime >= queryParameters.FromDate && u.LogInTime <= queryParameters.ToDate)
                    .ToList();

                var userList = userLoginData
                    .GroupBy(l => l.LoggedInUserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        UserLoginCount = g.Select(l => l.LoggedInUser).Count(),
                        loginFrequencyPerWeek = (double)g.Select(l => l.LoggedInUser).Count() / noWeek,
                    }).OrderBy(e => e.UserId);

                foreach (var currentUser in userList)
                {
                    var theuser = _userRepository.Find(currentUser.UserId);

                    userLoginFrequencyView.LoginFrequency = Math.Round(currentUser.loginFrequencyPerWeek, 2);
                    userLoginFrequencyView.UserLoginCount = currentUser.UserLoginCount;
                    if (theuser.FirstName.Equals(theuser.LastName))
                        userLoginFrequencyView.UserName = theuser.FirstName;
                    else
                    {
                        userLoginFrequencyView.UserName = $"{theuser.FirstName} {theuser.LastName}";
                    }
                }
            }
            if (standardUser != null)
            {
                var standardUserLoginData = _standardUserLoginStatsRepository.List.Where(
                    l => l.LoggedInUser.Id.Equals(standardUser.Id)
                         && l.LogInTime >= queryParameters.FromDate
                         && l.LogInTime <= queryParameters.ToDate).ToList();

                userLoginFrequencyView.LoginFrequency = Math.Round(standardUserLoginData.Count / noWeek);
                userLoginFrequencyView.UserLoginCount = standardUserLoginData.Count;
                if (standardUser.FirstName.Equals(standardUser.LastName))
                    userLoginFrequencyView.UserName = standardUser.FirstName;
                else
                {
                    userLoginFrequencyView.UserName = $"{standardUser.FirstName} {standardUser.LastName}";
                }
            }
            return userLoginFrequencyView;
        }
    }
}