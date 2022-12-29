using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Command.TrainingManual;
using Ramp.Contracts.CommandParameter.TrainingMannual;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler {
	public	class TrainingMannualChapterUserUploadResultsCommandHandler : CommandHandlerBase<CreateOrUpdateTrainingManualChapterUserUploadResultsCommand>,
		 ICommandHandlerBase<DeleteTrainingMannualUserUploadResultCommand> {

		readonly ITransientRepository<TrainingManualChapterUserUploadResult> _repository;		
		public TrainingMannualChapterUserUploadResultsCommandHandler(ITransientRepository<TrainingManualChapterUserUploadResult> repository) 
		{
			_repository = repository;
		}

		public override CommandResponse Execute(CreateOrUpdateTrainingManualChapterUserUploadResultsCommand command) 
		{
			var userId = string.IsNullOrEmpty(command.UserId) ? (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value : command.UserId;
			if (command.IsGlobalAccessed) {
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == command.DocumentId && x.TrainingManualChapterId == command.TrainingManualChapterId && x.UploadId == command.UploadId);
				if (entity == null) {
					entity = new TrainingManualChapterUserUploadResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						TrainingManualChapterId = command.TrainingManualChapterId,
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
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == command.AssignedDocumentId && x.TrainingManualChapterId == command.TrainingManualChapterId && x.UploadId == command.UploadId);
				if (entity == null) {
					entity = new TrainingManualChapterUserUploadResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						TrainingManualChapterId = command.TrainingManualChapterId,
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
		public CommandResponse Execute(DeleteTrainingMannualUserUploadResultCommand command) 
		{
			if(command.IsGlobalAccessed) {
				var entity = _repository.List.AsQueryable().Where(x => x.DocumentId == command.DocumentId && x.TrainingManualChapterId == command.TrainingMannualChapterId).ToList();
				if (entity != null) {
					foreach (var item in entity) {
						_repository.Delete(item);
					}
				}
				_repository.SaveChanges();
			} else {
				var entity = _repository.List.AsQueryable().Where(x => x.TrainingManualChapterId == command.TrainingMannualChapterId && x.UploadId == command.UploadId).ToList();
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
