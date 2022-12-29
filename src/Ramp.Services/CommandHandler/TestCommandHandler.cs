using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Test;
using Ramp.Contracts.Command.Test;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Common;
using Domain.Customer.Models.DocumentTrack;
using Domain.Customer;
using Domain.Customer.Models.Restore;
using Common.Data;
using Domain.Customer.Models.Document;
using ikvm.extensions;
using Domain.Enums;

namespace Ramp.Services.CommandHandler {
	public class TestCommandHandler : CommandHandlerBase<CreateOrUpdateTestCommand>,
        IValidator<CreateOrUpdateTestCommand>,
        ICommandHandlerBase<RestoreTestCommand>,
                                      ICommandHandlerAndValidator<DeleteByIdCommand<Test>>,
                                      ICommandHandlerAndValidator<DeleteByIdCommand<TestQuestion>>
    {
        private readonly ITransientRepository<Test> _repository;
        private readonly ITransientRepository<StandardUser> _standardUserRepository;
        private readonly ITransientRepository<TestQuestion> _questionRepository;
        private readonly ITransientRepository<TestQuestionAnswer> _answerRepository;
		private readonly ITransientRepository<RestoreTrack> _restoreRepository;
		private readonly ITransientRepository<Upload> _uploadRepository;
        private readonly IQueryExecutor _queryExecutor;
		private readonly IRepository<DocumentCategory> _documentCategoryRepository;
		private readonly ITransientRepository<DocumentAuditTrack> _documentAuditRepository;
		private readonly ICommandDispatcher _dispatcher;
		private readonly IRepository<DocumentUrl> _documentUrlRepository;

		public TestCommandHandler(
            ITransientRepository<Test> repository,
            ITransientRepository<StandardUser> standardUserRepository,
            IQueryExecutor queryExecutor,
            ICommandDispatcher dispatcher,
			IRepository<DocumentCategory> documentCategoryRepository,
			ITransientRepository<RestoreTrack> restoreRepository,
			ITransientRepository<DocumentAuditTrack> documentAuditRepository,
			ITransientRepository<TestQuestion> questionRepository,
            ITransientRepository<Upload> uploadRepository,
            ITransientRepository<TestQuestionAnswer> answerRepository,
			IRepository<DocumentUrl> documentUrlRepository)
        {
            _repository = repository;
			_documentCategoryRepository = documentCategoryRepository;
			_standardUserRepository = standardUserRepository;
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
            _queryExecutor = queryExecutor;
			_restoreRepository = restoreRepository;
			_dispatcher = dispatcher;
			_documentAuditRepository = documentAuditRepository;
			_uploadRepository = uploadRepository;
			_documentUrlRepository = documentUrlRepository;
		}
		public CommandResponse Execute(RestoreTestCommand command) {
			var e = _repository.Find(command.Id);
			var category = _documentCategoryRepository.List.FirstOrDefault(c => c.Title == "Default");
			e.Deleted = false;
			e.CategoryId = category.Id;
			e.Category = category;
			foreach (var q in e.Questions) {
				q.Deleted = false;
				foreach (var u in q.Uploads)
					u.Deleted = false;
				foreach (var a in q.Answers)
					a.Deleted = false;
			}

			var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
			var standardUser = _standardUserRepository.Find(userId.ConvertToGuid());
			var restoreDoc = new RestoreTrack {
				Id = Guid.NewGuid(),
				UpdatedDate = DateTime.UtcNow,
				LastEditedBy = userId,
				DocumentId = command.Id,
				DocumentType = DocumentType.Test.ToString(),
				DocumentStatus = "Restore"
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

		public override CommandResponse Execute(CreateOrUpdateTestCommand command)
        {
            var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            var standardUser = _standardUserRepository.Find(userId.ConvertToGuid());
            var entry = _repository.Find(command.Id);
            var isNew = entry == null;
            if (isNew)
                entry = new Test
                {
                    Id = command.Id,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    ReferenceId = _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                };

            var collaborators = entry?.Collaborators ?? command.Collaborators?.ToList() ?? new List<StandardUser>();
            collaborators.RemoveAll(x => x == null); // command.Collaborators is populated with [null]
            if (standardUser != null && !collaborators.Contains(standardUser))
            {
                collaborators.Add(standardUser);
            }
            DocumentCommandHandler.SyncCollaborators(entry, collaborators);

            var category = _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = command.Category?.Id });
            if (category != null)
                entry.CategoryId = category.Id;
            var coverPicture = _queryExecutor.Execute<SyncDocumentCoverPictureQuery, Upload>(new SyncDocumentCoverPictureQuery
            {
                ExistingUploadId = entry.CoverPicture?.Id,
                ModelId = command.CoverPicture?.Id
            });
            entry.CoverPicture = coverPicture;
            var certificate = _queryExecutor.Execute<FetchByIdQuery, Certificate>(new FetchByIdQuery { Id = command.Certificate?.Id });
            entry.Certificate = certificate;
            entry.CertificateId = certificate?.Id;
            entry.Description = command.Description;
            entry.DocumentStatus = command.DocumentStatus;
            entry.LastEditDate = DateTime.UtcNow;
            entry.Points = command.Points;
            entry.PreviewMode = command.PreviewMode;
            entry.Printable = command.Printable;
            entry.Title = command.Title;
            entry.PassMarks = command.PassMarks;
            entry.Duration = command.Duration ?? 0;
            entry.MaximumAttempts = command.MaximumAttempts;
            entry.IntroductionContent = command.IntroductionContent;
            entry.LastEditedBy = userId;
            entry.NotificationInteval = command.NotificationInteval;
            entry.NotificationIntevalDaysBeforeExpiry = command.NotificationIntevalDaysBeforeExpiry;
            entry.TestExpiresNumberDaysFromAssignment = command.TestExpiresNumberDaysFromAssignment;
            entry.AssignMarksToQuestions = command.AssignMarksToQuestions;
            entry.TestReview = command.TestReview;
            entry.RandomizeQuestions = command.RandomizeQuestions;
            entry.EmailSummary = command.EmailSummary;
            entry.HighlightAnswersOnSummary = command.HighlightAnswersOnSummary;
            entry.OpenTest = command.OpenTest;
            entry.EnableTimer = command.EnableTimer;
			entry.IsGlobalAccessed = command.IsGlobalAccessed;
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

            SyncQuestions(command, entry);
            if (isNew)
                _repository.Add(entry);
            _repository.SaveChanges();

			var documentAudit = new DocumentAuditTrack {
				Id = Guid.NewGuid(),
				LastEditDate = DateTime.UtcNow,
				LastEditedBy = userId,
				DocumentId = entry.Id,
				DocumentName = command.Title,
				DocumentType =DocumentType.Test.ToString(),
			};
			if (standardUser != null) {
				documentAudit.User = standardUser;
				documentAudit.UserName = standardUser.LastName + " " + standardUser.FirstName;

			} else {
				documentAudit.UserName = "Administrator";
			}
			if (isNew)
				documentAudit.DocumentStatus = "Document Created";
			else {
				documentAudit.DocumentStatus = "Document" + " " + (command.DocumentStatus == DocumentStatus.Draft ? "Edited" : command.DocumentStatus.ToString());
			}

			//try {
			//	var docId = command.Id;
			//	var docList = _documentUrlRepository.GetAll().Where(x => x.DocumentId == docId).ToList();
			//	if (docList.Count > 0) {
			//		foreach (var item in docList) {
			//			_documentUrlRepository.Delete(item);
			//		}
			//		_documentUrlRepository.SaveChanges();
			//	}
			//	if (command.DocumentUrl != null) {
			//		string[] urls = command.DocumentUrl.split(",");
			//		if (urls != null && urls.Count() > 0) {

			//			foreach (var url in urls) {
			//				var docUrl = new DocumentUrl() {
			//					DocumentId = docId,
			//					Url = url
			//				};
			//				_documentUrlRepository.Add(docUrl);
			//			}
			//			_documentUrlRepository.SaveChanges();
			//		}
			//	}
			//}
			//catch (Exception ex) {
			//	throw ex;
			//}
			_documentAuditRepository.Add(documentAudit);
			_documentAuditRepository.SaveChanges();
			return null;
        }
        private void SyncQuestions(CreateOrUpdateTestCommand command, Test entry)
        {
            var existingIds = entry.Questions.Where(c => !c.Deleted).Select(c => c.Id.ToString()).ToArray();
            var commandIds = command.ContentModels.Where(c => !c.Deleted).Select(x => x.Id).ToArray();
            var removedQuestions = existingIds.Where(id => !commandIds.Contains(id));

            CreateOrUpdateQuestions(entry, command.ContentModels.Where(c => !c.Deleted && !removedQuestions.Contains(c.Id)).ToArray()).ToList().ForEach(e => entry.Questions.Add(e));

            removedQuestions.ToList().ForEach(id =>
            {
                var e = _questionRepository.Find(id);
                if (e != null)
                {
                    e.Deleted = true;
                    foreach (var a in e.Answers)
                        a.Deleted = true;
                    foreach (var u in e.Uploads)
                        u.Deleted = true;
                }
            });
			
			 entry.Questions.ToList().ForEach(c => { c.Number = command.ContentModels.ToList().IndexOf(command.ContentModels.FirstOrDefault(x => x.Id == c.Id)) + 1; });
			
		}
		private IEnumerable<TestQuestion> CreateOrUpdateQuestions(Test entry, params TestQuestionModel[] models)
        {
            var questions = new List<TestQuestion>();
            models.ToList().ForEach(c =>
            {
                var e = _questionRepository.Find(c.Id);
                var isNew = e == null;
                if (e == null)
                    e = new TestQuestion { Id = c.Id.ToString(), TestId = entry.Id, Test = entry };
                e.Question = c.Question;
                e.Deleted = c.Deleted;
                e.AnswerWeightage = c.Marks;
                e.CorrectAnswerId = c.CorrectAnswerId;
                _queryExecutor.Execute<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentUploadsQuery
                {
                    ExistingModelIds = e.Uploads.Select(x => x.Id).ToArray(),
                    Models = c.Attachments.ToArray()
                }).ToList().ForEach(x => e.Uploads.Add(x));
                e.Uploads.ToList().ForEach(u => { u.Order = c.Attachments.ToList().IndexOf(c.Attachments.FirstOrDefault(x => x.Id == u.Id)) + 1; });
                
                _queryExecutor.Execute<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentToolsUploadsQuery
                {
                    ExistingModelIds = e.ContentToolsUploads.Select(x => x.Id).ToArray(),
                    Models = c.ContentToolsUploads.ToArray()
                }).ToList().ForEach(x => e.ContentToolsUploads.Add(x));

                SyncQuestionAnswers(e, c.Answers.ToArray());

                if (isNew)
                    questions.Add(e);
            });
            return questions;
        }
        private void SyncQuestionAnswers(TestQuestion entry,params TestQuestionAnswerModel[] models)
        {
            var existingIds = entry.Answers.Where(c => !c.Deleted).Select(c => c.Id.ToString()).ToArray();
            var commandIds = models.Where(c => !c.Deleted).Select(x => x.Id).ToArray();
            var removed = existingIds.Where(id => !commandIds.Contains(id));
            CreateOrUpdateQuestionAnswers(entry,models.Where(c => !c.Deleted && !removed.Contains(c.Id)).ToArray()).ToList().ForEach(e => entry.Answers.Add(e));

            removed.ToList().ForEach(id =>
            {
                var e = _answerRepository.Find(id);
                if (e != null)
                    e.Deleted = true;
            });
            entry.Answers.ToList().ForEach(c => { c.Number = models.ToList().IndexOf(models.FirstOrDefault(x => x.Id == c.Id)) + 1; });

        }
        private IEnumerable<TestQuestionAnswer> CreateOrUpdateQuestionAnswers(TestQuestion entry, params TestQuestionAnswerModel[] models)
        {
            var answers = new List<TestQuestionAnswer>();
            models.ToList().ForEach(c =>
            {
                var e = _answerRepository.Find(c.Id);
                var isNew = e == null;
                if (e == null)
                    e = new TestQuestionAnswer { Id = c.Id.ToString(), TestQuestionId = entry.Id, TestQuestion = entry };
                e.Option = c.Option;
                e.Deleted = c.Deleted;

                if (isNew)
                    answers.Add(e);
            });
            return answers;
        }

        public CommandResponse Execute(DeleteByIdCommand<Test> command)
        {
            var e = _repository.Find(command.Id);
            e.Deleted = true;
			e.LastEditDate = DateTime.UtcNow;
			e.CategoryId = null;
            e.Category = null;
            foreach(var q in e.Questions)
            {
                q.Deleted = true;
                foreach (var u in q.Uploads)
                    u.Deleted = true;
                foreach (var a in q.Answers)
                    a.Deleted = true;
            }
			var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
			var standardUser = _standardUserRepository.Find(userId.ConvertToGuid());
			var restoreDoc = new RestoreTrack {
				Id = Guid.NewGuid(),
				UpdatedDate = DateTime.UtcNow,
				LastEditedBy = userId,
				DocumentId = command.Id,
				DocumentType = DocumentType.Test.ToString(),
				DocumentStatus = "Deleted"
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

        public IEnumerable<IValidationResult> Validate(CreateOrUpdateTestCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Title))
                yield return new ValidationResult(nameof(command.Title), "Title is required");
            if (string.IsNullOrWhiteSpace(command.Description))
                yield return new ValidationResult(nameof(command.Description), "Description is required");
            if (command.EnableTimer && (!command.Duration.HasValue  || (command.Duration.HasValue && command.Duration.Value <= 0)))
                yield return new ValidationResult(nameof(command.Duration), "Test duration is required");
            if (command.MaximumAttempts < 1 || command.MaximumAttempts > 3)
                yield return new ValidationResult(nameof(command.MaximumAttempts),
                    "Maximum Attempts must be between 1 and 3");
            if (command.PassMarks <= 0 || command.PassMarks > 100)
                yield return new ValidationResult(nameof(command.PassMarks), "Pass Mark must be between 1 and 100");
            if (string.IsNullOrWhiteSpace(command.IntroductionContent))
                yield return new ValidationResult(nameof(command.IntroductionContent), "Introduction is required");
            if (command.Category == null ||
                (command.Category != null && string.IsNullOrWhiteSpace(command.Category.Id)) ||
                (command.Category != null && _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = command.Category.Id }) == null))
                yield return new ValidationResult("Category.Id", "Category is required");
        }

        public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<Test> command)
        {
            if (_repository.Find(command.Id) == null)
                yield return new ValidationResult("Id", $"Cannot find Test with id : {command.Id}");
        }
        public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<TestQuestion> command)
        {
            if (_questionRepository.Find(command.Id) == null)
                yield return new ValidationResult("Id", $"Cannot find TestQuestion with id : {command.Id}");
        }
        public CommandResponse Execute(DeleteByIdCommand<TestQuestion> command)
        {
            var q = _questionRepository.Find(command.Id);
            var a = _answerRepository.List.Where(z => z.TestQuestionId == command.Id).ToList();
            q.Deleted = true;

            foreach (var aa in a)
            {
                aa.Deleted = true;
                _answerRepository.SaveChanges();
            }
            //_questionRepository.SaveChanges();
            //_repository.SaveChanges();
            _questionRepository.SaveChanges(); /*done changes by softude*/
            _answerRepository.SaveChanges();
            return null;
        }
    }
}
