using Common.Command;
using Common.Data;
using Ramp.Contracts.Command.Label;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models;
using Data.EF.Customer;
using Ramp.Contracts.CommandParameter.TrainingLabel;
using Common;
using Common.Events;
using Ramp.Contracts.Events;

namespace Ramp.Services.CommandHandler
{
    public class LabelCommandHandler : ICommandHandlerBase<SyncLabelCommand>,
                                       ICommandHandlerAndValidator<CreateOrUpdateTrainingLableCommand>,
                                       ICommandHandlerAndValidator<DeleteByIdCommand<Label>>
    {
        readonly ITransientRepository<Label> _repository;
        readonly IEventPublisher _eventPublisher;
        public LabelCommandHandler(ITransientRepository<Label> repository,IEventPublisher eventPublisher)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
        }
        public CommandResponse Execute(SyncLabelCommand command)
        {
            var allLables = _repository.List.Select(l => l.Name).ToList();
            foreach (var label in command.Values.Where(l => !string.IsNullOrEmpty(l)).ToList())
            {
                if (!allLables.Contains(label))
                    _repository.Add(new Label { Name = label, Description = label, Id = Guid.NewGuid().ToString() });
                else
                {
                    var l = _repository.List.AsQueryable().FirstOrDefault(x => x.Name == label);
                    if (l != null)
                        l.Deleted = false;
                }
            }
            
            _repository.SaveChanges();
            return null;
        }

        public CommandResponse Execute(DeleteByIdCommand<Label> command)
        {
            var e = _repository.Find(command.Id);
            e.Deleted = true;
            _repository.SaveChanges();
            _eventPublisher.Publish(new LabelDeletedEvent { Name = e.Name });
            return null;
        }

        public CommandResponse Execute(CreateOrUpdateTrainingLableCommand command)
        {
            var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.Id == command.Id || x.Name == command.Name);
            if (entity == null)
            {
                entity = new Label
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = command.Description,
                    Name = command.Name
                };
                _repository.Add(entity);
            }
            else
            {
                entity.Description = command.Description;
                entity.Name = command.Name;
                entity.Deleted = false;
            }
            _repository.SaveChanges();
            return null;
        }

        public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<Label> argument)
        {
            if (_repository.Find(argument.Id) == null)
                yield return new ValidationResult("Id", $"Cannot find TrainingLabel with id : {argument.Id}");
        }

        public IEnumerable<IValidationResult> Validate(CreateOrUpdateTrainingLableCommand argument)
        {
            if (!string.IsNullOrWhiteSpace(argument.Id))
            {
                var entity = _repository.Find(argument.Id);
                if (entity == null)
                    yield return new ValidationResult("Id", $"No Training Lable With Id : {argument.Id}");
                if (!string.IsNullOrWhiteSpace(argument.Name))
                {
                    if (!argument.Name.Equals(entity.Name))
                        if (_repository.List.AsQueryable().Count(x => x.Name.Equals(argument.Name)) > 0)
                            yield return new ValidationResult("Name", "Name Must Be Unique");
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(argument.Name) && _repository.List.AsQueryable().Any(x => x.Name.Equals(argument.Name) && !x.Deleted))
                    yield return new ValidationResult("Name", "Name Must Be Unique");

            }
        }
    }
}
