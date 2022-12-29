using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Query.StandardUser;
using Ramp.Contracts.Security;

namespace Ramp.Services.QueryHandler
{
    public class StandardUserQueryHandler : IQueryHandler<ActiveCustomerAdminsQuery,IEnumerable<StandardUser>>,
                                            IQueryHandler<NewExpiredUsersQuery, IEnumerable<StandardUser>>
    {
        private readonly ITransientRepository<StandardUser> _standardUserRepository;
        private readonly ITransientRepository<StandardUserActivityLog> _activityLogRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        public StandardUserQueryHandler(
            ITransientRepository<StandardUser> standardUserRepository,
            ITransientRepository<StandardUserActivityLog> activityLogRepository,
            ICommandDispatcher commandDispatcher)
        {
            _standardUserRepository = standardUserRepository;
            _activityLogRepository = activityLogRepository;
            _commandDispatcher = commandDispatcher;
        }

        public IEnumerable<StandardUser> ExecuteQuery(ActiveCustomerAdminsQuery query)
        {
            _standardUserRepository.SetCustomerCompany(query.CompanyId.ToString());

            var admins = _standardUserRepository.List.Where(u =>
                u.Roles.Any(r => r.RoleName.Equals(Role.CustomerAdmin)) && u.IsActive && !u.IsUserExpire).ToList();

            _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

            return admins;
        }

        public IEnumerable<StandardUser> ExecuteQuery(NewExpiredUsersQuery query)
        {
            _standardUserRepository.SetCustomerCompany(query.CompanyId.ToString());
            _activityLogRepository.SetCustomerCompany(query.CompanyId.ToString());

            var activeUsers = _standardUserRepository.List.Where(u =>
                u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)) && u.IsActive && !u.IsUserExpire);

            var newExpiredUsers = new List<StandardUser>();

            foreach (var user in activeUsers)
            {
                var lastActiveDate = _activityLogRepository.List.Where(x => x.User.Id == user.Id).OrderByDescending(x => x.ActivityDate)
                                         .FirstOrDefault()?.ActivityDate ?? user.CreatedOn.Value;
                var inactiveDays = Convert.ToInt32((DateTime.UtcNow - lastActiveDate).TotalDays);

                if (inactiveDays > query.DefaultUserExpireDays)
                {
                    newExpiredUsers.Add(user);
                }
            }

            _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

            return newExpiredUsers;
        }
    }
}
