using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.ActivityManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.ActivityManagement
{
    public class UpdateUserActivityLogCommandHandler : ICommandHandlerBase<UpdateUserActivityLogCommand>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserActivityLog> _userActivityLogRepository;
        private readonly IRepository<StandardUserActivityLog> _standardUserActivityLogRepository;

        public UpdateUserActivityLogCommandHandler(IRepository<StandardUser> standardUserRepository,
            IRepository<Domain.Models.User> userRepository,
            IRepository<UserActivityLog> userActivityLogRepository,
            IRepository<StandardUserActivityLog> standardUserActivityLogRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
            _userActivityLogRepository = userActivityLogRepository;
            _standardUserActivityLogRepository = standardUserActivityLogRepository;
        }

        public CommandResponse Execute(UpdateUserActivityLogCommand command)
        {
            var user = _userRepository.Find(command.PreviousUserId);
            if (user != null)
            {
                var newUser = _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(user.EmailAddress));

                if (newUser != null)
                {
                    var userActivity = _userActivityLogRepository.List.Where(u => u.UserId.Equals(user.Id)).ToList();
                    if (userActivity.Count > 0)
                    {
                        foreach (var entry in userActivity)
                        {
                            var newEntry = new StandardUserActivityLog
                            {
                                Id = Guid.NewGuid(),
                                User = newUser,
                                Description = entry.Description,
                                ActivityDate = entry.ActivityDate
                            };
                            switch (entry.ActivityType)
                            {
                                case UserActivityEnum.CreateCustomerUser:
                                    newEntry.ActivityType = StandardUserActivityLog.CreateCustomerUser;
                                    break;

                                case UserActivityEnum.CreateTrainingGuide:
                                    newEntry.ActivityType = StandardUserActivityLog.CreateTrainingGuide;
                                    break;

                                case UserActivityEnum.CreateTrainingTest:
                                    newEntry.ActivityType = StandardUserActivityLog.CreateTrainingTest;
                                    break;

                                case UserActivityEnum.DeleteCustomerUser:
                                    newEntry.ActivityType = StandardUserActivityLog.DeleteCustomerUser;
                                    break;

                                case UserActivityEnum.DeleteTrainingGuide:
                                    newEntry.ActivityType = StandardUserActivityLog.DeleteTrainingGuide;
                                    break;

                                case UserActivityEnum.DeleteTrainingTest:
                                    newEntry.ActivityType = StandardUserActivityLog.DeleteTrainingTest;
                                    break;

                                case UserActivityEnum.EditTrainingGuide:
                                    newEntry.ActivityType = StandardUserActivityLog.EditTrainingGuide;
                                    break;

                                case UserActivityEnum.EditTrainingTest:
                                    newEntry.ActivityType = StandardUserActivityLog.EditTrainingTest;
                                    break;

                                case UserActivityEnum.Login:
                                    newEntry.ActivityType = StandardUserActivityLog.Login;
                                    break;

                                case UserActivityEnum.Logout:
                                    newEntry.ActivityType = StandardUserActivityLog.Logout;
                                    break;

                                case UserActivityEnum.ResetPassword:
                                    newEntry.ActivityType = StandardUserActivityLog.ResetPassword;
                                    break;

                                case UserActivityEnum.TakeTest:
                                    newEntry.ActivityType = StandardUserActivityLog.TakeTest;
                                    break;

                                case UserActivityEnum.UpdateCompanyProfile:
                                    newEntry.ActivityType = StandardUserActivityLog.UpdateCompanyProfile;
                                    break;

                                case UserActivityEnum.UpdateProfile:
                                    newEntry.ActivityType = StandardUserActivityLog.UpdateProfile;
                                    break;

                                case UserActivityEnum.UpdateUserProfile:
                                    newEntry.ActivityType = StandardUserActivityLog.UpdateUserProfile;
                                    break;

                                case UserActivityEnum.ViewTrainingGuide:
                                    newEntry.ActivityType = StandardUserActivityLog.ViewTrainingGuide;
                                    break;

                                default:
                                    break;
                            }
                            _standardUserActivityLogRepository.Add(newEntry);
                        }
                        _standardUserActivityLogRepository.SaveChanges();
                    }
                }
            }

            return null;
        }
    }
}