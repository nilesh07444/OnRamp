using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Memos;
using Ramp.Contracts.Command.Memo;
using Ramp.Contracts.CommandParameter.Memo;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler {
	public	class MemoChapterUserUploadResultsCommandHandler : CommandHandlerBase<CreateOrUpdateMemoChapterUserUploadResultsCommand>,
		 ICommandHandlerBase<DeleteMemoUserUploadResultCommand> {

		readonly ITransientRepository<MemoChapterUserUploadResult> _repository;

		public MemoChapterUserUploadResultsCommandHandler(ITransientRepository<MemoChapterUserUploadResult> repository) {
			_repository = repository;
		}

		public override CommandResponse Execute(CreateOrUpdateMemoChapterUserUploadResultsCommand command) {
			var userId = string.IsNullOrEmpty(command.UserId) ? (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value : command.UserId;
			if (command.IsGlobalAccessed) {
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == command.DocumentId && x.MemoChapterId == command.MemoChapterId && x.UploadId == command.UploadId);
				if (entity == null) {
					entity = new MemoChapterUserUploadResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						MemoChapterId = command.MemoChapterId,
						CreatedDate = DateTime.UtcNow,
						UploadId = command.UploadId,
						DocumentId = command.DocumentId,
						IsGlobalAccessed = command.IsGlobalAccessed,
						UserId=userId, 
						isSignOff=command.isSignOff
					};
					_repository.Add(entity);
				} else {

					entity.UploadId = command.UploadId;
				}
			} else {
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == command.AssignedDocumentId && x.MemoChapterId == command.MemoChapterId && x.UploadId == command.UploadId);
				if (entity == null) {
					entity = new MemoChapterUserUploadResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						MemoChapterId = command.MemoChapterId,
						CreatedDate = DateTime.UtcNow,
						UploadId = command.UploadId,
						DocumentId = command.DocumentId,
						IsGlobalAccessed = command.IsGlobalAccessed,
						UserId = userId,
						isSignOff=command.isSignOff
					};
					_repository.Add(entity);
				} else {

					entity.UploadId = command.UploadId;
				}
			}
			_repository.SaveChanges();
			return null;
		}
		public CommandResponse Execute(DeleteMemoUserUploadResultCommand command) {
			if(command.IsGlobalAccessed) {
				var entity = _repository.List.AsQueryable().Where(x => x.DocumentId == command.DocumentId && x.MemoChapterId == command.MemoChapterId).ToList();
				if (entity != null) {
					foreach (var item in entity) {
						_repository.Delete(item);
					}
				}
				_repository.SaveChanges();
			} else {
				var entity = _repository.List.AsQueryable().Where(x => x.MemoChapterId == command.MemoChapterId && x.UploadId == command.UploadId).ToList();
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
