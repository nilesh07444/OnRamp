using Common.Command;
using Common.Query;
using Domain.Customer;
using Domain.Customer.Models.Test;
using LinqKit;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ramp.Contracts.Query.DocumentCategory;
using Data.EF.Customer;
using Ramp.Contracts.Query.RecycleBinQuery;
using Domain.Customer.Models.Document;
using Common.Data;
using Domain.Customer.Models;

namespace Ramp.Services.QueryHandler {
	public class TestQueryHandler : IQueryHandler<FetchByIdQuery, TestModel>,IQueryHandler<FetchByCustomDocumentIdQuery, Test>,
                                    IQueryHandler<FetchByIdQuery<Test>,TestResultModel>,
                                    IQueryHandler<TestListQuery, IEnumerable<TestListModel>>,
									IQueryHandler<RecycleTestQuery, IEnumerable<DocumentListModel>>,
									IQueryHandler<TestListQuery, IEnumerable<DocumentListModel>>,
									IQueryHandler<FetchByCategoryIdQuery, TestChartViewModel>,
									IQueryHandler<FetchTotalDocumentsQuery<Test>, int>
    {
        
        readonly ITransientReadRepository<Test> _repository;
        
        readonly ICommandDispatcher _commandDispatcher;
        
        private readonly IQueryExecutor _queryExecutor;
		
        private readonly IRepository<DocumentUrl> _documentUrlRepository;

		private readonly IRepository<StandardUser> _standardUser;	

		public TestQueryHandler(ITransientReadRepository<Test> repository, ICommandDispatcher commandDispatcher, IQueryExecutor queryExecutor, IRepository<DocumentUrl> documentUrlRepository, IRepository<StandardUser> standardUser) {
            _repository = repository;
            _commandDispatcher = commandDispatcher;
            _queryExecutor = queryExecutor;
			_documentUrlRepository = documentUrlRepository;
            _standardUser = standardUser;
		}
        	
        public TestChartViewModel ExecuteQuery(FetchByCategoryIdQuery query) {
			var id = Convert.ToString(query.Id);
			var test = _repository.List.Where(c => c.CategoryId == id);
			var result = new TestChartViewModel() {
				TestCount= test.Count()
			};
			return result;
		}
        public Test ExecuteQuery(FetchByCustomDocumentIdQuery query)
        {
            return _repository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());
        }
        public TestModel ExecuteQuery(FetchByIdQuery query)
        {
            var test = _repository.Find(query.Id?.ToString());
            if (test == null || (test != null && test.Deleted))
                return new TestModel { Id = Guid.NewGuid().ToString(), Category = new CategoryViewModelShort() };
            var model = Project.Test_TestModel.Invoke(test);
			
			_commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model });
            return model;
        }
        
        public IEnumerable<DocumentListModel> ExecuteQuery(TestListQuery query)
        {
            var entries = Filter(query).Where(x => x.IsCustomDocument ==null).Select(Project.Test_DocumentListModel).ToList();
            entries.ForEach(model => _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model }));

			var users = _standardUser.List.ToList();

			entries.ForEach(x =>
			{
				if (x.Approver != null)
				{
					var userIds = x.Approver.Split(',').ToList();
					string names = null;
					foreach (var i in userIds)
					{
						foreach (var u in users)
						{
							if (u.Id.ToString() == i)
							{
								if (names == null)
								{
									names = u.FirstName + " " + u.LastName;
								}
								else if (names != null)
								{
									names = names + ", " + u.FirstName + " " + u.LastName;
								}
							}
						}
					}

					x.ApproversName = names;
				}
				else x.ApproversName = "none";
			});

			return entries;
        }
		
        public IEnumerable<DocumentListModel> ExecuteQuery(RecycleTestQuery query) {
			var entries = _repository.List.AsQueryable().Where(x => x.Deleted );
			var statusCollection = query.DocumentStatus.String_DocumentStatus();
			if (statusCollection.Any())
				entries = entries.Where(x => statusCollection.Contains(x.DocumentStatus));
			var models = entries.OrderBy(x => x.Title).Select(Project.Test_DocumentListModel);
			return models;
		}
		
        public int ExecuteQuery(FetchTotalDocumentsQuery<Test> query)
        {
            return _repository.List.AsQueryable().Count();
        }

        IEnumerable<TestListModel> IQueryHandler<TestListQuery, IEnumerable<TestListModel>>.ExecuteQuery(TestListQuery query)
        {
            var entries = Filter(query).Select(Project.Test_TestListModel).ToList();
            entries.ForEach(model => _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model }));
            return entries;

        }
        
        public IOrderedQueryable<Test> Filter(DocumentListQueryBase query)
        {
            var entries = _repository.List.AsQueryable().Where(x => !x.Deleted);
            if (!string.IsNullOrWhiteSpace(query.CategoryId))
            {
                var categoryIds =
                    _queryExecutor.Execute<DocumentCategoryAndDescendantsIdQuery, IEnumerable<string>>(
                        new DocumentCategoryAndDescendantsIdQuery { CategoryId = query.CategoryId }).ToList();
                entries = entries.Where(x => categoryIds.Contains(x.CategoryId));
            }
            var statusCollection = query.DocumentStatus.String_DocumentStatus();
            if (statusCollection.Any())
                entries = entries.Where(x => statusCollection.Contains(x.DocumentStatus));
            return entries.OrderBy(x => x.Title);

        }

        TestResultModel IQueryHandler<FetchByIdQuery<Test>, TestResultModel>.ExecuteQuery(FetchByIdQuery<Test> query)
        {
            var test = ExecuteQuery(new FetchByIdQuery { Id = query.Id });
            return Project.TestModel_TestResultModel.Invoke(test);
        }
    }
}
namespace Ramp.Services {
	public static partial class Extensions
    {
        public static IEnumerable<DocumentStatus> String_DocumentStatus(this IEnumerable<string> statusCollection)
        {
            var result = new List<DocumentStatus>();
            foreach (var s in statusCollection)
            {
                if (Enum.TryParse<DocumentStatus>(s, out var status))
                    if (!result.Contains(status))
                        result.Add(status);
            }
            return result;
        }
    }
}
namespace Ramp.Services.Projection {
	public static partial class Project
    {
        
