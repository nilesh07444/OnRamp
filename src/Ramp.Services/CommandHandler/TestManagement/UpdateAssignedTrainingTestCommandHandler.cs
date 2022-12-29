using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using System.Linq;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class UpdateAssignedTrainingTestCommandHandler : ICommandHandlerBase<UpdateAssignedTrainingTestCommand>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<TestAssigned> _assignedTrainingTestRepository;

        public UpdateAssignedTrainingTestCommandHandler(IRepository<User> userRepository,
            IRepository<StandardUser> standardUserRepository,
            IRepository<TestAssigned> assignedTrainingTestRepository)
        {
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
            _assignedTrainingTestRepository = assignedTrainingTestRepository;
        }

        public CommandResponse Execute(UpdateAssignedTrainingTestCommand command)
        {
            var user = _userRepository.Find(command.PreviousUserId);
            if (user != null)
            {
                var newUser = _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(user.EmailAddress));

                if (newUser != null)
                {
                    var assignedTests = _assignedTrainingTestRepository.List.Where(t => t.UserId.Equals(user.Id));

                    foreach (var assignedTest in assignedTests)
                    {
                        assignedTest.UserId = newUser.Id;
                    }
                }
                _assignedTrainingTestRepository.SaveChanges();
            }

            return null;
        }
    }
}