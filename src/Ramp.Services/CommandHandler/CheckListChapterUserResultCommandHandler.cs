using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models.CheckLists;
using Ramp.Contracts.Command.CheckList;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler {
	public class CheckListChapterUserResultCommandHandler : CommandHandlerBase<CreateOrUpdateCheckListChapterUserResultCommand> {

		readonly ITransientRepository<CheckListChapterUserResult> _repository;

		public CheckListChapterUserResultCommandHandler(ITransientRepository<CheckListChapterUserResult> repository) {
			_repository = repository;
		}

		public override CommandResponse Execute(CreateOrUpdateCheckListChapterUserResultCommand command) {
			var userId =string.IsNullOrEmpty(command.UserId)? (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value:command.UserId;
			if (command.IsGlobalAccessed) {
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == command.DocumentId && x.CheckListChapterId == command.CheckListChapterId);
				if (entity == null) {
					entity = new CheckListChapterUserResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						CheckListChapterId = command.CheckListChapterId,
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
			} else {
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == command.AssignedDocumentId && x.CheckListChapterId == command.CheckListChapterId);
				if (entity == null) {
					entity = new CheckListChapterUserResult {
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						CheckListChapterId = command.CheckListChapterId,
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
	}
}
