using Common.Command;
using Common.Query;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.TrainingManual;
using LinqKit;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.TrainingManual;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ramp.Contracts.Query.DocumentCategory;
using Data.EF.Customer;
using Ramp.Contracts.Query.Upload;
using Ramp.Contracts.Query.RecycleBinQuery;
using Common.Data;
using Domain.Customer.Models.Document;

namespace Ramp.Services.QueryHandler
{
    public class TrainingManualQueryHandler : IQueryHandler<FetchByIdQuery, TrainingManualModel>,

                                                IQueryHandler<FetchByCategoryIdQuery, TrainingManualChartViewModel>,
                                              IQueryHandler<TrainingManualListQuery, IEnumerable<TrainingManualListModel>>,
                                              IQueryHandler<TrainingManualListQuery, IEnumerable<DocumentListModel>>,
                                                IQueryHandler<RecycleManualQuery, IEnumerable<DocumentListModel>>,
                                              IQueryHandler<FetchTotalDocumentsQuery<TrainingManual>, int>,
        IQueryHandler<FetchUploadByIdQuery, Upload>,IQueryHandler<FetchByCustomDocumentIdQuery, TrainingManual>
    {
        private readonly ITransientReadRepository<TrainingManual> _repository;
        private readonly ITransientReadRepository<Upload> _uploadRepository;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryExecutor _queryExecutor;
        private readonly IRepository<DocumentUrl> _documentUrlRepository;
        private readonly ITransientRepository<DocumentUsage> _documentUsageRepository;

        private readonly IRepository<StandardUser> _standardUser;

        public TrainingManualQueryHandler(
            ITransientRepository<DocumentUsage> documentUsageRepository,
            ITransientReadRepository<TrainingManual> repository, ITransientReadRepository<Upload> uploadRepository, IRepository<StandardUser> standardUser,
            ICommandDispatcher commandDispatcher, IQueryExecutor queryExecutor, IRepository<DocumentUrl> documentUrlRepository)
        {
            _documentUsageRepository = documentUsageRepository;
            _repository = repository;
            _commandDispatcher = commandDispatcher;
            _queryExecutor = queryExecutor;
            _uploadRepository = uploadRepository;
            _documentUrlRepository = documentUrlRepository;
            _standardUser = standardUser;
        }

        #region this code  added to get upload based on Id 
        public Upload ExecuteQuery(FetchUploadByIdQuery query)
        {
            if (query == null)
                return null;
            Upload result = new Upload();

            result = _uploadRepository.Find(query.Id);
            return result;
        }

        #endregion

        public TrainingManualChartViewModel ExecuteQuery(FetchByCategoryIdQuery query)
        {
            var id = Convert.ToString(query.Id);
            var manual = _repository.List.Where(c => c.DocumentCategoryId == id);
            var result = new TrainingManualChartViewModel
            {
                ManualCount = manual.Count()
            };
            return result;
        }

        public TrainingManualModel ExecuteQuery(FetchByIdQuery query)
        {
            var manual = _repository.Find(query.Id?.ToString());
            if (manual == null || (manual != null && manual.Deleted))
                return new TrainingManualModel { Id = Guid.NewGuid().ToString(), Category = new CategoryViewModelShort() };
            var model = Project.TrainingManual_TrainingManualModel.Compile().Invoke(manual);
            model.DocLinks = new List<DocumentUrlViewModel>();
            var docUrls = _documentUrlRepository.GetAll().Where(x => x.DocumentId == model.Id).ToList();
            //if(docUrls.Count > 0) {
            foreach (var contentModel in model.ContentModels)
            {
                contentModel.DocLinks = new List<DocumentUrlViewModel>();
                foreach (var url in docUrls)
                {
                    if (contentModel.Id == url.ChapterId)
                    {
                        var urlViewModel = new DocumentUrlViewModel()
                        {
                            DocumentId = url.DocumentId,
                            Id = url.Id,
                            Url = url.Url,
                            ChapterId = url.ChapterId,
                            Name = url.Name
                        };
                        contentModel.DocLinks.Add(urlViewModel);
                    }
                }
                contentModel.DocLinkAndAttachmentCount = contentModel.DocLinks.Count + contentModel.Attachments.Count();
            }
            //}
            _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model });

            var status = _documentUsageRepository.List
                    .Where(x => x.DocumentId.ToString() == query.Id.ToString()).OrderByDescending(r => r.ViewDate).FirstOrDefault(); ;

