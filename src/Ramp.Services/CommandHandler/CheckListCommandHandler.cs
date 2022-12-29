using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Ramp.Contracts.Command.CheckList;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using VirtuaCon;
using Common;
using Ramp.Services.Helpers;
using Domain.Customer.Models.DocumentTrack;
using Domain.Customer;
using Domain.Customer.Models.Restore;
using Common.Data;
using Domain.Customer.Models.Document;
using ikvm.extensions;
using Domain.Enums;

namespace Ramp.Services.CommandHandler {
	public class CheckListCommandHandler : CommandHandlerBase<CreateOrUpdateCheckListCommand>,
												IValidator<CreateOrUpdateCheckListCommand>,
												ICommandHandlerBase<RestoreChecklistCommand>,
												ICommandHandlerBase<DeleteByIdCommand<CheckList>>,
												IValidator<DeleteByIdCommand<CheckList>>,
												ICommandHandlerAndValidator<DeleteByIdCommand<CheckListChapter>> {


		private readonly ITransientRepository<CheckListChapter> _chapterRepository;
		private readonly ITransientRepository<StandardUser> _standardUserRepository;
		private readonly ITransientRepository<CheckList> _repository;
		private readonly ITransientRepository<RestoreTrack> _restoreRepository;
		private readonly ITransientRepository<DocumentAuditTrack> _documentAuditRepository;
		private readonly ITransientRepository<Upload> _uploadRepository;
		private readonly IRepository<DocumentCategory> _documentCategoryRepository;
		private readonly ICommandDispatcher _dispatcher;
		private readonly IQueryExecutor _queryExecutor;
		private readonly IRepository<DocumentUrl> _documentUrlRepository;

		public CheckListCommandHandler(
			ITransientRepository<CheckList> repository,
			ITransientRepository<StandardUser> standardUserRepository,
			IQueryExecutor queryExecutor,
			ICommandDispatcher dispatcher,
			ITransientRepository<DocumentAuditTrack> documentAuditRepository,
			IRepository<DocumentCategory> documentCategoryRepository,
			ITransientRepository<CheckListChapter> chapterRepository,
			ITransientRepository<RestoreTrack> restoreRepository,
			ITransientRepository<Upload> uploadRepository,
			IRepository<DocumentUrl> documentUrlRepository
			) {
			_repository = repository;
			_standardUserRepository = standardUserRepository;
			_queryExecutor = queryExecutor;
			_dispatcher = dispatcher;
			_chapterRepository = chapterRepository;
			_uploadRepository = uploadRepository;
			_documentCategoryRepository = documentCategoryRepository;
			_restoreRepository = restoreRepository;
			_documentAuditRepository = documentAuditRepository;
			_documentUrlRepository = documentUrlRepository;
		}

		public CommandResponse Execute(RestoreChecklistCommand command) 
		{
			var e = _repository.Find(command.Id);
			var category = _documentCategoryRepository.List.FirstOrDefault(c => c.Title == "Default");
			e.Deleted = false;
			e.DocumentCategoryId = category.Id;
			e.Category = category;
			foreach (var c in e.Chapters) {
				c.Deleted = false;
				foreach (var u in c.Uploads)
					u.Deleted = false;
				foreach (var u in c.ContentToolsUploads)
					u.Deleted = false;
			}
			var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
			var standardUser = _standardUserRepository.Find(userId.ConvertToGuid());
			
			var restoreDoc = new RestoreTrack {
				Id = Guid.NewGuid(),
				UpdatedDate = DateTime.UtcNow,
				LastEditedBy = userId,
				DocumentId = command.Id,
				DocumentType = DocumentType.Checklist.ToString(),
				DocumentStatus="Restore"
			};
			if (standardUser != null) {
				
				restoreDoc.UserName = standardUser.LastName + " " + standardUser.FirstName;

			} else {
				restoreDoc.UserName = "Administrator";
			}
			_restoreRepository.Add(restoreDoc);
			_restoreRepository.SaveChanges();
			_repository.SaveChanges();

			return null;
		}

		public override CommandResponse Execute(CreateOrUpdateCheckListCommand command) 
		{
			var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
			var standardUser = _standardUserRepository.Find(userId.ConvertToGuid());
			var entry = _repository.Find(command.Id);
			var isNew = entry == null;

			if (isNew)
				entry = new CheckList {
					Id = command.Id,
					CreatedBy = userId,
					CreatedOn = DateTime.UtcNow,
					ReferenceId = _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
				};

			var collaborators = entry?.Collaborators ?? command.Collaborators?.ToList() ?? new List<StandardUser>();
			collaborators.RemoveAll(x => x == null); // command.Collaborators is populated with [null]
			if (standardUser != null && !collaborators.Contains(standardUser)) {
				collaborators.Add(standardUser);
			}
			DocumentCommandHandler.SyncCollaborators(entry, collaborators);

			var category = _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = command.Category?.Id });
			
