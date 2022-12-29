using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using ikvm.extensions;
using Ramp.Contracts.CommandParameter.TestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class UpdateTrainingTestUserCommandHandler : ICommandHandlerBase<UpdateTrainingTestUserCommand>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public UpdateTrainingTestUserCommandHandler(IRepository<StandardUser> standardUserRepository,
             IRepository<Domain.Models.User> userRepository,
             IRepository<TrainingTest> trainingTestRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
            _trainingTestRepository = trainingTestRepository;
        }

        public CommandResponse Execute(UpdateTrainingTestUserCommand command)
        {
            var user = _userRepository.Find(command.PreviousUserId);
            if (user != null)
            {
                var newUser = _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(user.EmailAddress));

                if (newUser != null)
                {
                    var tests = _trainingTestRepository.List.Where(t => t.CreatedBy.Equals(user.Id)).ToList();
                    if (tests.Count > 0)
                    {
                        tests.ForEach(t => t.CreatedBy = newUser.Id);
                    }
                }
                _trainingTestRepository.SaveChanges();
            }
            return null;
        }
    }
}