            if (status != null)
            {
                #region
                if (status.ViewDate == null || status.ViewDate != null && status.Status == null)
                {
                    model.Status = AssignedDocumentStatus.Pending;
                }
                else if (status.ViewDate != null && (int)status.Status == (int)DocumentUsageStatus.Pending)
                {
                    model.Status = AssignedDocumentStatus.Pending;
                }
                else if (status.ViewDate != null && (int)status.Status == (int)DocumentUsageStatus.UnderReview)
                {
                    model.Status = AssignedDocumentStatus.UnderReview;
                }
                else if (status.ViewDate != null && (int)status.Status == (int)DocumentUsageStatus.Complete)
                {
                    model.Status = AssignedDocumentStatus.Complete;
                }
                else if (status.ViewDate != null && (int)status.Status == (int)DocumentUsageStatus.InProgress)
                {
                    model.Status = AssignedDocumentStatus.InProgress;

                }
                #endregion
            }

            return model;
        }

        public IEnumerable<TrainingManualListModel> ExecuteQuery(TrainingManualListQuery query)
        {
            var entries = Filter(query).Select(Project.TrainingManual_TrainingManualListModel).ToList();
            entries.ForEach(model => _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model }));
            return entries;
        }

        public int ExecuteQuery(FetchTotalDocumentsQuery<TrainingManual> query)
        {
            return _repository.List.AsQueryable().Count();
        }
        public TrainingManual ExecuteQuery(FetchByCustomDocumentIdQuery query)
        {
            return _repository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());
        }
        public IEnumerable<DocumentListModel> ExecuteQuery(RecycleManualQuery query)
        {
            var entries = _repository.List.AsQueryable().Where(x => x.Deleted);
            var statusCollection = query.DocumentStatus.String_DocumentStatus();
            if (statusCollection.Any())
                entries = entries.Where(x => statusCollection.Contains(x.DocumentStatus));
            var models = entries.OrderBy(x => x.Title).Select(Project.TrainingManual_DocumentListModel).ToList();
            return models;
        }

        IEnumerable<DocumentListModel> IQueryHandler<TrainingManualListQuery, IEnumerable<DocumentListModel>>.ExecuteQuery(TrainingManualListQuery query)
        {
            var entries = Filter(query).Where(x => x.IsCustomDocument == null).Select(Project.TrainingManual_DocumentListModel).ToList().OrderByDescending(x => x.CreatedOn).ToList();
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

        public IOrderedQueryable<TrainingManual> Filter(DocumentListQueryBase query)
        {
            var entries = _repository.List.AsQueryable().Where(x => !x.Deleted);
            if (!string.IsNullOrWhiteSpace(query.CategoryId))
            {
                var categoryIds =
                    _queryExecutor.Execute<DocumentCategoryAndDescendantsIdQuery, IEnumerable<string>>(
                        new DocumentCategoryAndDescendantsIdQuery { CategoryId = query.CategoryId }).ToList();
                entries = entries.Where(x => categoryIds.Contains(x.DocumentCategoryId));
            }
            var statusCollection = query.DocumentStatus.String_DocumentStatus();
            if (statusCollection.Any())
                entries = entries.Where(x => statusCollection.Contains(x.DocumentStatus));
            return entries.OrderBy(x => x.Title);

        }
    }
}
namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<TrainingManual, TrainingManualListModel>> TrainingManual_TrainingManualListModel =
            x => new TrainingManualListModel
            {
                CreatedBy = x.CreatedBy.ToString(),
                CreatedOn = x.CreatedOn.Value,
                Deleted = x.Deleted,
                Description = x.Description,
                Id = x.Id.ToString(),
                DocumentStatus = x.DocumentStatus,
                IsGlobalAccessed = x.IsGlobalAccessed,
                Approver = x.Approver,
                ApproverId = x.ApproverId,
                PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
                LastEditDate = x.LastEditDate,
                Points = x.Points,
                Printable = x.Printable,
                ReferenceId = x.ReferenceId,
                Title = x.Title,
                CategoryId = x.DocumentCategoryId,
                CoverPictureId = x.CoverPictureId,
                LastEditedBy = x.LastEditedBy,
                TrainingLabels = x.TrainingLabels,
                Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
            };
        public static readonly Expression<Func<TrainingManual, DocumentListModel>> TrainingManual_DocumentListModel =
            x => new DocumentListModel
            {
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn.Value,
                Deleted = x.Deleted,
                Description = x.Description,
                IsGlobalAccessed = x.IsGlobalAccessed,
                Approver = x.Approver,
                ApproverId = x.ApproverId,
                PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
                DocumentType = DocumentType.TrainingManual,
                DocumentStatus = x.DocumentStatus,
                LastEditDate = x.LastEditDate,
                Points = x.Points,
                Printable = x.Printable,
                ReferenceId = x.ReferenceId,
                Title = x.Title,
                Id = x.Id.ToString(),
                CategoryId = x.DocumentCategoryId,
                CoverPictureId = x.CoverPictureId,
                LastEditedBy = x.LastEditedBy,
                TrainingLabels = string.IsNullOrEmpty(x.TrainingLabels) ? "none" : x.TrainingLabels,
                Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
            };
        public static readonly Expression<Func<TrainingManual, DocumentListModel>> TrainingManual_DocumentListModel_WithCategory =
           x => new DocumentListModel
           {
               CreatedBy = x.CreatedBy,
               CreatedOn = x.CreatedOn.Value,
               Deleted = x.Deleted,
               Description = x.Description,
               DocumentType = DocumentType.TrainingManual,
               DocumentStatus = x.DocumentStatus,
               LastEditDate = x.LastEditDate,
               IsGlobalAccessed = x.IsGlobalAccessed,
               Points = x.Points,
               Printable = x.Printable,
               ReferenceId = x.ReferenceId,
               Title = x.Title,
               Id = x.Id.ToString(),
               CategoryId = x.DocumentCategoryId,
               Category = x.Category == null ? null : Category_CategoryViewModelShort.Invoke(x.Category),
               CoverPictureId = x.CoverPictureId,
               LastEditedBy = x.LastEditedBy,
               TrainingLabels = x.TrainingLabels,
               Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
           };
        public static readonly Expression<Func<Categories, CategoryViewModelShort>> Categories_CategoryViewModelShort =
            x => new CategoryViewModelShort
            {
                Id = x.Id.ToString(),
                Name = x.Description
            };
        public static readonly Expression<Func<TrainingManual, TrainingManualModel>> TrainingManual_TrainingManualModel =
            x => new TrainingManualModel
            {
                Approver = x.Approver,
                ApproverId = x.ApproverId,
                PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn.Value,
                LastEditedBy = x.LastEditedBy,
                IsGlobalAccessed = x.IsGlobalAccessed,
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
                TrainingLabels = x.TrainingLabels,
                DocumentType = DocumentType.TrainingManual,
                CoverPicture = x.CoverPicture != null ? Upload_UploadResultViewModel.Invoke(x.CoverPicture) : null,
                CoverPictureId = x.CoverPictureId,
                Category = x.Category != null ? Category_CategoryViewModelShort.Invoke(x.Category) : null,
                CategoryId = x.DocumentCategoryId,
                ContentModels = x.Chapters.AsQueryable().Where(c => !c.Deleted).OrderBy(c => c.Number).Select(TrainingManualChapter_TrainingManualChapterModel).ToArray(),
                Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
            };


        public static readonly Expression<Func<DocumentCategory, CategoryViewModelShort>> Category_CategoryViewModelShort =
            x => new CategoryViewModelShort
            {
                Id = x.Id,
                Name = x.Title
            };

        public static readonly Expression<Func<Upload, UploadResultViewModel>> Upload_UploadResultViewModel =
            x => new UploadResultViewModel
            {
                Id = x.Id,
                Description = x.Description,
                Name = x.Name,
                Type = x.Type,
                Size = x.Data != null ? x.Data.Length : 0,
                Number = x.Order,
                Data=x.Data
            };

        public static readonly Expression<Func<TrainingManualChapter, TrainingManualChapterModel>> TrainingManualChapter_TrainingManualChapterModel =
            x => new TrainingManualChapterModel
            {
                Content = x.Content,
                Deleted = x.Deleted,
                Id = x.Id.ToString(),
                Number = x.Number,
                IsAttached = x.IsAttached,
                IsSignOff = x.IsSignOff,
                NoteAllow = x.NoteAllow,
                AttachmentRequired = x.AttachmentRequired,
                IsConditionalLogic = x.IsConditionalLogic,
                ParentId = x.TrainingManualId.ToString(),
                Title = x.Title,
                CustomDocumentOrder = x.CustomDocumentOrder,
                Attachments = x.Uploads.AsQueryable().Where(u => !u.Deleted).OrderBy(e => e.Order).Select(Upload_UploadResultViewModel).ToArray(),
                ContentToolsUploads = x.ContentToolsUploads.AsQueryable().Where(u => !u.Deleted).Select(Upload_UploadFromContentToolsResultModel).ToArray()
            };

        public static readonly Expression<Func<Upload, UploadFromContentToolsResultModel>> Upload_UploadFromContentToolsResultModel =
            x => new UploadFromContentToolsResultModel
            {
                url = x.Id
            };
    }
}
