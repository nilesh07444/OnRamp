using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.CommandParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler
{
    public class CommandAuditTrailCommandHandler : ICommandHandlerBase<CreateCommandAuditTrailCommand>,ICommandHandlerAndValidator<EditCommandAuditTrailCommand>
    {
        private readonly IRepository<CommandAuditTrail> _repository;
        public CommandAuditTrailCommandHandler(IRepository<CommandAuditTrail> repository)
        {
            _repository = repository;
        }

        public CommandResponse Execute(CreateCommandAuditTrailCommand command)
        {
            if (_repository.Find(command.Id) != null)
                command.Id = Guid.NewGuid();
            _repository.Add(new CommandAuditTrail
            {
                Command = command.Command,
                Id = command.Id,
                CommandType = command.CommandType,
                Executed = command.Executed
            });
			_repository.SaveChanges();
            return null;
        }

        public CommandResponse Execute(EditCommandAuditTrailCommand command)
        {
            var e = _repository.Find(command.Id);
            e.Executed = command.Executed;
            _repository.SaveChanges();
            return null;
        }

        public IEnumerable<IValidationResult> Validate(EditCommandAuditTrailCommand command)
        {
            if (_repository.Find(command.Id) == null)
                yield return new ValidationResult("Id", "No Found");
        }
    }
}
