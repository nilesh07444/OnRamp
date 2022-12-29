using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Forms;
using Domain.Customer.Models.Groups;
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
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Ramp.Contracts.ViewModel.UserActivityAndPerformanceViewModel;

namespace Ramp.Services.QueryHandler.CustomDocument
{
    class CustomDocumentQueryHandler :
        IQueryHandler<FetchByIdQuery, CustomDocumentModel>,
        IQueryHandler<FetchByCustomIdQuery, CustomDocumentModel>,
        IQueryHandler<CustomDocumentListQuery, IEnumerable<DocumentListModel>>,
        IQueryHandler<CustomDocumentListQuery, IEnumerable<CustomDocumentListModel>>,
        IQueryHandler<FetchAllRecordsQuery, IEnumerable<DocumentListModel>>,
        IQueryHandler<FetchTotalDocumentsQuery<Domain.Customer.Models.CustomDocument>, int>,
        IQueryHandler<AllCustomDocumentListSubmissionReportQuery, IEnumerable<CustomDocumentCheckList>>,
        IQueryHandler<CustomDocumentSubmissionReportQuery, IEnumerable<CustomdocumentInteractionModel>>
    {
        private readonly ITransientReadRepository<Domain.Customer.Models.CustomDocument> _repository;

        private readonly ITransientReadRepository<Upload> _uploadRepository;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryExecutor _queryExecutor;
        private readonly IRepository<DocumentUrl> _CustdocumentUrlRepository;
        private readonly ITransientRepository<DocumentUsage> _documentUsageRepository;

        private readonly IRepository<StandardUser> _standardUser;
        private readonly IRepository<StandardUserGroup> _standardUserGroup;
        private readonly IRepository<CustomerGroup> _customerGroupRepository;
        private readonly IRepository<TestQuestion> _testQuestionRepository;
        private readonly IRepository<CustomDocumentMessageCenter> _CustomDocumentMessageCenterRepository;
        private readonly IRepository<CustomDocumentAnswerSubmission> _customDocumentAnswerSubmissionsrepository;


        private readonly ITransientReadRepository<Memo> _Memorepository;
        private readonly ITransientReadRepository<AcrobatField> _acrobatFieldrepository;
        private readonly ITransientReadRepository<Policy> _Policyrepository;
        private readonly ITransientReadRepository<TrainingManual> _Trainingrepository;
        private readonly ITransientReadRepository<Test> _TestRepository;
        private readonly ITransientRepository<StandardUserAdobeFieldValues> _standardAcrobatFieldRepository;
        private readonly ITransientReadRepository<ConditionalTable> _ConditionalRepository;
        private readonly ITransientReadRepository<CheckList> _CheckListrepository;
        private readonly IRepository<AssignedDocument> _assignedDocumentsRepository;

        private readonly IRepository<StandardUserGroup> _standardUserGroupRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        //added by softude
        private readonly ITransientReadRepository<Form> _FormRepository;
        private readonly ITransientReadRepository<FormFiledUserResult> _FormFieldUserResultRepository;


        public CustomDocumentQueryHandler(
            ITransientRepository<DocumentUsage> documentUsageRepository,
            ITransientReadRepository<Domain.Customer.Models.CustomDocument> repository, ITransientReadRepository<Upload> uploadRepository, IRepository<StandardUser> standardUser,

            ICommandDispatcher commandDispatcher, IQueryExecutor queryExecutor, IRepository<DocumentUrl> documentUrlRepository,
            ITransientReadRepository<Memo> Memorepository, ITransientReadRepository<Policy> Policyrepository,
            ITransientReadRepository<TrainingManual> Trainingrepository,
            ITransientReadRepository<Test> Testrepository,
            ITransientReadRepository<AcrobatField> acrobatFieldrepository,
            ITransientReadRepository<CheckList> checklistrepository,
            ITransientReadRepository<ConditionalTable> ConditionalRepository,
            IRepository<CustomDocumentAnswerSubmission> customDocumentAnswerSubmissionsrepository,
            IRepository<AssignedDocument> assignedDocumentsRepository,
            IRepository<StandardUserGroup> standardUserGroup, IRepository<CustomerGroup> customerGroupRepository,
            IRepository<TestQuestion> testQuestionRepository,
            IRepository<CustomDocumentMessageCenter> CustomDocumentMessageCenterRepository,
            ITransientRepository<StandardUserAdobeFieldValues> standardAcrobatFieldRepository,
            IRepository<StandardUserGroup> standardUserGroupRepository,
            IRepository<CustomerGroup> groupRepository,
            //added by softude
            ITransientReadRepository<Form> Formrepository,
             ITransientReadRepository<FormFiledUserResult> FormFieldUserResultRepository

            )
        {
            _CustomDocumentMessageCenterRepository = CustomDocumentMessageCenterRepository;
            _assignedDocumentsRepository = assignedDocumentsRepository;
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
            _customDocumentAnswerSubmissionsrepository = customDocumentAnswerSubmissionsrepository;
            _standardUserGroup = standardUserGroup;
            _testQuestionRepository = testQuestionRepository;
            _customerGroupRepository = customerGroupRepository;
            _standardAcrobatFieldRepository = standardAcrobatFieldRepository;
            _standardUserGroupRepository = standardUserGroupRepository;
            _groupRepository = groupRepository;
            //added by softude
            _FormRepository = Formrepository;
            _FormFieldUserResultRepository = FormFieldUserResultRepository;
        }

        CustomDocumentModel IQueryHandler<FetchByIdQuery, CustomDocumentModel>.ExecuteQuery(FetchByIdQuery query)
        {
            return getCustomDocData(new FetchByCustomIdQuery { Id = query.Id, userId = query.userId });
        }

        CustomDocumentModel IQueryHandler<FetchByCustomIdQuery, CustomDocumentModel>.ExecuteQuery(FetchByCustomIdQuery query)
        {
            return getCustomDocData(query);
        }

        private CustomDocumentModel getCustomDocData(FetchByCustomIdQuery query)
        {
            var userId = "";
            if (query.userId == null || query.userId == "undefined" || query.userId == "")
            {
                userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            }
            
            List<ContenTableData> contenTableData = new List<ContenTableData>();

            var CustomDocumentData = query.Id == null ? null : _repository.List.AsQueryable().FirstOrDefault(x => x.Id == query.Id.ToString() && !x.Deleted);

            var MemoManual = query.Id == null ? null : _Memorepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());

            var AcrobatFieldmanual = query.Id == null ? null : _acrobatFieldrepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());

