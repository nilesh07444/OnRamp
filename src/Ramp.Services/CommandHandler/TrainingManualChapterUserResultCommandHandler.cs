using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.TrainingManual;
using Ramp.Contracts.Command.TrainingManual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler {
	public class TrainingManualChapterUserResultCommandHandler : CommandHandlerBase<CreateOrUpdateTrainingManualChapterUserResultCommand>,
		ICommandHandlerAndValidator<DeleteByIdCommand<TrainingManualChapter>>
	{

		readonly ITransientRepository<TrainingManualChapterUserResult> _repository;
		readonly ITransientRepository<TrainingManualChapter> _trainingManualChapterrepository;

		public TrainingManualChapterUserResultCommandHandler(ITransientRepository<TrainingManualChapterUserResult> repository, ITransientRepository<TrainingManualChapter> trainingManualChapterrepository)
		{
			_repository = repository;
			_trainingManualChapterrepository = trainingManualChapterrepository;
		}

		public override CommandResponse Execute(CreateOrUpdateTrainingManualChapterUserResultCommand command) 
		{
			var userId =string.IsNullOrEmpty(command.UserId)? (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value:command.UserId;
			
			if (command.IsGlobalAccessed) {
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == command.DocumentId && x.TrainingManualChapterId == command.TrainingManualChapterId);
				if (entity == null) {
					entity = new TrainingManualChapterUserResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						TrainingManualChapterId = command.TrainingManualChapterId,
						IsChecked = command.IsChecked,
						CreatedDate = DateTime.UtcNow,
						IssueDiscription = command.IssueDiscription,
						DocumentId = command.DocumentId,
						DateCompleted = DateTime.UtcNow,
						IsGlobalAccessed = command.IsGlobalAccessed,
						UserId = userId

					};
					if (entity.IsChecked) {
						entity.ChapterTrackedDate = DateTime.UtcNow;
					} else {
						entity.ChapterTrackedDate = null;
					}
					_repository.Add(entity);
				} else {
					entity.DateCompleted = DateTime.UtcNow;
					entity.IsChecked = command.IsChecked;
					entity.UserId = userId;
					entity.IssueDiscription = command.IssueDiscription;
					if (entity.IsChecked && entity.ChapterTrackedDate == null) {
						entity.ChapterTrackedDate = DateTime.UtcNow;
					} else if (!entity.IsChecked) {
						entity.ChapterTrackedDate = null;
					}
				}
				_repository.SaveChanges();
			} 
			else 
			{
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == command.AssignedDocumentId && x.TrainingManualChapterId == command.TrainingManualChapterId);
				if (entity == null) {
					entity = new TrainingManualChapterUserResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						TrainingManualChapterId = command.TrainingManualChapterId,
						IsChecked = command.IsChecked,
						CreatedDate = DateTime.UtcNow,
						DateCompleted = DateTime.UtcNow,
						UserId = userId,
						IssueDiscription = command.IssueDiscription,
						DocumentId = command.DocumentId,
						IsGlobalAccessed = command.IsGlobalAccessed

					};
					if (entity.IsChecked) {
						entity.ChapterTrackedDate = DateTime.UtcNow;
					} else {
						entity.ChapterTrackedDate = null;
					}

					_repository.Add(entity);
				} else {
					entity.DateCompleted = DateTime.UtcNow;
					entity.UserId = userId;
					entity.IsChecked = command.IsChecked;
					entity.IssueDiscription = command.IssueDiscription;
					if (entity.IsChecked && entity.ChapterTrackedDate == null) {
						entity.ChapterTrackedDate = DateTime.UtcNow;
					} else if (!entity.IsChecked) {
						entity.ChapterTrackedDate = null;
					}
				}
				_repository.SaveChanges();
			}
			return null;
		}

		public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<TrainingManualChapter> argument)
		{
			if (_trainingManualChapterrepository.Find(argument.Id) == null)
				yield return new ValidationResult("Id", $"Cannot find Acrobat field Content Box with id : {argument.Id}");
		}

		public CommandResponse Execute(DeleteByIdCommand<TrainingManualChapter> command)
		{
			var reponse = new CommandResponse()
			{
				Id = command.Id.ToString(),
				ErrorMessage = null,
				Validation = null
			};

			try
			{
				var document = _trainingManualChapterrepository.Find(command.Id);

				if (document != null)
				{
					document.Deleted = true;
					_trainingManualChapterrepository.SaveChanges();
				}
				else
				{
					reponse.ErrorMessage = "No Record Exist";
				}
			}
			catch (Exception ex)
			{
				reponse.ErrorMessage = ex.Message;
			}

			return reponse;
		}

	}
}
