using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class UpdateTrainingGuideUsageStatsCommandHandler : ICommandHandlerBase<UpdateTrainingGuideUsageStatsCommand>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<TrainingGuideusageStats> _trainingGuideUsageStatsRepository;

        public UpdateTrainingGuideUsageStatsCommandHandler(IRepository<StandardUser> standardUserRepository,
             IRepository<Domain.Models.User> userRepository,
             IRepository<TrainingGuideusageStats> trainingGuideUsageStatsRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
            _trainingGuideUsageStatsRepository = trainingGuideUsageStatsRepository;
        }

        public CommandResponse Execute(UpdateTrainingGuideUsageStatsCommand command)
        {
            var user = _userRepository.Find(command.PreviousUserId);
            if (user != null)
            {
                var newUser = _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(user.EmailAddress));

                if (newUser != null)
                {
                    var guideStats =
                        _trainingGuideUsageStatsRepository.List.Where(s => s.UserId.Equals(user.Id)).ToList();
                    if (guideStats.Count > 0)
                    {
                        guideStats.ForEach(s => s.UserId = newUser.Id);
                    }
                }
                _trainingGuideUsageStatsRepository.SaveChanges();
            }
            return null;
        }
    }
}