			if (category != null) entry.DocumentCategoryId = category.Id;
				
			var coverPicture = _queryExecutor.Execute<SyncDocumentCoverPictureQuery, Upload>(new SyncDocumentCoverPictureQuery 
			{
				ExistingUploadId = entry.CoverPicture?.Id,
				ModelId = command.CoverPicture?.Id
			});

			entry.CoverPicture = coverPicture;
			entry.Description = command.Description;
			entry.DocumentStatus = command.DocumentStatus;
			entry.LastEditDate = DateTime.UtcNow;
			entry.LastEditedBy = userId;
			entry.Points = command.Points;
			entry.PreviewMode = command.PreviewMode;
			entry.Printable = command.Printable;
			entry.Title = command.Title;
			entry.NotificationInteval = command.NotificationInteval;
			entry.NotificationIntevalDaysBeforeExpiry = command.NotificationIntevalDaysBeforeExpiry;
			entry.CheckListExpiresNumberDaysFromAssignment = command.CheckListExpiresNumberDaysFromAssignment;
			entry.IsGlobalAccessed = command.IsGlobalAccessed;
			entry.IsChecklistTracked = command.IsChecklistTracked;
			entry.Approver = command.Approver;
			entry.ApproverId = command.PublishStatus == DocumentPublishWorkflowStatus.Approved ? Guid.Parse(userId) : command.PublishStatus == DocumentPublishWorkflowStatus.Declined ? Guid.Parse(userId) : Guid.Empty;
			entry.PublishStatus = command.PublishStatus;

			var values = string.IsNullOrWhiteSpace(command.TrainingLabels) ? new string[0] : command.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			
			var labels = _queryExecutor.Execute<SyncDocumentLabelsQuery, IEnumerable<Label>>(new SyncDocumentLabelsQuery 
			{
				Values = values,
				ExistingModelIds = string.IsNullOrEmpty(entry.TrainingLabels) ? Enumerable.Empty<string>() : entry.TrainingLabels.Split(',')
			}).ToArray();

			entry.TrainingLabels = labels.Any() ? string.Join(",", labels.Select(x => x.Name).ToArray()) : "";

			SyncChapters(command, entry);

			if (isNew)_repository.Add(entry);
			_repository.SaveChanges();

			var documentAudit = new DocumentAuditTrack 
			{
				Id = Guid.NewGuid(),
				LastEditDate = DateTime.UtcNow,
				LastEditedBy = userId,
				DocumentId = entry.Id,
				DocumentName = command.Title,
				DocumentType = DocumentType.Checklist.ToString(),
			};

			if (standardUser != null) 
			{
				documentAudit.User = standardUser;
				documentAudit.UserName = standardUser.LastName + " " + standardUser.FirstName;

			} 
			else 
			{
				documentAudit.UserName = "Administrator";
			}
			if (isNew) 
			{
				documentAudit.DocumentStatus = "Document Created";
			} 
			else 
			{
				documentAudit.DocumentStatus = "Document" + " " + (command.DocumentStatus == DocumentStatus.Draft ? "Edited" : command.DocumentStatus.ToString());
			}

