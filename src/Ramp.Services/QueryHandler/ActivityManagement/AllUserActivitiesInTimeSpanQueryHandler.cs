using Common;
using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.Portal;
using Ramp.Contracts.QueryParameter.ActivityManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.ActivityManagement
{
    public class AllUserActivitiesInTimeSpanQueryHandler :
        QueryHandlerBase<AllUserActivitiesInTimeSpanQueryParameter, List<UserActivityLogViewModel>>
    {
        private readonly IRepository<UserActivityLog> _userActivityLogRepository;
        public AllUserActivitiesInTimeSpanQueryHandler(IRepository<UserActivityLog> userActivityLogRepository,
            IRepository<StandardUserDisclaimerActivityLog> userDisclaimerActivityLogRepository)
        {
            _userActivityLogRepository = userActivityLogRepository;
        }

        public override List<UserActivityLogViewModel> ExecuteQuery(AllUserActivitiesInTimeSpanQueryParameter queryParameters)
        {
            var userActivityList = new List<UserActivityLogViewModel>();
            if (queryParameters.SelectedOption.Equals(AllUserActivitiesInTimeSpanQueryParameter.ProvisionalUsers))
            {
                var userActivities = _userActivityLogRepository.List.AsQueryable();
                if (!queryParameters.UserId.Equals(Guid.Empty))
                    userActivities = userActivities.Where(a => a.UserId.Equals(queryParameters.UserId))
                            .OrderByDescending(u => u.ActivityDate);
                if (queryParameters.FromDate != default(DateTime))
                {
                    queryParameters.FromDate = queryParameters.FromDate.AtBeginningOfDay();
                    userActivities = userActivities.Where(a => a.ActivityDate >= queryParameters.FromDate);
                }
                if (queryParameters.ToDate != default(DateTime))
                {
                    queryParameters.ToDate = queryParameters.ToDate.AtEndOfDay();
                    userActivities = userActivities.Where(a => a.ActivityDate <= queryParameters.ToDate);
                }

                userActivities.OrderByDescending(a => a.ActivityDate).ToList().ForEach(a => userActivityList.Add(Project.UserActivityLogViewModelFrom(a)));

            }
            else if (queryParameters.SelectedOption.Equals(AllUserActivitiesInTimeSpanQueryParameter.CompanyUsers))
            {
                if (queryParameters.CompanyId != Guid.Empty)
                {
                    new CommandDispatcher().Dispatch(new OverridePortalContextCommand
                    {
                        CompanyId = queryParameters.CompanyId
                    });
                }
                if (!queryParameters.UserId.Equals(Guid.Empty))
                {
                    userActivityList.AddRange(
                        new QueryExecutor()
                            .Execute<AllUserActivityForCustomerCompanyQuery, List<UserActivityLogViewModel>>(
                                new AllUserActivityForCustomerCompanyQuery {
                                    UserId = queryParameters.UserId ,
                                    FromDate = queryParameters.FromDate != default(DateTime) ? queryParameters.FromDate : new DateTime?(),
                                    ToDate = queryParameters.ToDate != default(DateTime) ? queryParameters.ToDate : new DateTime?()
                                }));
                }
            }
            return userActivityList.OrderByDescending(x => x.ActivityDate).ToList();
        }
    }
}