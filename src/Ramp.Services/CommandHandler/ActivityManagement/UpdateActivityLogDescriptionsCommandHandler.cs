using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.ActivityManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.ActivityManagement
{
    public class UpdateActivityLogDescriptionsCommandHandler : ICommandHandlerBase<UpdateActivityLogDescriptionsCommand>
    {
        private readonly IRepository<StandardUserActivityLog> _standardUserActivityLogRepository;

        public UpdateActivityLogDescriptionsCommandHandler(IRepository<StandardUserActivityLog> standardUserActivityLogRepository)
        {
            _standardUserActivityLogRepository = standardUserActivityLogRepository;
        }

        public CommandResponse Execute(UpdateActivityLogDescriptionsCommand command)
        {
            foreach (var entry in _standardUserActivityLogRepository.List)
            {
                if (command.UpdateActivityIdentity)
                {
                    if (entry.ActivityType.Equals(StandardUserActivityLog.CreateCustomerUserDesc))
                        entry.ActivityType = StandardUserActivityLog.CreateCustomerUser;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.CreateTrainingGuideDesc))
                        entry.ActivityType = StandardUserActivityLog.CreateTrainingGuide;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.CreateTrainingTestDesc))
                        entry.ActivityType = StandardUserActivityLog.CreateTrainingTest;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.DeleteCustomerUserDesc))
                        entry.ActivityType = StandardUserActivityLog.DeleteCustomerUser;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.DeleteTrainingGuideDesc))
                        entry.ActivityType = StandardUserActivityLog.DeleteTrainingGuide;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.DeleteTrainingTestDesc))
                        entry.ActivityType = StandardUserActivityLog.DeleteTrainingTest;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.EditTrainingGuideDesc))
                        entry.ActivityType = StandardUserActivityLog.EditTrainingGuide;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.EditTrainingTestDesc))
                        entry.ActivityType = StandardUserActivityLog.EditTrainingTest;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.LoginDesc))
                        entry.ActivityType = StandardUserActivityLog.Login;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.LogoutDesc))
                        entry.ActivityType = StandardUserActivityLog.Logout;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.ResetPasswordDesc))
                        entry.ActivityType = StandardUserActivityLog.ResetPassword;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.TakeTestDesc))
                        entry.ActivityType = StandardUserActivityLog.TakeTest;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.ViewTrainingGuideDesc))
                        entry.ActivityType = StandardUserActivityLog.ViewTrainingGuide;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.UpdateCompanyProfileDesc))
                        entry.ActivityType = StandardUserActivityLog.UpdateCompanyProfile;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.UpdateProfileDesc))
                        entry.ActivityType = StandardUserActivityLog.UpdateProfile;
                    if (entry.ActivityType.Equals(StandardUserActivityLog.UpdateUserProfileDesc))
                        entry.ActivityType = StandardUserActivityLog.UpdateUserProfile;
                }
            }
            _standardUserActivityLogRepository.SaveChanges();
            return null;
        }
    }
}