			try 
			{
				var docId = command.Id;
				List<DocumentUrl> docList = _documentUrlRepository.GetAll().Where(x => x.DocumentId == docId).ToList();
				
				if (docList.Count > 0) 
				{
					foreach (var item in docList) {
						_documentUrlRepository.Delete(item);
					}
					_documentUrlRepository.SaveChanges();
				}

				foreach (var model in command.ContentModels) 
				{
					if (model.ChapterDocLinks != null && model.ChapterDocLinks != "" && model.ChapterDocNames != null && model.ChapterDocNames != "") 
					{
						string[] urls = model.ChapterDocLinks.split(",");
						string[] names = model.ChapterDocNames.split(",");
						var webFrame = new List<DocumentUrl>();

						if (urls != null && urls.Count() > 0 && names != null && names.Count() > 0) 
						{
							for (var i = 0; i < names.Length; i++) 
							{
								for (var j = 0; j < urls.Length; j++) 
								{
									if (i == j) 
									{
										var docUrl = new DocumentUrl() 
										{
											DocumentId = docId,
											Url = urls[j],
											ChapterId = model.Id,
											Name = names[i]
										};
										webFrame.Add(docUrl);
									}
								}
							}

							foreach (var link in webFrame) {
								_documentUrlRepository.Add(link);
							}
							_documentUrlRepository.SaveChanges();
						}
					}
				}
			}
			catch (Exception ex) {
				throw ex;
			}

