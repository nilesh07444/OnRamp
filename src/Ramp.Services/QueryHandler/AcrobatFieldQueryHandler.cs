using Common.Command;
using Common.Query;
using Domain.Customer;
using LinqKit;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.AcrobatField;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ramp.Contracts.Query.DocumentCategory;
using Data.EF.Customer;
using Common.Data;
using Domain.Customer.Models.Document;
using Domain.Customer.Models;
using Ramp.Contracts.Query.Reporting;
using static Ramp.Contracts.ViewModel.AcrobatFieldDetailsViewModel;

namespace Ramp.Services.QueryHandler
{
    public class AcrobatFieldQueryHandler : IQueryHandler<FetchByIdQuery, AcrobatFieldModel>,
                                    IQueryHandler<AcrobatFieldListQuery, IEnumerable<AcrobatFieldListModel>>,
                                    IQueryHandler<AcrobatFieldListQuery, IEnumerable<DocumentListModel>>,
                                    IQueryHandler<RecycleAcrobatFieldQuery, IEnumerable<DocumentListModel>>,
                                    IQueryHandler<AcrobetFieldDetailsQuery, AcrobatFieldDetailsViewModel>,
                                    IQueryHandler<FetchTotalDocumentsQuery<AcrobatField>, int>

    {
        readonly ITransientReadRepository<AcrobatField> _repository;
        readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryExecutor _queryExecutor;
        private readonly IRepository<DocumentUrl> _documentUrlRepository;
        private readonly IRepository<Domain.Customer.Models.CustomDocument> _CustomDocumentRepository;
        private readonly ITransientRepository<DocumentUsage> _documentUsageRepository;
        private readonly ITransientRepository<StandardUserAdobeFieldValues> _acrobatFieldRepository;
        private readonly IRepository<StandardUser> _standardUser;


        public AcrobatFieldQueryHandler(
            ITransientRepository<DocumentUsage> documentUsageRepository,
            ITransientReadRepository<AcrobatField> repository, ICommandDispatcher commandDispatcher, IQueryExecutor queryExecutor,
            IRepository<DocumentUrl> documentUrlRepository,
            IRepository<StandardUser> standardUser, ITransientRepository<StandardUserAdobeFieldValues> acrobatFieldRepository

            )
        {
            _documentUsageRepository = documentUsageRepository;
            _repository = repository;
            _commandDispatcher = commandDispatcher;
            _queryExecutor = queryExecutor;
            _documentUrlRepository = documentUrlRepository;
            _standardUser = standardUser;
            _acrobatFieldRepository = acrobatFieldRepository;

        }


        public AcrobatFieldModel ExecuteQuery(FetchByIdQuery query)
        {
            var manual = _repository.Find(query.Id?.ToString());
            if (manual == null || (manual != null && manual.Deleted))
                return new AcrobatFieldModel { Id = Guid.NewGuid().ToString(), Category = new CategoryViewModelShort() };
            var model = Project.AcrobatField_AcrobatFieldModel.Invoke(manual);
            var docUrls = _documentUrlRepository.GetAll().Where(x => x.DocumentId == model.Id).ToList();
            //if (docUrls.Count > 0) {
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
                    .Where(x => x.DocumentId.ToString() == query.Id.ToString()).FirstOrDefault();

            //below code added by neeraj
            #region
            if (status != null)
            {

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
                    ;
                }
            }
            #endregion

            return model;
        }
        public IEnumerable<AcrobatFieldListModel> ExecuteQuery(AcrobatFieldListQuery query)
        {

            var models = Filter(query).Select(Project.AcrobatField_AcrobatFieldListModel).ToList();
            models.ForEach(model => _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model }));
            return models;
        }
        public IEnumerable<DocumentListModel> ExecuteQuery(RecycleAcrobatFieldQuery query)
        {
            var entries = _repository.List.AsQueryable().Where(x => x.Deleted);
            var statusCollection = query.DocumentStatus.String_DocumentStatus();
            if (statusCollection.Any())
                entries = entries.Where(x => statusCollection.Contains(x.DocumentStatus));
            var models = entries.OrderBy(x => x.Title).Select(Project.AcrobatField_DocumentListModel).ToList();
            return models;
        }


        public int ExecuteQuery(FetchTotalDocumentsQuery<AcrobatField> query)
        {
            return _repository.List.AsQueryable().Count();
        }

        IEnumerable<DocumentListModel> IQueryHandler<AcrobatFieldListQuery, IEnumerable<DocumentListModel>>.ExecuteQuery(AcrobatFieldListQuery query)
        {
            var models = Filter(query).Where(x => x.IsCustomDocument == null).Select(Project.AcrobatField_DocumentListModel).ToList();
            models.ForEach(model => _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = model }));

            var users = _standardUser.List.ToList();

            models.ForEach(x =>
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

            return models;
        }

        public IOrderedQueryable<AcrobatField> Filter(DocumentListQueryBase query)
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



        public AcrobatFieldDetailsViewModel ExecuteQuery(AcrobetFieldDetailsQuery query)
        {
            AcrobatFieldDetailsViewModel viewModel = new AcrobatFieldDetailsViewModel();

            var headData = _repository.List.Where(z => z.CustomDocummentId == Guid.Parse(query.CustomDocumentId)).FirstOrDefault();
            viewModel.GeneratedDate = headData.CreatedOn.Value.Date;
            viewModel.DocumentType = DocumentType.custom;
            viewModel.DocumentTitle = headData.Title;

            var AcroData = (from afr in _acrobatFieldRepository.List.Where(z => z.DocumentId == query.CustomDocumentId).ToList()
                            join su in _standardUser.List on afr.User_ID equals su.Id
                            select new AcrobatFieldDetailsModel
                            {
                                CustomDocumentID = afr.DocumentId,
                                FieldName = afr.Field_Name,
                                FieldValue = afr.Field_Value,
                                FirstName = su.FirstName,
                                LastName = su.LastName,
                                UserId = afr.User_ID,
                                ViewDate = afr.CreatedOn.Date
                            }).ToList();
            var fieldName = string.Join("|", AcroData.Select(c => c.FieldName).ToList());
            var fieldValues = string.Join("|", AcroData.Select(c => c.FieldValue).ToList());

            var tableHeaderData = AcroData.Select(z => new
            {
                CustomDocumentID = z.CustomDocumentID,
                FirstName = z.FirstName,
                LastName = z.LastName,
                UserId = z.UserId,
                ViewDate = z.ViewDate.Date,
                FieldName = fieldName,
                FieldValue = fieldValues
            }).Distinct().ToList();

            List<AcrobatFieldtableModel> listAcrobatFieldTable = new List<AcrobatFieldtableModel>();

            tableHeaderData.ForEach(z => listAcrobatFieldTable.Add(new AcrobatFieldtableModel
            {
                CustomDocumentID = z.CustomDocumentID,
                FirstName = z.FirstName,
                LastName = z.LastName,
                UserId = z.UserId,
                ViewDate = z.ViewDate.Date,
                FieldName = z.FieldName,
                FieldValue = z.FieldValue
            }));
            viewModel.AcrobatTableModel = listAcrobatFieldTable;

            return viewModel;
        }


    }
}
namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<AcrobatField, AcrobatFieldListModel>> AcrobatField_AcrobatFieldListModel =
            x => new AcrobatFieldListModel
            {
                Approver = x.Approver,
                ApproverId = x.ApproverId,
                PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
                CreatedBy = x.CreatedBy.ToString(),
                CreatedOn = x.CreatedOn,
                Deleted = x.Deleted,
                Description = x.Description,
                Id = x.Id.ToString(),
                IsGlobalAccessed = x.IsGlobalAccessed,
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
                Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)


            };
        public static readonly Expression<Func<AcrobatField, DocumentListModel>> AcrobatField_DocumentListModel =
            x => new DocumentListModel
            {
                Approver = x.Approver,
                ApproverId = x.ApproverId,
                PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn,
                Deleted = x.Deleted,
                IsGlobalAccessed = x.IsGlobalAccessed,
                Description = x.Description,
                DocumentType = DocumentType.AcrobatField,
                DocumentStatus = x.DocumentStatus,
                LastEditDate = x.LastEditDate,
                Points = x.Points,
                Printable = x.Printable,
                ReferenceId = x.ReferenceId,
                Title = x.Title,
                Id = x.Id.ToString(),
                CategoryId = x.CategoryId,
                CoverPictureId = x.CoverPictureId,
                LastEditedBy = x.LastEditedBy,
                TrainingLabels = string.IsNullOrEmpty(x.TrainingLabels) ? "none" : x.TrainingLabels,
                Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
            };
        public static readonly Expression<Func<AcrobatField, DocumentListModel>> AcrobatField_DocumentListModel_WithCategory =
            x => new DocumentListModel
            {
                Approver = x.Approver,
                ApproverId = x.ApproverId,
                PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
                CreatedBy = x.CreatedBy,
                IsGlobalAccessed = x.IsGlobalAccessed,
                CreatedOn = x.CreatedOn,
                Deleted = x.Deleted,
                Description = x.Description,
                DocumentType = DocumentType.AcrobatField,
                DocumentStatus = x.DocumentStatus,
                LastEditDate = x.LastEditDate,
                Points = x.Points,
                Printable = x.Printable,
                ReferenceId = x.ReferenceId,
                Title = x.Title,
                Id = x.Id.ToString(),
                CategoryId = x.CategoryId,
                Category = x.Category == null ? null : Category_CategoryViewModelShort.Compile().Invoke(x.Category),
                CoverPictureId = x.CoverPictureId,
                LastEditedBy = x.LastEditedBy,
                TrainingLabels = x.TrainingLabels,
                Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
            };
        public static readonly Expression<Func<AcrobatField, AcrobatFieldModel>> AcrobatField_AcrobatFieldModel =
          x => new AcrobatFieldModel
          {
              Approver = x.Approver,
              ApproverId = x.ApproverId,
              PublishStatus = x.DocumentStatus == 0 ? x.PublishStatus : Domain.Enums.DocumentPublishWorkflowStatus.Approved,
              CreatedBy = x.CreatedBy,
              IsGlobalAccessed = x.IsGlobalAccessed,
              CreatedOn = x.CreatedOn,
              LastEditedBy = x.LastEditedBy,
              Deleted = x.Deleted,
              Description = x.Description,
              Id = x.Id.ToString(),
              DocumentStatus = x.DocumentStatus,
              DocumentType = DocumentType.AcrobatField,
              LastEditDate = x.LastEditDate,
              Points = x.Points,
              PreviewMode = x.PreviewMode,
              Printable = x.Printable,
              ReferenceId = x.ReferenceId,
              Title = x.Title,
              TrainingLabels = x.TrainingLabels,
              CoverPicture = x.CoverPicture != null ? Upload_UploadResultViewModel.Invoke(x.CoverPicture) : null,
              CoverPictureId = x.CoverPictureId,
              Category = x.Category != null ? Category_CategoryViewModelShort.Invoke(x.Category) : null,
              CategoryId = x.CategoryId,
              ContentModels = x.ContentBoxes.AsQueryable().Where(c => !c.Deleted).OrderBy(c => c.Number).Select(AcrobatFieldContentBox_AcrobatFieldContentBoxModel).ToArray(),
              Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
          };
        public static readonly Expression<Func<AcrobatFieldContentBox, AcrobatFieldContentBoxModel>> AcrobatFieldContentBox_AcrobatFieldContentBoxModel =
          x => new AcrobatFieldContentBoxModel
          {
              Content = x.Content,
              Deleted = x.Deleted,
              Id = x.Id.ToString(),
              Number = x.Number,
              ParentId = x.AcrobatFieldId.ToString(),
              Title = x.Title,
              IsAttached = x.IsAttached,
              IsSignOff = x.IsSignOff,
              CustomDocumentOrder = x.CustomDocumentOrder,
              NoteAllow = x.NoteAllow,
              AttachmentRequired = x.AttachmentRequired,
              Attachments = x.Uploads.AsQueryable().Where(u => !u.Deleted).OrderBy(e => e.Order).Select(Upload_UploadResultViewModel).ToArray(),
              ContentToolsUploads = x.ContentToolsUploads.AsQueryable().Where(u => !u.Deleted).Select(Upload_UploadFromContentToolsResultModel).ToArray(),
          };


    }
}
