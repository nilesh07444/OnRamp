using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using LinqKit;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Query.CustomDocument;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.DocumentCategory;
using Ramp.Contracts.Query.Policy;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.CustomDocument
{
    class CustomDocumentQueryHandler :
        IQueryHandler<FetchByIdQuery, CustomDocumentModel>,
        IQueryHandler<CustomDocumentListQuery, IEnumerable<DocumentListModel>>,
        IQueryHandler<CustomDocumentListQuery, IEnumerable<CustomDocumentListModel>>,
        IQueryHandler<FetchAllRecordsQuery, IEnumerable<DocumentListModel>>,
        IQueryHandler<FetchTotalDocumentsQuery<Domain.Customer.Models.CustomDocument>, int>
    //IQueryHandler<PolicyListQuery, IEnumerable<DocumentListModel>>
    {
        private readonly ITransientReadRepository<Domain.Customer.Models.CustomDocument> _repository;
        private readonly ITransientReadRepository<Upload> _uploadRepository;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryExecutor _queryExecutor;
        private readonly IRepository<DocumentUrl> _CustdocumentUrlRepository;
        private readonly ITransientRepository<DocumentUsage> _documentUsageRepository;

        private readonly IRepository<StandardUser> _standardUser;
        private readonly ITransientReadRepository<Memo> _Memorepository;
        private readonly ITransientReadRepository<AcrobatField> _acrobatFieldrepository;
       private readonly ITransientReadRepository<Policy> _Policyrepository;
        private readonly ITransientReadRepository<TrainingManual> _Trainingrepository;
        private readonly ITransientReadRepository<Test> _TestRepository;
        private readonly ITransientReadRepository<ConditionalTable> _ConditionalRepository;
        private readonly ITransientReadRepository<CheckList> _CheckListrepository;


        public CustomDocumentQueryHandler(
            ITransientRepository<DocumentUsage> documentUsageRepository,
            ITransientReadRepository<Domain.Customer.Models.CustomDocument> repository, ITransientReadRepository<Upload> uploadRepository, IRepository<StandardUser> standardUser,
            ICommandDispatcher commandDispatcher, IQueryExecutor queryExecutor, IRepository<DocumentUrl> documentUrlRepository, ITransientReadRepository<Memo> Memorepository, ITransientReadRepository<Policy> Policyrepository,
            ITransientReadRepository<TrainingManual> Trainingrepository,
            ITransientReadRepository<Test> Testrepository,
            ITransientReadRepository<AcrobatField> acrobatFieldrepository,
        ITransientReadRepository<CheckList> checklistrepository,
             ITransientReadRepository<ConditionalTable> ConditionalRepository
            )
        {
            _documentUsageRepository = documentUsageRepository;
            _repository = repository;
            _commandDispatcher = commandDispatcher;
            _queryExecutor = queryExecutor;
            _uploadRepository = uploadRepository;
            _CustdocumentUrlRepository = documentUrlRepository;
            _standardUser = standardUser;
            _Memorepository = Memorepository;
            _Policyrepository = Policyrepository;
            _Trainingrepository = Trainingrepository;
            _TestRepository = Testrepository;
            _CheckListrepository = checklistrepository;
            _ConditionalRepository = ConditionalRepository;
            _acrobatFieldrepository = acrobatFieldrepository;
        }
        CustomDocumentModel IQueryHandler<FetchByIdQuery, CustomDocumentModel>.ExecuteQuery(FetchByIdQuery query)
        {
            // Domain.Customer.Models.CustomDocument manual = null;
            var customDocumentData = query.Id == null ? null : _repository.List.AsQueryable().FirstOrDefault(x => x.Id == query.Id.ToString() && !x.Deleted);
            var Memomanual = query.Id == null ? null : _Memorepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());
             var AcrobatFieldmanual = query.Id == null ? null : _acrobatFieldrepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());
           var Trainingmanual = query.Id == null ? null : _Trainingrepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());
            var PolicyData = query.Id == null ? null : _Policyrepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());
            var TestData = query.Id == null ? null : _TestRepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());
            var checklistData = query.Id == null ? null : _CheckListrepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());


            if (query.Id != null)
            {
                Memomanual = _Memorepository.Find(query.Id.ToString());

            }
            if (customDocumentData == null || (customDocumentData != null && customDocumentData.Deleted))
            {
                var defaultDocument = new CustomDocumentModel { Id = Guid.NewGuid().ToString(), Category = new CategoryViewModelShort() };
                //List<TrainingManualChapterModel> TmDefault=new List<TrainingManualChapterModel>();
                //TmDefault.Add(new TrainingManualChapterModel { IsAttached = false, IsChecked = false, AttachmentRequired = false });
                //defaultDocument.TMContentModels = TmDefault;
                return defaultDocument;
            }
            var CustomDocumentmodel = Project.CustomDocument_CustomDocumentListModel.Compile().Invoke(customDocumentData);

            if (TestData != null)
            {
                var TestModels = Project.Test_TestModel.Compile().Invoke(TestData);
                List<TestQuestionModel> TestContentModels = new List<TestQuestionModel>();
                foreach (var ContentModel in TestModels.ContentModels)
                {
                    TestContentModels.Add(ContentModel);
                }
                CustomDocumentmodel.TestContentModels = TestContentModels;
            }

            if (Memomanual != null)
            {
                var MemoModels = Project.Memo_MemoModel.Compile().Invoke(Memomanual);
                List<MemoContentBoxModel> MemoContentModels = new List<MemoContentBoxModel>();
                var docUrls = _CustdocumentUrlRepository.GetAll().Where(x => x.DocumentId == MemoModels.Id).ToList();

                foreach (var contentModel in MemoModels.ContentModels)
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
                    var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(contentModel.ParentId)).ToList();
                    if (conditionals.Count > 0)
                    {
                        contentModel.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
                        contentModel.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
                        contentModel.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
                        contentModel.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
                    }
                    contentModel.DocLinkAndAttachmentCount = contentModel.DocLinks.Count + contentModel.Attachments.Count();
                    MemoContentModels.Add(contentModel);

                }

                CustomDocumentmodel.MemoContentModels = MemoContentModels;
            }

            if (Trainingmanual != null)
            {
                var TrainingModels = Project.TrainingManual_TrainingManualModel.Compile().Invoke(Trainingmanual);
                List<TrainingManualChapterModel> TrainingContentModels = new List<TrainingManualChapterModel>();
                var docUrls = _CustdocumentUrlRepository.GetAll().Where(x => x.DocumentId == TrainingModels.Id).ToList();

                foreach (var ContentModel in TrainingModels.ContentModels)
                {
                    ContentModel.DocLinks = new List<DocumentUrlViewModel>();
                    foreach (var url in docUrls)
                    {
                        if (ContentModel.Id == url.ChapterId)
                        {
                            var urlViewModel = new DocumentUrlViewModel()
                            {
                                DocumentId = url.DocumentId,
                                Id = url.Id,
                                Url = url.Url,
                                ChapterId = url.ChapterId,
                                Name = url.Name
                            };
                            ContentModel.DocLinks.Add(urlViewModel);
                        }
                    }
                    var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(ContentModel.ParentId)).ToList();
                   if (conditionals.Count > 0)
                    {  ContentModel.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
                    ContentModel.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
                   
                        ContentModel.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
                        ContentModel.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
                    }
                    ContentModel.DocLinkAndAttachmentCount = ContentModel.DocLinks.Count + ContentModel.Attachments.Count();

                    TrainingContentModels.Add(ContentModel);

                }
                CustomDocumentmodel.TMContentModels = TrainingContentModels;
            }

            if (PolicyData != null)
            {
                var PolicyModels = Project.Policy_PolicyModel.Compile().Invoke(PolicyData);
                //              List<PolicyContentBoxModel> policyContentModels = new List<PolicyContentBoxModel>();
                //                var docUrls = _CustdocumentUrlRepository.GetAll().Where(x => x.DocumentId == PolicyModels.Id).ToList();
                //foreach (var ContentModel in PolicyModels.ContentModels)
                //{
                //    ContentModel.DocLinks = new List<DocumentUrlViewModel>();
                //    foreach (var url in docUrls)
                //    {
                //        if (ContentModel.Id == url.ChapterId)
                //        {
                //            var urlViewModel = new DocumentUrlViewModel()
                //            {
                //                DocumentId = url.DocumentId,
                //                Id = url.Id,
                //                Url = url.Url,
                //                ChapterId = url.ChapterId,
                //                Name = url.Name
                //            };
                //            ContentModel.DocLinks.Add(urlViewModel);
                //        }
                //    }
                //    ContentModel.DocLinkAndAttachmentCount = ContentModel.DocLinks.Count + ContentModel.Attachments.Count();
                //    policyContentModels.Add(ContentModel);
                //}
                //CustomDocumentmodel.PolicyContentModels= policyContentModels;

                var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(PolicyModels.Id)).ToList();
                if (conditionals.Count > 0)
                {
                    PolicyModels.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
                    PolicyModels.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
                    PolicyModels.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
                    PolicyModels.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
                }
                CustomDocumentmodel.PolicyContent = PolicyModels;
            }

            if (checklistData != null)
            {
                var checklistModels = Project.CheckList_CheckListModel.Compile().Invoke(checklistData);
                List<CheckListChapterModel> checklistContentModels = new List<CheckListChapterModel>();
                var docUrls = _CustdocumentUrlRepository.GetAll().Where(x => x.DocumentId == checklistModels.Id).ToList();
                foreach (var ContentModel in checklistModels.ContentModels)
                {
                    ContentModel.DocLinks = new List<DocumentUrlViewModel>();
                    foreach (var url in docUrls)
                    {
                        if (ContentModel.Id == url.ChapterId)
                        {
                            var urlViewModel = new DocumentUrlViewModel()
                            {
                                DocumentId = url.DocumentId,
                                Id = url.Id,
                           
                                Url = url.Url,
                                ChapterId = url.ChapterId,
                                Name = url.Name
                            };
                            ContentModel.DocLinks.Add(urlViewModel);
                        }
                    }
                    var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(ContentModel.ParentId)).ToList();
                    if (conditionals.Count > 0)
                    {
                        ContentModel.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
                        ContentModel.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
                        ContentModel.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
                        ContentModel.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
                    }

                    ContentModel.DocLinkAndAttachmentCount = ContentModel.DocLinks.Count + ContentModel.Attachments.Count();
                    checklistContentModels.Add(ContentModel);
                }
                CustomDocumentmodel.CLContentModels = checklistContentModels;
            }

            if (AcrobatFieldmanual != null)
            {
                var AcrobatFieldModels = Project.AcrobatField_AcrobatFieldModel.Compile().Invoke(AcrobatFieldmanual);
                List<AcrobatFieldContentBoxModel> AcrobatFieldContentModels = new List<AcrobatFieldContentBoxModel>();
                var docUrls = _CustdocumentUrlRepository.GetAll().Where(x => x.DocumentId == AcrobatFieldModels.Id).ToList();

                foreach (var contentModel in AcrobatFieldModels.ContentModels)
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
                    var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(contentModel.ParentId)).ToList();
                    if (conditionals.Count > 0)
                    {
                        contentModel.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
                        contentModel.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
                        contentModel.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
                        contentModel.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
                    }
                    contentModel.DocLinkAndAttachmentCount = contentModel.DocLinks.Count + contentModel.Attachments.Count();
                    AcrobatFieldContentModels.Add(contentModel);

                }

                CustomDocumentmodel.AcrobatFieldContentModels = AcrobatFieldContentModels;
            }

            _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = CustomDocumentmodel });
            return CustomDocumentmodel;

        }

        public CustomDocumentListModel ExecuteQuery(FetchByIdQuery query)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CustomDocumentListModel> ExecuteQuery(CustomDocumentListQuery query)
        {
            throw new NotImplementedException();
        }

        public int ExecuteQuery(FetchTotalDocumentsQuery<Domain.Customer.Models.CustomDocument> query)
        {
            //throw new NotImplementedException();
            return _repository.List.AsQueryable().Count();
        }

        IEnumerable<DocumentListModel> IQueryHandler<FetchAllRecordsQuery, IEnumerable<DocumentListModel>>.ExecuteQuery(FetchAllRecordsQuery query)
        {
            var customdoc = _repository.List.Where(x => x.Deleted != true && x.CreatedOn > DateTime.Parse("2022 - 01 - 28 13:11:24.340")).Select(x => new DocumentListModel
            {
                Id = x.Id,
                ReferenceId = x.ReferenceId,
                Title = x.Title,
                Description = x.Description,
                DocumentStatus = x.DocumentStatus,
                CreatedOn = x.CreatedOn.Value,
                CreatedBy = x.CreatedBy,
                LastEditedBy = x.LastEditedBy,
                Printable = x.Printable,
                IsGlobalAccessed = x.IsGlobalAccessed,
                LastEditDate = x.LastEditDate,
                CoverPictureId = x.CoverPictureId,
                TrainingLabels = x.TrainingLabels,
                PublishStatus = x.PublishStatus,
                Approver = x.Approver,
                ApproverId = x.ApproverId



            }).ToList();



            //if (status != null)
            //{
            //	#region
            //	if (status.ViewDate == null || status.ViewDate != null && status.Status == null)
            //	{
            //		model.Status = AssignedDocumentStatus.Pending;
            //	}
            //	else if (status.ViewDate != null && (int)status.Status == (int)DocumentUsageStatus.Pending)
            //	{
            //		model.Status = AssignedDocumentStatus.Pending;
            //	}
            //	else if (status.ViewDate != null && (int)status.Status == (int)DocumentUsageStatus.Incomplete)
            //	{
            //		model.Status = AssignedDocumentStatus.Incomplete;
            //	}
            //	else if (status.ViewDate != null && (int)status.Status == (int)DocumentUsageStatus.Completed)
            //	{
            //		model.Status = AssignedDocumentStatus.Complete;
            //	}
            //	else if (status.ViewDate != null && (int)status.Status == (int)DocumentUsageStatus.Viewed)
            //	{
            //		model.Status = AssignedDocumentStatus.Viewed;

            //	}
            //	#endregion
            //}

            //return model;
            return customdoc;
        }

        IEnumerable<DocumentListModel> IQueryHandler<CustomDocumentListQuery, IEnumerable<DocumentListModel>>.ExecuteQuery(CustomDocumentListQuery query)
        {
            var models = Filter(query).Where(x => !x.Deleted).Select(Project.Custom_DocumentListModel).ToList();
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

        public IOrderedQueryable<Domain.Customer.Models.CustomDocument> Filter(DocumentListQueryBase query)
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

        public static readonly Expression<Func<CustomDocument, CustomDocumentModel>> CustomDocument_CustomDocumentListModel =
            x => new CustomDocumentModel
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
                Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort),
                //MemoContentModels = x.MemoContentModels.AsQueryable().Where(c => !c.Deleted).OrderBy(c => c.Number).Select(CustomDocument_DocumentListModel_WithCategory).ToArray()
            };

        public static readonly Expression<Func<CustomDocument, DocumentListModel>> Custom_DocumentListModel =
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
                //DocumentType = DocumentType.custom,
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

        public static readonly Expression<Func<CustomDocument, CustomDocumentListModel>> CustomDocument_DocumentListModel_WithCategory =
           x => new CustomDocumentListModel
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







    }
}