        public static readonly Expression<Func<Test, TestListModel>> Test_TestListModel =
            x => new TestListModel
            {
				Approver = x.Approver,
				ApproverId = x.ApproverId,
				PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
				CreatedBy = x.CreatedBy.ToString(),
                CreatedOn = x.CreatedOn,
                Deleted = x.Deleted,
                Description = x.Description,
                Id = x.Id.ToString(),
                DocumentStatus = x.DocumentStatus,
                LastEditDate = x.LastEditDate,
                Points = x.Points,
                Printable = x.Printable,
                ReferenceId = x.ReferenceId,
                Title = x.Title,
                CategoryId = x.CategoryId,
                CoverPictureId = x.CoverPictureId,
                LastEditedBy = x.LastEditedBy,
                TrainingLabels = x.TrainingLabels,
				IsGlobalAccessed=x.IsGlobalAccessed,
				Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort).ToList()
            };
        public static readonly Expression<Func<Test, DocumentListModel>> Test_DocumentListModel =
            x => new DocumentListModel
            {
				Approver = x.Approver,
				ApproverId = x.ApproverId,
				PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
				CreatedBy = x.CreatedBy.ToString(),
                CreatedOn = x.CreatedOn.Value,
                Deleted = x.Deleted,
                Description = x.Description,
                Id = x.Id.ToString(),
                DocumentStatus = x.DocumentStatus,
                DocumentType = DocumentType.Test,
                LastEditDate = x.LastEditDate,
                Points = x.Points,
                Printable = x.Printable,
                ReferenceId = x.ReferenceId,
                Title = x.Title,
                CategoryId = x.CategoryId,
                CoverPictureId = x.CoverPictureId,
				TrainingLabels = string.IsNullOrEmpty(x.TrainingLabels) ? "none" : x.TrainingLabels,
				LastEditedBy = x.LastEditedBy,
				IsGlobalAccessed = x.IsGlobalAccessed,
                Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort).ToList()
            };
        public static readonly Expression<Func<Test, DocumentListModel>> Test_DocumentListModel_WithCategory =
           x => new DocumentListModel
           {
			   Approver = x.Approver,
			   ApproverId = x.ApproverId,
			   PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
			   CreatedBy = x.CreatedBy.ToString(),
               CreatedOn = x.CreatedOn,
               Deleted = x.Deleted,
               Description = x.Description,
               Id = x.Id.ToString(),
               DocumentStatus = x.DocumentStatus,
               DocumentType = DocumentType.Test,
               LastEditDate = x.LastEditDate,
               Points = x.Points,
               Printable = x.Printable,
               ReferenceId = x.ReferenceId,
               Title = x.Title,
               CategoryId = x.CategoryId,
               Category = x.Category == null ? null : Category_CategoryViewModelShort.Compile().Invoke(x.Category),
               CoverPictureId = x.CoverPictureId,
               LastEditedBy = x.LastEditedBy,
			   IsGlobalAccessed = x.IsGlobalAccessed,
               Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort).ToList(),
               TrainingLabels = x.TrainingLabels
           };
        public static readonly Expression<Func<Test, TestModel>> Test_TestModel =
           x => new TestModel
           {
			   Approver = x.Approver,
			   ApproverId = x.ApproverId,
			   PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
			   CreatedBy = x.CreatedBy,
               CreatedOn = x.CreatedOn,
               LastEditedBy = x.LastEditedBy,
               Deleted = x.Deleted,
               Description = x.Description,
               Id = x.Id.ToString(),
               DocumentStatus = x.DocumentStatus,
               LastEditDate = x.LastEditDate,
               Points = x.Points,
               PreviewMode = x.PreviewMode,
               Printable = x.Printable,
               ReferenceId = x.ReferenceId,
               Title = x.Title,
			   IsGlobalAccessed=x.IsGlobalAccessed,
               TrainingLabels = x.TrainingLabels,
               CoverPicture = x.CoverPicture != null ? Upload_UploadResultViewModel.Invoke(x.CoverPicture) : null,
               CoverPictureId = x.CoverPictureId,
               Category = x.Category != null ? Category_CategoryViewModelShort.Invoke(x.Category) : null,
               CategoryId = x.CategoryId,
               AssignMarksToQuestions = x.AssignMarksToQuestions,
               Duration = x.Duration,
               MaximumAttempts = x.MaximumAttempts,
               EmailSummary = x.EmailSummary,
               HighlightAnswersOnSummary = x.HighlightAnswersOnSummary,
               OpenTest = x.OpenTest,
               EnableTimer = x.EnableTimer,
               IntroductionContent = x.IntroductionContent,
               PassMarks = x.PassMarks,
               RandomizeQuestions = x.RandomizeQuestions,
               TestExpiresNumberDaysFromAssignment = x.TestExpiresNumberDaysFromAssignment,
               NotificationInteval = x.NotificationInteval,
               NotificationIntevalDaysBeforeExpiry = x.NotificationIntevalDaysBeforeExpiry,
               TestReview = x.TestReview,
               DocumentType = DocumentType.Test,
               Certificate = x.Certificate != null ? Certificate_UploadResultViewModel.Invoke(x.Certificate) : null,
               ContentModels = x.Questions.AsQueryable().Where(c => !c.Deleted).OrderBy(c => c.Number).Select(TestQuestion_TestQuestionModel).ToArray(),
               Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort).ToList()
           };
        public static readonly Expression<Func<TestQuestion, TestQuestionModel>> TestQuestion_TestQuestionModel =
            x => new TestQuestionModel
            {
                Marks = x.AnswerWeightage,
                Deleted = x.Deleted,
                Id = x.Id.ToString(),
                Number = x.Number,
                Question = x.Question,
                
                //added by softude
                Title = x.Title,

                CheckRequired =x.CheckRequired,
                AttachmentRequired=x.AttachmentRequired,
                dynamicFields=x.dynamicFields,
                IsSignOff=x.IsSignOff,
                NoteAllow=x.NoteAllow,
                CustomDocumentOrder = x.CustomDocumentOrder,
                CorrectAnswerId = x.CorrectAnswerId,
                Attachments = x.Uploads.AsQueryable().Where(e => !e.Deleted).OrderBy(e => e.Order).Select(Upload_UploadResultViewModel).ToArray(),
                ContentToolsUploads = x.ContentToolsUploads.AsQueryable().Where(e => !e.Deleted).Select(Upload_UploadFromContentToolsResultModel).ToArray(),
                Answers = x.Answers.AsQueryable().Where(e => !e.Deleted).OrderBy(e => e.Number).Select(TestQuestionAnswer_TestQuestionAnswerModel).ToArray()
            };
        public static readonly Expression<Func<TestQuestionAnswer, TestQuestionAnswerModel>> TestQuestionAnswer_TestQuestionAnswerModel = x => new TestQuestionAnswerModel
            {
                Deleted = x.Deleted,
                Id = x.Id.ToString(),
                Number = x.Number,
                Option = x.Option
            };
        public static readonly Expression<Func<TestModel, TestResultModel>> TestModel_TestResultModel =
            x => new TestResultModel
            {
				Approver = x.Approver,
				ApproverId = x.ApproverId,
				PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
				AssignMarksToQuestions = x.AssignMarksToQuestions,
                Category = x.Category,
				IsGlobalAccessed = x.IsGlobalAccessed,
				CategoryId = x.CategoryId,
                Certificate = x.Certificate,
                ContentModels = x.ContentModels.AsQueryable().Select(TestQuestionModel_TestQuestionResultModel).ToArray(),
                CoverPicture = x.CoverPicture,
                CoverPictureId = x.CoverPictureId,
                CoverPictureUpload = x.CoverPictureUpload,
                CreatedBy = x.CreatedBy,
                CreatedByModel = x.CreatedByModel,
                CreatedOn = x.CreatedOn,
                Deleted = x.Deleted,
                Description = x.Description,
                DocumentStatus = x.DocumentStatus,
                DocumentType = x.DocumentType,
                Duration = x.Duration,
                EmailSummary = x.EmailSummary,
                HighlightAnswersOnSummary = x.HighlightAnswersOnSummary,
                Id = x.Id,
                IntroductionContent = x.IntroductionContent,
                LastEditDate = x.LastEditDate,
                LastEditedBy = x.LastEditedBy,
                LastEditedByModel = x.LastEditedByModel,
                NotificationInteval = x.NotificationInteval,
                NotificationIntevalDaysBeforeExpiry = x.NotificationIntevalDaysBeforeExpiry,
                PassMarks = x.PassMarks,
                Points = x.Points,
                PreviewMode = x.PreviewMode,
                Printable = x.Printable,
                RandomizeQuestions = x.RandomizeQuestions,
                ReferenceId = x.ReferenceId,
                TestExpiresNumberDaysFromAssignment = x.TestExpiresNumberDaysFromAssignment,
                TestReview = x.TestReview,
                Title = x.Title,
                TrainingLabels = x.TrainingLabels,
                EnableTimer = x.EnableTimer
            };
        public static readonly Expression<Func<TestQuestionModel, TestQuestionResultModel>> TestQuestionModel_TestQuestionResultModel =
            x => new TestQuestionResultModel
            {
                Answers = x.Answers.AsQueryable().Select(TestQuestionAnswerModel_TestQuestionAnswerResultModel).ToArray(),
                Attachments = x.Attachments,
                ContentToolsUploads = x.ContentToolsUploads,
                CorrectAnswerId = x.CorrectAnswerId,
                Deleted = x.Deleted,
                Id = x.Id,
                Marks = x.Marks,
                Number = x.Number,
                Question = x.Question,
                Upload = x.Upload
            };
        public static readonly Expression<Func<TestQuestionAnswerModel, TestQuestionAnswerResultModel>> TestQuestionAnswerModel_TestQuestionAnswerResultModel =
            x => new TestQuestionAnswerResultModel
            {
                Deleted = x.Deleted,
                Id = x.Id,
                Number = x.Number,
                Option = x.Option,

            };
    }
}
