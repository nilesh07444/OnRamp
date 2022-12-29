using Common.Command;
using Common.Data;
using Data.EF.Customer;
using Domain.Customer.Models.CheckLists;
using Ramp.Contracts.Command.CheckList;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler {
	public class CheckListUserResultCommandHandler : CommandHandlerBase<CreateOrUpdateCheckListUserResultCommand> {

		readonly IRepository<CheckListUserResult> _repository;

		public CheckListUserResultCommandHandler(IRepository<CheckListUserResult> repository) {
			_repository = repository;
		}

		public override CommandResponse Execute(CreateOrUpdateCheckListUserResultCommand command) {
			var userId = string.IsNullOrEmpty(command.UserId) ? (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value : command.UserId;
			if (command.IsGlobalAccessed) {
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == command.DocumentId);
				if (entity == null) {
					entity = new CheckListUserResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = null,
						DocumentId = command.DocumentId,
						SubmittedDate = DateTime.UtcNow,
						Status = command.Status,
						IsGlobalAccessed = command.IsGlobalAccessed,
						UserId=userId
					};
					_repository.Add(entity);
				} else {

					entity.UserId = userId;
					entity.Status = command.Status;
					entity.SubmittedDate = DateTime.UtcNow;
					entity.IsGlobalAccessed = command.IsGlobalAccessed;
					_repository.SaveChanges();
				}
			} else {
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == command.AssignedDocumentId);
				if (entity == null) {
					entity = new CheckListUserResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						SubmittedDate = DateTime.UtcNow,
						Status = command.Status,
						UserId = userId,
						IsGlobalAccessed = command.IsGlobalAccessed
					};
					_repository.Add(entity);
				} else {
					entity.UserId = userId;
					entity.Status = command.Status;
					entity.SubmittedDate = DateTime.UtcNow;
					entity.IsGlobalAccessed = command.IsGlobalAccessed;
					_repository.SaveChanges();
				}
			}

			_repository.SaveChanges();
			return null;
		}

	}
}
