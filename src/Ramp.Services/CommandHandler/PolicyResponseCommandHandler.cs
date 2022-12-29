using System;
using System.Collections.Generic;
using System.Linq;
using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models.PolicyResponse;
using Ramp.Contracts.Command.PolicyResponse;
using Ramp.Contracts.Events.Account;

namespace Ramp.Services.CommandHandler
{
    public class PolicyResponseCommandHandler : ICommandHandlerAndValidator<CreatePolicyResponseCommand>,IEventHandler<StandardUserDeletedEvent>
    {
        private readonly IRepository<PolicyResponse> _policyResponseRepository;

        public PolicyResponseCommandHandler(
            IRepository<PolicyResponse> policyResponseRepository)
        {
            _policyResponseRepository = policyResponseRepository;
        }

        public IEnumerable<IValidationResult> Validate(CreatePolicyResponseCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.UserId))
                yield return new ValidationResult(nameof(command.UserId), "UserId is required");
            if (string.IsNullOrWhiteSpace(command.PolicyId))
                yield return new ValidationResult(nameof(command.PolicyId), "PolicyId is required");
        }

        public CommandResponse Execute(CreatePolicyResponseCommand command)
        {
            var response = new PolicyResponse
            {
                Id = Guid.NewGuid().ToString(),
                PolicyId = command.PolicyId,
                UserId = command.UserId,
                Created = DateTime.UtcNow,
                Response = command.Response,
				IsGlobalAccessed=command.IsGlobalAccessed
            };

            _policyResponseRepository.Add(response);

            return null;
        }

        public void Handle(StandardUserDeletedEvent @event)
        {
            if (!string.IsNullOrWhiteSpace(@event.Id))
            {
                _policyResponseRepository.List.AsQueryable().Where(x => x.UserId == @event.Id).ToList().ForEach(x => _policyResponseRepository.Delete(x));
                _policyResponseRepository.SaveChanges();
            }
        }
    }
}