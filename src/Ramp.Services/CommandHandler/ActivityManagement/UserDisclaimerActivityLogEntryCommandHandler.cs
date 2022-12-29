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
    public class UserDisclaimerActivityLogEntryCommandHandler : ICommandHandlerBase<CreateUserDiclaimerActivityLogEntryCommand>,
                                                                IValidator<CreateUserDiclaimerActivityLogEntryCommand>
    {
        private readonly IRepository<StandardUser> _userRepository;
        public UserDisclaimerActivityLogEntryCommandHandler(IRepository<StandardUser> userRepository)
        {
            _userRepository = userRepository;
        }
        public CommandResponse Execute(CreateUserDiclaimerActivityLogEntryCommand command)
        {
            var user = _userRepository.Find(command.UserId);
            var entry = new StandardUserDisclaimerActivityLog
            {
                Id = Guid.NewGuid(),
                Accepted = command.Accepted,
                IPAddress = command.IPAddress,
                Stamp = DateTime.Now,
                User = user
            };
            user.DisclaimerActivityLog.Add(entry);
            _userRepository.SaveChanges();
            return null;
        }

        public IEnumerable<IValidationResult> Validate(CreateUserDiclaimerActivityLogEntryCommand argument)
        {
            if (_userRepository.Find(argument.UserId) == null)
                yield return new ValidationResult("UserId", $"No user found with id : {argument.UserId}");
        }
    }
}
