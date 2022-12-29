using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models.CheckLists;
using Ramp.Contracts.Command.CheckList;
using Ramp.Contracts.CommandParameter.CheckList;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler {
	public	class CheckListChapterUserUploadResultsCommandHandler : CommandHandlerBase<CreateOrUpdateCheckListChapterUserUploadResultsCommand>,
		 ICommandHandlerBase<DeleteCheckListUserUploadResultCommand> {

		readonly ITransientRepository<CheckListChapterUserUploadResult> _repository;

		public CheckListChapterUserUploadResultsCommandHandler(ITransientRepository<CheckListChapterUserUploadResult> repository) {
			_repository = repository;
		}

		public override CommandResponse Execute(CreateOrUpdateCheckListChapterUserUploadResultsCommand command) {
			var userId = string.IsNullOrEmpty(command.UserId) ? (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value : command.UserId;
			if (command.IsGlobalAccessed) {
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == command.DocumentId && x.CheckListChapterId == command.CheckListChapterId && x.UploadId == command.UploadId);
				if (entity == null) {
					entity = new CheckListChapterUserUploadResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						CheckListChapterId = command.CheckListChapterId,
						CreatedDate = DateTime.UtcNow,
						UploadId = command.UploadId,
						DocumentId = command.DocumentId,
						IsGlobalAccessed = command.IsGlobalAccessed,
						UserId= userId,
							isSignOff = command.isSignOff

					};
					_repository.Add(entity);
				} else {

					entity.UploadId = command.UploadId;
				}
			} else {
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == command.AssignedDocumentId && x.CheckListChapterId == command.CheckListChapterId && x.UploadId == command.UploadId);
				if (entity == null) {
					entity = new CheckListChapterUserUploadResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						CheckListChapterId = command.CheckListChapterId,
						CreatedDate = DateTime.UtcNow,
						UploadId = command.UploadId,
						DocumentId = command.DocumentId,
						IsGlobalAccessed = command.IsGlobalAccessed,
						UserId = userId,
						isSignOff = command.isSignOff
					};
					_repository.Add(entity);
				} else {

					entity.UploadId = command.UploadId;
				}
			}
			_repository.SaveChanges();
			return null;
		}
		public CommandResponse Execute(DeleteCheckListUserUploadResultCommand command) {
			if(command.IsGlobalAccessed) {
				var entity = _repository.List.AsQueryable().Where(x => x.DocumentId == command.DocumentId && x.CheckListChapterId == command.CheckListChapterId).ToList();
				if (entity != null) {
					foreach (var item in entity) {
						_repository.Delete(item);
					}
				}
				_repository.SaveChanges();
			} else {
				var entity = _repository.List.AsQueryable().Where(x => x.CheckListChapterId == command.CheckListChapterId && x.UploadId == command.UploadId).ToList();
				if (entity != null) {
					foreach (var item in entity) {
						_repository.Delete(item);
					}
				}
				_repository.SaveChanges();
			}
			return null;
		}

	}
}
