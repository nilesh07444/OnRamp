using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Policy;
using Ramp.Contracts.Command.Policy;
using Ramp.Contracts.CommandParameter.Policy;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;


namespace Ramp.Services.CommandHandler
{
	public class PolicyContentBoxUserUploadResultsCommandHandler : CommandHandlerBase<CreateOrUpdatePolicyContentBoxUserUploadResultsCommand>,
		 ICommandHandlerBase<DeletePolicyContentUserUploadResultCommand>
	{

		readonly ITransientRepository<PolicyContentBoxUserUploadResult> _repository;

		public PolicyContentBoxUserUploadResultsCommandHandler(ITransientRepository<PolicyContentBoxUserUploadResult> repository)
		{
			_repository = repository;
		}

		public override CommandResponse Execute(CreateOrUpdatePolicyContentBoxUserUploadResultsCommand command)
		{
			var userId = string.IsNullOrEmpty(command.UserId) ? (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value : command.UserId;
			if (command.IsGlobalAccessed)
			{
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == command.DocumentId && x.PolicyContentBoxId == command.PolicyContentBoxId && x.UploadId == command.UploadId);
				if (entity == null)
				{
					entity = new PolicyContentBoxUserUploadResult
					{
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						PolicyContentBoxId = command.PolicyContentBoxId,
						CreatedDate = DateTime.UtcNow,
						UploadId = command.UploadId,
						DocumentId = command.DocumentId,
						IsGlobalAccessed = command.IsGlobalAccessed,
						UserId = userId,
						isSignOff = command.isSignOff

					};
					_repository.Add(entity);
				}
				else
				{

					entity.UploadId = command.UploadId;
				}
			}
			else
			{
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == command.AssignedDocumentId && x.PolicyContentBoxId == command.PolicyContentBoxId && x.UploadId == command.UploadId);
				if (entity == null)
				{
					entity = new PolicyContentBoxUserUploadResult
					{
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						PolicyContentBoxId = command.PolicyContentBoxId,
						CreatedDate = DateTime.UtcNow,
						UploadId = command.UploadId,
						DocumentId = command.DocumentId,
						IsGlobalAccessed = command.IsGlobalAccessed,
						UserId = userId,
						isSignOff = command.isSignOff
					};
					_repository.Add(entity);
				}
				else
				{

					entity.UploadId = command.UploadId;
				}
			}
			_repository.SaveChanges();
			return null;
		}

		public CommandResponse Execute(DeletePolicyContentUserUploadResultCommand command)
		{
			if (command.IsGlobalAccessed)
			{
				var entity = _repository.List.AsQueryable().Where(x => x.DocumentId == command.DocumentId && x.PolicyContentBoxId == command.PolicyContentBoxId).ToList();
				if (entity != null)
				{
					foreach (var item in entity)
					{
						_repository.Delete(item);
					}
				}
				_repository.SaveChanges();
			}
			else
			{
				var entity = _repository.List.AsQueryable().Where(x => x.PolicyContentBoxId == command.PolicyContentBoxId && x.UploadId == command.UploadId).ToList();
				if (entity != null)
				{
					foreach (var item in entity)
					{
						_repository.Delete(item);
					}
				}
				_repository.SaveChanges();
			}
			return null;
		}

	}
}