            var Trainingmanual = query.Id == null ? null : _Trainingrepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());

            var PolicyData = query.Id == null ? null : _Policyrepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());

            var TestData = query.Id == null ? null : _TestRepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());

            var CheckListData = query.Id == null ? null : _CheckListrepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());

            var FormData = query.Id == null ? null : _FormRepository.List.AsQueryable().FirstOrDefault(x => x.CustomDocummentId.ToString() == query.Id.ToString());

            var AssignedDocument = query.Id == null && query.userId == null ? null : _assignedDocumentsRepository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == query.Id.ToString() && x.UserId == query.userId);

            if (query.Id != null)
            {
                MemoManual = _Memorepository.Find(query.Id.ToString());
            }

            if (CustomDocumentData == null || (CustomDocumentData != null && CustomDocumentData.Deleted))
            {
                var defaultDocument = new CustomDocumentModel { Id = Guid.NewGuid().ToString(), Category = new CategoryViewModelShort() };
                return defaultDocument;
            }

            var CustomDocumentmodel = Project.CustomDocument_CustomDocumentListModel.Compile().Invoke(CustomDocumentData);

            if(AssignedDocument != null)
            {
                CustomDocumentmodel.DeclineMessages = _CustomDocumentMessageCenterRepository.List.Where(z => z.AssignedDocumentId == AssignedDocument.Id).Select(c => new DeclineMessages { messages = c.Messages, CreatedOn = c.CreatedOn.ToString("MM/dd/yyyy hh: mm tt", CultureInfo.InvariantCulture), type = (byte)c.Status }).ToList();
                CustomDocumentmodel.AssignedDocumentId = AssignedDocument.Id;

                var customDocumentUsageData = _documentUsageRepository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == AssignedDocument.Id);

                if (customDocumentUsageData != null && customDocumentUsageData.Status != null)
                {
                    CustomDocumentmodel.Status = (AssignedDocumentStatus)customDocumentUsageData.Status;
                }
            }

            if (Trainingmanual != null)
            {
                var TrainingModels = Project.TrainingManual_TrainingManualModel.Compile().Invoke(Trainingmanual);

                List<TrainingManualChapterModel> TrainingContentModels = new List<TrainingManualChapterModel>();

                var docUrls = _CustdocumentUrlRepository.GetAll().Where(x => x.DocumentId == TrainingModels.Id).ToList();

                IEnumerable<TrainingManualChapterModel> trainingModels = new List<TrainingManualChapterModel>();

                if (TrainingModels.ContentModels != null)
                {
                    trainingModels = TrainingModels.ContentModels;
                }

                foreach (var ContentModel in trainingModels)
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

                    if (ContentModel.IsConditionalLogic)
                    {
                        var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(ContentModel.Id)).ToList();
                        if (conditionals.Count > 0)
                        {
                            ContentModel.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
                            ContentModel.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
                            ContentModel.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
                            ContentModel.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
                            //ContentModel.IsConditionalLogic = true;
                        }
                    }

                    ContentModel.DocLinkAndAttachmentCount = ContentModel.DocLinks.Count + ContentModel.Attachments.Count();
                    if (ContentModel.IsConditionalLogic == false)
                    {
                        ContenTableData contentTable = new ContenTableData();
                        contentTable.Id = ContentModel.Id;
                        contentTable.CustomDocumentOrder = ContentModel.CustomDocumentOrder;
                        contentTable.Title = ContentModel.Title;
                        contentTable.SectionData = ContentModel;
                        contenTableData.Add(contentTable);

                    }
                    TrainingContentModels.Add(ContentModel);
                }

                CustomDocumentmodel.TMContentModels = TrainingContentModels;
            }

            if (TestData != null)
            {
                var TestModels = Project.Test_TestModel.Compile().Invoke(TestData);
                List<TestQuestionModel> TestContentModels = new List<TestQuestionModel>();
                IEnumerable<TestQuestionModel> Models = new List<TestQuestionModel>();

                Models = TestModels.ContentModels;

                foreach (var ContentModel in Models)
                {
                    if (!string.IsNullOrEmpty(query.userId))
                    {
                        var CustomDocumentAnswerSubmissions = _customDocumentAnswerSubmissionsrepository.List.Where(z => z.CustomDocumentID == Guid.Parse(query.Id.ToString()) && z.TestQuestionID == Guid.Parse(ContentModel.Id) && z.StandarduserID == Guid.Parse(query.userId)).FirstOrDefault();
                        if (CustomDocumentAnswerSubmissions != null)
                        {
                            ContentModel.SelectedAnswer = CustomDocumentAnswerSubmissions.TestSelectedAnswer;
                            ContentModel.IsConditionalLogic = true;
                        }
                    }
                    var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(ContentModel.Id)).ToList();
                    if (conditionals.Count > 0)
                    {
                        ContentModel.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
                        ContentModel.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
                        ContentModel.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
                        ContentModel.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
                        ContentModel.IsConditionalLogic = true;
                    }
                    if (ContentModel.IsConditionalLogic == false)
                    {
                        ContenTableData contentTable = new ContenTableData();
                        contentTable.Id = ContentModel.Id;
                        contentTable.CustomDocumentOrder = ContentModel.CustomDocumentOrder;
                        contentTable.Title = ContentModel.Title;
                        contentTable.SectionData = ContentModel;
                        contenTableData.Add(contentTable);
                    }
                    TestContentModels.Add(ContentModel);



                    CustomDocumentmodel.TestContentModels = TestContentModels;
                }
            }

            if (MemoManual != null)
            {
                var MemoModels = Project.Memo_MemoModel.Compile().Invoke(MemoManual);
                List<MemoContentBoxModel> MemoContentModels = new List<MemoContentBoxModel>();
                var docUrls = _CustdocumentUrlRepository.GetAll().Where(x => x.DocumentId == MemoModels.Id).ToList();
                IEnumerable<MemoContentBoxModel> memoModels = new List<MemoContentBoxModel>();
                memoModels = MemoModels.ContentModels;

                foreach (var contentModel in memoModels)
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
                    if (contentModel.IsConditionalLogic)
                    {
                        var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(contentModel.Id)).ToList();
                        if (conditionals.Count > 0)
                        {
                            contentModel.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
                            contentModel.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
                            contentModel.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
                            contentModel.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
                            contentModel.IsConditionalLogic = true;
                        }
                    }

                    contentModel.DocLinkAndAttachmentCount = contentModel.DocLinks.Count + contentModel.Attachments.Count();
                    if (contentModel.IsConditionalLogic == false)
                    {
                        ContenTableData contentTable = new ContenTableData();
                        contentTable.Id = contentModel.Id;
                        contentTable.CustomDocumentOrder = contentModel.CustomDocumentOrder;
                        contentTable.Title = contentModel.Title;
                        contentTable.SectionData = contentModel;
                        contenTableData.Add(contentTable);
                    }
                    MemoContentModels.Add(contentModel);
                }

                CustomDocumentmodel.MemoContentModels = MemoContentModels;
            }

            if (CheckListData != null)
            {
                var checklistModels = Project.CheckList_CheckListModel.Compile().Invoke(CheckListData);
                List<CheckListChapterModel> checklistContentModels = new List<CheckListChapterModel>();
                var docUrls = _CustdocumentUrlRepository.GetAll().Where(x => x.DocumentId == checklistModels.Id).ToList();

                IEnumerable<CheckListChapterModel> checkModels = new List<CheckListChapterModel>();

                {
                    checkModels = checklistModels.ContentModels;
                }
                foreach (var ContentModel in checkModels)
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

                    if (ContentModel.IsConditionalLogic)
                    {
                        var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(ContentModel.Id)).ToList();
                        if (conditionals.Count > 0)
                        {
                            ContentModel.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
                            ContentModel.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
                            ContentModel.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
                            ContentModel.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
                            ContentModel.IsConditionalLogic = true;
                        }
                    }

                    ContentModel.DocLinkAndAttachmentCount = ContentModel.DocLinks.Count + ContentModel.Attachments.Count();


                    if (ContentModel.IsConditionalLogic == false)
                    {
                        ContenTableData contentTable = new ContenTableData();
                        contentTable.Id = ContentModel.Id;
                        contentTable.CustomDocumentOrder = ContentModel.CustomDocumentOrder;
                        contentTable.Title = ContentModel.Title;
                        contentTable.SectionData = ContentModel;
                        contenTableData.Add(contentTable);
                    }
                    checklistContentModels.Add(ContentModel);
                }
                CustomDocumentmodel.CLContentModels = checklistContentModels;
            }

            if (PolicyData != null && PolicyData.Title != null)
            {

                var PolicyModels = Project.Policy_PolicyModel.Compile().Invoke(PolicyData);

                List<PolicyContentBoxModel> CTA_PolicyContentModels = new List<PolicyContentBoxModel>();

                var docUrls = _CustdocumentUrlRepository.GetAll().Where(x => x.DocumentId == PolicyModels.Id).ToList();

                IEnumerable<PolicyContentBoxModel> policyboxModels = new List<PolicyContentBoxModel>();
                {
                    policyboxModels = PolicyModels.ContentModels;
                }

                foreach (var ContentModel in policyboxModels)
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

                    if (ContentModel.IsConditionalLogic)
                    {
                        var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(ContentModel.Id)).ToList();
                        if (conditionals.Count > 0)
                        {
                            ContentModel.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
                            ContentModel.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
                            ContentModel.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
                            ContentModel.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
                            //ContentModel.IsConditionalLogic = true;
                        }
                    }

                    ContentModel.DocLinkAndAttachmentCount = ContentModel.DocLinks.Count + ContentModel.Attachments.Count();
                    if (ContentModel.IsConditionalLogic == false)
                    {
                        ContenTableData contentTable = new ContenTableData();
                        contentTable.Id = ContentModel.Id;
                        contentTable.CustomDocumentOrder = ContentModel.CustomDocumentOrder;
                        contentTable.Title = ContentModel.Title;
                        contentTable.SectionData = ContentModel;
                        contenTableData.Add(contentTable);
                    }
                    CTA_PolicyContentModels.Add(ContentModel);

                }

                CustomDocumentmodel.PolicyContentModels = CTA_PolicyContentModels;
            }

            if (AcrobatFieldmanual != null)
            {
                var AcrobatFieldModels = Project.AcrobatField_AcrobatFieldModel.Compile().Invoke(AcrobatFieldmanual);
                List<AcrobatFieldContentBoxModel> AcrobatFieldContentModels = new List<AcrobatFieldContentBoxModel>();
                var docUrls = _CustdocumentUrlRepository.GetAll().Where(x => x.DocumentId == AcrobatFieldModels.Id).ToList();
                IEnumerable<AcrobatFieldContentBoxModel> acrobatModels = new List<AcrobatFieldContentBoxModel>();

                {
                    acrobatModels = AcrobatFieldModels.ContentModels;
                }

                foreach (var contentModel in acrobatModels)
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
                    var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(contentModel.Id)).ToList();
                    if (conditionals.Count > 0)
                    {
                        contentModel.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
                        contentModel.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
                        contentModel.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
                        contentModel.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
                        contentModel.IsConditionalLogic = true;
                    }
                    contentModel.AdobeFieldValues = _standardAcrobatFieldRepository.List.Where(z => z.DocumentId == contentModel.ParentId.ToString()).ToList();
                    contentModel.DocLinkAndAttachmentCount = contentModel.DocLinks.Count + contentModel.Attachments.Count();
                    AcrobatFieldContentModels.Add(contentModel);

                }

                CustomDocumentmodel.AcrobatFieldContentModels = AcrobatFieldContentModels;
            }

            if (FormData != null)
            {

                var PDF_FormModels = Project.Form_FormModel.Compile().Invoke(FormData);
                List<FormChapterModel> PDF_FormChapterModels = new List<FormChapterModel>();
                IEnumerable<FormChapterModel> Models = new List<FormChapterModel>();

                Models = PDF_FormModels.FormContentModels;

                foreach (var itemFormData in Models)
                {
                    foreach (var itemFormFieldData in itemFormData.FormFields)
                    {
                        var formFieldUserData = _FormFieldUserResultRepository.List.AsQueryable().FirstOrDefault(x => x.FormFieldId == itemFormFieldData.Id && x.FormChapterId == itemFormData.Id && (x.UserId == query.userId || x.UserId == userId));
                        if (formFieldUserData != null)
                        {
                            itemFormFieldData.FormFieldDescription = formFieldUserData.Value;
                        }
                    }


                }

                foreach (var ContentModel in Models)
                {
                    if (ContentModel.IsConditionalLogic)
                    {
                        var conditionals = _ConditionalRepository.List.Where(z => z.ChapterID == Guid.Parse(ContentModel.Id)).ToList();
                        if (conditionals.Count > 0)
                        {
                            ContentModel.TestAnswer = conditionals.Select(z => z.TestAnswer).ToList();
                            ContentModel.TestQuestion = conditionals.Select(z => z.TestQuestion).ToList();
                            ContentModel.selectedTestAnswer = conditionals.FirstOrDefault().TestAnswer;
                            ContentModel.selectedTestQuestion = conditionals.FirstOrDefault().TestQuestion;
                            //ContentModel.IsConditionalLogic = true;
                        }
                    }

                    if (ContentModel.IsConditionalLogic == false)
                    {
                        ContenTableData contentTable = new ContenTableData();
                        contentTable.Id = ContentModel.Id;
                        contentTable.CustomDocumentOrder = ContentModel.CustomDocumentOrder;
                        contentTable.Title = ContentModel.Title;
                        contentTable.SectionData = ContentModel;
                        contenTableData.Add(contentTable);
                    }
                    PDF_FormChapterModels.Add(ContentModel);
                }
                CustomDocumentmodel.FormContentModels = Models;
            }

            _commandDispatcher.Dispatch(new PostProcessDocumentListModelCommand { Model = CustomDocumentmodel });

            contenTableData = contenTableData.OrderBy(z => z.CustomDocumentOrder).ToList();

            CustomDocumentmodel.ContenTableDataList = contenTableData;            

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
            var customdoc = _repository.List.Where(x => x.Deleted != true && x.CreatedOn > DateTime.Parse("2022-01-28 13:11:24.340")).Select(x => new DocumentListModel
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

        public IEnumerable<CustomDocumentCheckList> ExecuteQuery(AllCustomDocumentListSubmissionReportQuery query)
        {
            var checkLists = _repository.List.AsQueryable().Where(x => !x.Deleted).ToList();
            var customDocumentCheckList = new List<CustomDocumentCheckList>();
            checkLists.ForEach(x =>
            customDocumentCheckList.Add(new CustomDocumentCheckList()
            {
                Title = x.Title,
                Description = x.Description,
                Id = x.Id,
                Status = x.DocumentStatus.ToString()
            }));
            return customDocumentCheckList;
        }

        private string groupTitles(List<StandardUserGroup> standardUserGroups, List<CustomerGroup> customerGroups, Guid userId)
        {
            var customGrp = (from sug in standardUserGroups
                             join cg in customerGroups on sug.GroupId equals cg.Id
                             where sug.UserId == userId
                             select cg.Title).ToList();
            var groups = string.Join(",", customGrp);

            return groups;
        }

        private string fetchTestQuestion(List<TestQuestion> testQuestions, Guid questionId)
        {
            var testQue = (from tq in testQuestions
                           where Guid.Parse(tq.Id) == questionId
                           select tq.Question).FirstOrDefault();

            return testQue;
        }

        public IEnumerable<CustomdocumentInteractionModel> ExecuteQuery(CustomDocumentSubmissionReportQuery query)
        {
            #region ["commented by Pawan 24-10-2022"]
            //var standardUser = _standardUser.List.AsQueryable().ToList();
            //var standardUserGroup = _standardUserGroup.List.AsQueryable().ToList();
            //var testQuestions = _testQuestionRepository.List.AsQueryable().ToList();
            //var CustomerGroups = _customerGroupRepository.List.AsQueryable().ToList();
            //var customDocuments = _repository.List.AsQueryable().Where(x => !x.Deleted && query.CustomDocumentIds.Contains(x.Id)).ToList();
            //var customDocumentAnswerSubmissions = _customDocumentAnswerSubmissionsrepository.List.AsQueryable().ToList();
            //var assignedDocuments = _assignedDocumentsRepository.List.AsQueryable().ToList();
            //var documentUsages = _documentUsageRepository.List.AsQueryable().ToList();
            //var documentMessages = _CustomDocumentMessageCenterRepository.List.AsQueryable().ToList();
            //var submissionRpt = (from cd in customDocuments
            //                     join cas in customDocumentAnswerSubmissions on Guid.Parse(cd.Id) equals cas.CustomDocumentID
            //                     join su in standardUser on cas.StandarduserID equals su.Id
            //                     join ad in assignedDocuments on cas.CustomDocumentID equals Guid.Parse(ad.DocumentId)
            //                     join du in documentUsages on
            //                     new { CustomDocumentID = cas.CustomDocumentID.ToString(), documentType = cas.documentType.ToString(), StandarduserID = cas.StandarduserID.ToString(), }
            //                     equals
            //                     new { CustomDocumentID = du.DocumentId, documentType = du.DocumentType.ToString(), StandarduserID = du.UserId }
            //                     //   where ad.AssignedDate.Date >= query.FromDate.Value.Date && ad.AssignedDate.Date <= query.ToDate.Value.Date
            //                     select (new CustomDocumentInteractionModel
            //                     {
            //                         DocumentTitle = cd.Title,
            //                         UserName = $"{su.FirstName} {su.LastName}",
            //                         TestSelectedAnswer = cas.TestSelectedAnswer,
            //                         TestQuestion = fetchTestQuestion(testQuestions, cas.TestQuestionID),
            //                         DateAssigned = ad.AssignedDate,
            //                         DateViewed = du.ViewDate,
            //                         CustomDocumentId = cas.CustomDocumentID.ToString(),
            //                         Status = documentUsages.Where(x => x.DocumentId == cd.Id && x.DocumentType != 0).FirstOrDefault().Status.ToString(),
            //                         Mesages = documentMessages.Where(x => x.DocumentId == cd.Id).ToList(),
            //                         Access = cd.IsGlobalAccessed.ToString(),
            //                         DocumentType = cas.documentType.ToString(),
            //                         StandarduserID = cas.StandarduserID.ToString(),
            //                         Group = groupTitles(standardUserGroup, CustomerGroups, cas.StandarduserID),
            //                         TestQuestionID = cas.TestQuestionID,
            //                         CustomDocumentID = cas.CustomDocumentID,
            //                         Viewed = du.ViewDate != DateTime.MinValue ? "Yes" : "No"

            //                     })).ToList();


            //if (query.Status != null && query.Status.Count() > 0)
            //{
            //    submissionRpt = submissionRpt.Where(z => query.Status.Contains(z.Status)).ToList();
            //}
            //if (query.Access != null && query.Access.Count() > 0)
            //{
            //    submissionRpt = submissionRpt.Where(z => query.Access.Contains(z.Access)).ToList();
            //}

            //var docUses = _documentUsageRepository.List.AsQueryable().OrderByDescending(p => p.ViewDate).Where(c => query.CustomDocumentIds.Contains(c.DocumentId)).ToList();
            //var model = new List<CustomDocumentInteractionModel>();
            //var result = new List<CustomDocumentInteractionModel>();
            //return submissionRpt; 
            #endregion


            var checkLists = _repository.List.AsQueryable().Where(x => !x.Deleted && query.CustomDocumentIds.Contains(x.Id)).ToList();

            var assignedDocs = _assignedDocumentsRepository.GetAll().Where(c => query.CustomDocumentIds.Contains(c.DocumentId) && !c.Deleted).ToList();

            var docUses = _documentUsageRepository.List.AsQueryable().OrderByDescending(p => p.ViewDate).Where(c => query.CustomDocumentIds.Contains(c.DocumentId) && c.AssignedDocumentId != null).ToList();
            var model = new List<CustomdocumentInteractionModel>();
            var result = new List<CustomDocumentInteractionModel>();

            if (assignedDocs != null && assignedDocs.Count > 0)
            {
                result = (from a in assignedDocs
                          join u in _standardUser.GetAll() on Guid.Parse(a.UserId) equals u.Id
                          join c in _repository.List.AsQueryable() on a.DocumentId equals c.Id                          
                          select new CustomDocumentInteractionModel
                          {
                              Id = a.DocumentId,
                              DocumentTitle = c.Title,
                              DateAssigned = a.AssignedDate,
                              UserName = u.FirstName + " " + u.LastName,
                              AssignedDocId = a.Id,
                              EmployeeCode = u.EmployeeNo,
                              IdNumber = u.IDNumber,
                              UserId = a.UserId,
                              IsGlobalAccess = (c.IsGlobalAccessed) ? true : false,
                          }

                        ).ToList();

                foreach (var item in result)
                {

                    var viewDocs = docUses.Where(c => c.UserId == item.UserId).ToList();

                    item.Viewed = (viewDocs != null && viewDocs.Any()) ? true : false;
                    item.DateViewed = (viewDocs != null && viewDocs.Any()) ? viewDocs.FirstOrDefault().ViewDate : DateTime.UtcNow;

                   
                    var ds = docUses.Where(x => x.DocumentId == item.Id && x.DocumentType != 0 && x.AssignedDocumentId == item.AssignedDocId).FirstOrDefault();
                    item.Status = ds != null ? ds.Status.ToString() : "Pending";

                    item.Access = (item != null && item.IsGlobalAccess) ? "Global" : "Assigned";

                    //var checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == item.AssignedDocId).ToList();
                    //var checkListChapter = _checkListChapterRepository.List.AsQueryable().Where(c => c.CheckListId == item.Id && !c.Deleted).ToList();

                    //item.ChecksCompleted = $"{checkListChapterUserResult.Where(c => c.IsChecked == true).Count()}/{checkListChapter.Count()}";

                }

                if (query.Status != null && query.Status.Any())
                {
                    result = result.Where(c => query.Status.Contains(c.Status)).ToList();
                }
                if (query.Access != null && query.Access.Any())
                {
                    result = result.Where(c => query.Access.Contains(c.Access)).ToList();
                }
                if (query.ToDate.HasValue && query.FromDate.HasValue)
                {
                    result = result.Where(c => c.DateAssigned.Date >= query.FromDate.Value.Date && c.DateAssigned.Date <= query.ToDate.Value.Date).ToList();
                }
                else if (query.ToDate.HasValue)
                {
                    result = result.Where(c => c.DateAssigned.Date <= query.ToDate.Value.Date).ToList();
                }
                else if (query.FromDate.HasValue)
                {
                    result = result.Where(c => c.DateAssigned.Date >= query.FromDate.Value.Date).ToList();
                }
                var check = result.GroupBy(c => c.DocumentTitle).Select(f => f.FirstOrDefault()).ToList();
               
            }

            #region ["commented by Pawan 24-10-2022"]
            //var checklist = checkLists.FirstOrDefault();
            //var checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.DocumentId == checklist.Id).ToList();

            //foreach (var item in checkListUserResult)
            //{
            //    var checklistresult = new CheckLisInteractionModel
            //    {
            //        Id = item.DocumentId,
            //        DocumentTitle = checklist.Title,
            //        UserId = item.UserId
            //    };

            //    var viewDocs = docUses.Where(c => c.UserId == item.UserId).ToList();
            //    checklistresult.Viewed = (viewDocs != null && viewDocs.Any()) ? true : false;
            //    checklistresult.DateViewed = (viewDocs != null && viewDocs.Any()) ? viewDocs.FirstOrDefault().ViewDate : DateTime.UtcNow;

            //    checklistresult.DateSubmitted = item.SubmittedDate;
            //    checklistresult.Completed = item.Status ? "Completed" : "InComplete";
            //    checklistresult.Status = item.Status ? "1" : "0";
            //    checklistresult.Access = item.IsGlobalAccessed ? "Global" : "Assigned";

            //    var checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().Where(c => c.DocumentId == item.DocumentId && c.IsGlobalAccessed && c.UserId == item.UserId).ToList();
            //    var checkListChapter = _checkListChapterRepository.List.AsQueryable().Where(c => c.CheckListId == item.DocumentId && !c.Deleted).ToList();
            //    checklistresult.ChecksCompleted = $"{checkListChapterUserResult.Where(c => c.IsChecked == true).Count()}/{checkListChapter.Count()}";
            //    var userId = Guid.Parse(item.UserId);
            //    var userDetail = _userRepository.List.Where(c => c.Id == userId).FirstOrDefault();

            //    if (userDetail != null)
            //    {
            //        checklistresult.UserName = userDetail.FirstName + " " + userDetail.LastName;
            //        checklistresult.EmployeeCode = userDetail.EmployeeNo;
            //        checklistresult.IdNumber = userDetail.IDNumber;
            //    }
            //    result.Add(checklistresult);
            //} 
            #endregion

            if (query.Status != null && query.Status.Any())
            {
                result = result.Where(c => query.Status.Contains(c.Status)).ToList();
            }

            if (query.Access != null && query.Access.Any())
            {
                result = result.Where(c => query.Access.Contains(c.Access)).ToList();
            }           

            foreach (var obj in checkLists)
            {
                var list = new List<CustomDocumentInteractionModel>();
                foreach (var item in result)
                {
                    if (obj.Id == item.Id)
                    {
                        var user = _standardUser.Find(Guid.Parse(item.UserId));
                        item.Group = user?.Group?.Title ?? "";
                        list.Add(item);
                    }
                }
                var m = new CustomdocumentInteractionModel
                {
                    DocumentTitle = obj.Title,
                    Id = obj.Id,
                    Checklist= list
                };
                model.Add(m);
            }

            //below code added by neeraj
            foreach (var u in model)
            {
                foreach (var rr in u.Checklist)
                {

                    var groups = _groupRepository.List;

                    var groupList = _standardUserGroupRepository.List.Where(c => c.UserId.ToString() == rr.UserId).ToList();

                    string name = null;
                    //List<string> name = new List<string>();
                    if (groupList.Count > 0)
                    {
                        foreach (var g in groupList)
                        {
                            foreach (var gl in groups)
                            {
                                if (gl.Id == g.GroupId)
                                {
                                    //name.Add(gl.Title);
                                    if (name != null)
                                        name = name + "," + gl.Title;
                                    else name = name + gl.Title;
                                }
                            }
                        }
                    }
                    if (name != null)
                    {
                        rr.Group = name;
                    }

                }

            }

            return model;

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
                IsMannualReview = x.IsMannualReview,
                IsResourceCentre = x.IsResourceCentre,
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
                CertificateId = x.CertificateId,
                Certificate = x.Certificate != null ? Certificate_UploadResultViewModel.Invoke(x.Certificate) : null,
                TestExpiresNumberDaysFromAssignment = x.TestExpiresNumberDaysFromAssignment,
                NotificationInteval = x.NotificationInteval,
                NotificationIntevalDaysBeforeExpiry = x.NotificationIntevalDaysBeforeExpiry,                
            };



        public static readonly Expression<Func<Form, FormModel>> Form_FormModel =
               x => new FormModel
               {

                   CreatedBy = x.CreatedBy,
                   CreatedOn = x.CreatedOn,
                   LastEditedBy = x.LastEditedBy,
                   Deleted = x.Deleted,
                   Description = x.Description,
                   Id = x.Id.ToString(),
                   LastEditDate = x.LastEditDate,
                   ReferenceId = x.ReferenceId,
                   Title = x.Title,
                   //DocumentType = DocumentType.Form,
                   FormContentModels = x.FormChapters.AsQueryable().Where(c => !c.Deleted).OrderBy(c => c.Number).Select(FormChapter_FormChapterModel).ToArray(),
               };


        public static readonly Expression<Func<FormChapter, FormChapterModel>> FormChapter_FormChapterModel =
                x => new FormChapterModel
                {
                    Deleted = x.Deleted,
                    Id = x.Id.ToString(),
                    Number = x.Number,
                    Title = x.Title,
                    CheckRequired = x.CheckRequired,
                    CustomDocumentOrder = x.CustomDocumentOrder,
                    Content = x.Content,
                    FormFieldTitle = x.FormFieldTitle,
                    FormFieldValue = x.FormFieldValue,
                    IsConditionalLogic = x.IsConditionalLogic,
                    FormFields = x.FormFields.AsQueryable().Where(e => !e.Deleted).OrderBy(e => e.Number).Select(FormField_FormFieldModel).ToArray()
                };


        public static readonly Expression<Func<FormField, FormFieldModel>> FormField_FormFieldModel =
            x => new FormFieldModel
            {
                Deleted = x.Deleted,
                Id = x.Id.ToString(),
                Number = x.Number,
                FormFieldName = x.FormFieldName,
                FormFieldDescription = x.FormFieldDescription
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
                DocumentType = DocumentType.custom,
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
               //Certificate = x.Certificate,
               //Certificate = x.Certificate != null ? Certificate_UploadResultViewModel.Invoke(x.Certificate) : null,
               LastEditedBy = x.LastEditedBy,
               TrainingLabels = x.TrainingLabels,
               //Certificate = x.Certificate,
               //added by softude
               Certificate = x.Certificate != null ? Certificate_UploadResultViewModel.Invoke(x.Certificate) : null,
               Collaborators = x.Collaborators.AsQueryable().Select(StandardUser_UserModelShort)
           };
    }
}
