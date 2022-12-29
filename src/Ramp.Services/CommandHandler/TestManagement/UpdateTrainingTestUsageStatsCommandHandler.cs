using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class UpdateTrainingTestUsageStatsCommandHandler : ICommandHandlerBase<UpdateTrainingTestUsageStatsCommand>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<TrainingTestUsageStats> _trainingTestUsageStatsRepository;

        public UpdateTrainingTestUsageStatsCommandHandler(IRepository<StandardUser> standardUserRepository,
             IRepository<Domain.Models.User> userRepository,
             IRepository<TrainingTestUsageStats> trainingTestUsageStatsRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
            _trainingTestUsageStatsRepository = trainingTestUsageStatsRepository;
        }

        public CommandResponse Execute(UpdateTrainingTestUsageStatsCommand command)
        {
            var user = _userRepository.Find(command.PreviousUserId);
            if (user != null)
            {
                var newUser = _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(user.EmailAddress));

                if (newUser != null)
                {
                    var testUsageStats =
                        _trainingTestUsageStatsRepository.List.Where(s => s.UserId.Equals(user.Id)).ToList();
                    if (testUsageStats.Count > 0)
                    {
                        testUsageStats.ForEach(s => s.UserId = newUser.Id);
                    }
                }
                _trainingTestUsageStatsRepository.SaveChanges();
            }

            return null;
        }
    }
}