			_documentAuditRepository.Add(documentAudit);
			_documentAuditRepository.SaveChanges();
			return null;
		}

		private void SyncChapters(CreateOrUpdateCheckListCommand command, CheckList entry) 
		{
			var existingIds = entry.Chapters.Where(c => !c.Deleted).Select(c => c.Id.ToString()).ToArray();
			var commandIds = command.ContentModels.Where(c => !c.Deleted).Select(x => x.Id).ToArray();
			var removedChapters = existingIds.Where(id => !commandIds.Contains(id));

			CreateOrUpdateChapters(entry, command.ContentModels.Where(c => !c.Deleted && !removedChapters.Contains(c.Id)).ToArray()).ToList().ForEach(e => entry.Chapters.Add(e));

			removedChapters.ForEach(id => {
				var e = _chapterRepository.Find(id);
				if (e != null) {
					e.Deleted = true;
					foreach (var u in e.ContentToolsUploads)
						u.Deleted = true;
					foreach (var u in e.Uploads)
						u.Deleted = true;
				}
			});

			entry.Chapters.ForEach(c => { c.Number = command.ContentModels.ToList().IndexOf(command.ContentModels.FirstOrDefault(x => x.Id == c.Id)) + 1; });

		}

		private IEnumerable<CheckListChapter> CreateOrUpdateChapters(CheckList entry, params CheckListChapterModel[] models) 
		{
			var chapters = new List<CheckListChapter>();

			models.ToList().ForEach(c => {
				var e = _chapterRepository.Find(c.Id);
				var isNew = e == null;

				if (e == null) e = new CheckListChapter { Id = c.Id.ToString(), CheckListId = entry.Id, CheckList = entry };
				e.Content = c.Content;
				e.Deleted = c.Deleted;
				e.Title = c.Title;
				e.AttachmentRequired = c.AttachmentRequired;
				e.IsChecked = c.IsChecked;
				e.NoteAllow = c.NoteAllow;
				e.CheckRequired = c.CheckRequired;
				e.IsSignOff = c.IsSignOff;

				_queryExecutor.Execute<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentUploadsQuery 
				{
					ExistingModelIds = e.Uploads.Select(x => x.Id).ToArray(),
					Models = c.Attachments.ToArray()
				}).ToList().ForEach(x => e.Uploads.Add(x));
				e.Uploads.ForEach(u => { u.Order = c.Attachments.ToList().IndexOf(c.Attachments.FirstOrDefault(x => x.Id == u.Id)) + 1; });

				_queryExecutor.Execute<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentToolsUploadsQuery {
					ExistingModelIds = e.ContentToolsUploads.Select(x => x.Id).ToArray(),
					Models = GetUploadUrls(c.Content)
				}).ToList().ForEach(x => e.ContentToolsUploads.Add(x));

				if (isNew)chapters.Add(e);
			});
			return chapters;
		}
		public IEnumerable<UploadFromContentToolsResultModel> GetUploadUrls(string html) 
		{
			if (string.IsNullOrWhiteSpace(html))
				return Enumerable.Empty<UploadFromContentToolsResultModel>();
			var imgTags = html.FindHTMLTags("img").Apply(tag => tag.FindHtmlAttr("src")).ToList();
			var divTags = html.FindHTMLTags("div", "ce-element--type-image").Apply(tag => tag.FindInlineCss("background-image")).RemoveEmpty().Select(x => x.SubstringAfter("url('").SubstringBefore("')")).ToList();
			return imgTags.Union(divTags).Select(x => new UploadFromContentToolsResultModel { url = x }).ToList();
		}
		public IEnumerable<IValidationResult> Validate(CreateOrUpdateCheckListCommand argument) 
		{
			if (string.IsNullOrWhiteSpace(argument.Title))
				yield return new ValidationResult(nameof(argument.Title), "Title is required");
			if (string.IsNullOrWhiteSpace(argument.Description))
				yield return new ValidationResult(nameof(argument.Description), "Description is required");
			if (argument.Category == null ||
				(argument.Category != null && string.IsNullOrWhiteSpace(argument.Category.Id)) ||
				(argument.Category != null && _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = argument.Category.Id }) == null))
				yield return new ValidationResult("Category.Id", "Category is required");

		}

		public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<CheckList> argument) 
		{
			if (_repository.Find(argument.Id) == null)
				yield return new ValidationResult("Id", $"Cannot find Check List with id : {argument.Id}");
		}

		public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<CheckListChapter> argument)
		{
			if (_chapterRepository.Find(argument.Id) == null)
				yield return new ValidationResult("Id", $"Cannot find Check List with id : {argument.Id}");
		}

		public CommandResponse Execute(DeleteByIdCommand<CheckList> command) 
		{
			var e = _repository.Find(command.Id);
			e.Deleted = true;
			e.LastEditDate = DateTime.UtcNow;
			e.DocumentCategoryId = null;
			e.Category = null;

			foreach (var c in e.Chapters) 
			{
				c.Deleted = true;
				foreach (var u in c.Uploads) u.Deleted = true;
				foreach (var u in c.ContentToolsUploads) u.Deleted = true;
			}

			var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
			var standardUser = _standardUserRepository.Find(userId.ConvertToGuid());
			
			var restoreDoc = new RestoreTrack 
			{
				Id = Guid.NewGuid(),
				UpdatedDate = DateTime.UtcNow,
				LastEditedBy = userId,
				DocumentId = command.Id,
				DocumentType = DocumentType.Checklist.ToString(),
				DocumentStatus = "Deleted"
			};

			if (standardUser != null) 
			{
				restoreDoc.UserName = standardUser.LastName + " " + standardUser.FirstName;
			} 
			else 
			{
				restoreDoc.UserName = "Administrator";
			}

			_restoreRepository.Add(restoreDoc);
			_restoreRepository.SaveChanges();
			_repository.SaveChanges();
			return null;
		}

		public CommandResponse Execute(DeleteByIdCommand<CheckListChapter> command)
		{
			var q = _chapterRepository.Find(command.Id);			
			q.Deleted = true;
			_chapterRepository.SaveChanges();			
			return null;
		}
	}

}
