using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Tests;
using Ramp.Contracts.Command.Test;
using Ramp.Contracts.CommandParameter.Test;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler {
	public	class TestChapterUserUploadResultsCommandHandler : CommandHandlerBase<CreateOrUpdateTestChapterUserUploadResultsCommand>,
		 ICommandHandlerBase<DeleteTestUserUploadResultCommand> {

		readonly ITransientRepository<TestChapterUserUploadResult> _repository;

		public TestChapterUserUploadResultsCommandHandler(ITransientRepository<TestChapterUserUploadResult> repository) {
			_repository = repository;
		}

		public override CommandResponse Execute(CreateOrUpdateTestChapterUserUploadResultsCommand command) {
			var userId = string.IsNullOrEmpty(command.UserId) ? (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value : command.UserId;
			if (command.IsGlobalAccessed) {
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == command.DocumentId && x.TestChapterId == command.TestChapterId && x.UploadId == command.UploadId);
				if (entity == null) {
					entity = new TestChapterUserUploadResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						TestChapterId = command.TestChapterId,
						CreatedDate = DateTime.UtcNow,
						UploadId = command.UploadId,
						DocumentId = command.DocumentId,
						IsGlobalAccessed = command.IsGlobalAccessed,
						UserId=userId

					};
					_repository.Add(entity);
				} else {

					entity.UploadId = command.UploadId;
				}
			} else {
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == command.AssignedDocumentId && x.TestChapterId == command.TestChapterId && x.UploadId == command.UploadId);
				if (entity == null) {
					entity = new TestChapterUserUploadResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						TestChapterId = command.TestChapterId,
						CreatedDate = DateTime.UtcNow,
						UploadId = command.UploadId,
						DocumentId = command.DocumentId,
						IsGlobalAccessed = command.IsGlobalAccessed,
						UserId = userId
					};
					_repository.Add(entity);
				} else {

					entity.UploadId = command.UploadId;
				}
			}
			_repository.SaveChanges();
			return null;
		}
		public CommandResponse Execute(DeleteTestUserUploadResultCommand command) {
			if(command.IsGlobalAccessed) {
				var entity = _repository.List.AsQueryable().Where(x => x.DocumentId == command.DocumentId && x.TestChapterId == command.TestChapterId).ToList();
				if (entity != null) {
					foreach (var item in entity) {
						_repository.Delete(item);
					}
				}
				_repository.SaveChanges();
			} else {
				var entity = _repository.List.AsQueryable().Where(x => x.TestChapterId == command.TestChapterId && x.UploadId == command.UploadId).ToList();
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
