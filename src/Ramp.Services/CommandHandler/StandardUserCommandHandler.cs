using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Command;
using Common.Data;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Command.StandardUser;
using Ramp.Contracts.Security;
using VirtuaCon;

namespace Ramp.Services.CommandHandler
{
    public class StandardUserCommandHandler : ICommandHandlerBase<DeleteDisclaimerActivityLogsCommand>,
                                              ICommandHandlerBase<UpdateUsersExpiryStatusCommand>
    {
        private readonly ITransientRepository<StandardUser> _standardUserRepository;
        private readonly ITransientRepository<StandardUserActivityLog> _activityLogRepository;
        private readonly ITransientRepository<StandardUserDisclaimerActivityLog> _disclaimerActivityLogRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        public StandardUserCommandHandler(
            ITransientRepository<StandardUser> standardUserRepository,
            ITransientRepository<StandardUserActivityLog> activityLogRepository,
            ITransientRepository<StandardUserDisclaimerActivityLog> disclaimerActivityLogRepository,
            ICommandDispatcher commandDispatcher)
        {
            _standardUserRepository = standardUserRepository;
            _activityLogRepository = activityLogRepository;
            _disclaimerActivityLogRepository = disclaimerActivityLogRepository;
            _commandDispatcher = commandDispatcher;
        }

        public CommandResponse Execute(DeleteDisclaimerActivityLogsCommand command)
        {
            _disclaimerActivityLogRepository.SetCustomerCompany(command.CompanyId.ToString());
            _disclaimerActivityLogRepository.List.Where(x => !x.Deleted).ForEach(x => x.Deleted = true);
            _disclaimerActivityLogRepository.SaveChanges();

            _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

            return null;
        }

        public CommandResponse Execute(UpdateUsersExpiryStatusCommand command)
        {
            _standardUserRepository.SetCustomerCompany(command.CompanyId.ToString());
            _activityLogRepository.SetCustomerCompany(command.CompanyId.ToString());

            var standardUsers = _standardUserRepository.List.Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)));

            foreach (var user in standardUsers)
            {
                var lastActiveDate = _activityLogRepository.List.Where(x => x.User.Id == user.Id).OrderByDescending(x => x.ActivityDate)
                    .FirstOrDefault()?.ActivityDate ?? user.CreatedOn.Value;
                var inactiveDays = Convert.ToInt32((DateTime.UtcNow - lastActiveDate).TotalDays);

                if (inactiveDays > command.DefaultUserExpireDays)
                {
                    user.IsUserExpire = true;
                    user.ExpireDays = inactiveDays - command.DefaultUserExpireDays;
                }
            }

            _standardUserRepository.SaveChanges();

            _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

            return null;
        }
    }
}
