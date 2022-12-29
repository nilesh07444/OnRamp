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
    public class UpdateTestResultCommandHandler : ICommandHandlerBase<UpdateTestResultCommand>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<TestResult> _testResultRepository;

        public UpdateTestResultCommandHandler(IRepository<StandardUser> standardUserRepository,
             IRepository<Domain.Models.User> userRepository,
             IRepository<TestResult> testResultRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
            _testResultRepository = testResultRepository;
        }

        public CommandResponse Execute(UpdateTestResultCommand command)
        {
            var user = _userRepository.Find(command.PreviousUserId);
            if (user != null)
            {
                var newUser = _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(user.EmailAddress));

                if (newUser != null)
                {
                    var testResults =
                        _testResultRepository.List.Where(r => r.TestTakenByUserId.Equals(user.Id)).ToList();
                    if (testResults.Count > 0)
                        testResults.ForEach(r => r.TestTakenByUserId = newUser.Id);
                }
                _testResultRepository.SaveChanges();
            }

            return null;
        }
    }
}