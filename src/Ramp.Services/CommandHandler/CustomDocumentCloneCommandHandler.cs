using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.TrainingManual;
using LinqKit;
using Ramp.Contracts.Command.TrainingManual;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Uploads;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Data;
using Domain.Customer.Models.Document;
using Ramp.Contracts.Command.CustomDocument;
using Domain.Customer.Models.DocumentTrack;
using Domain.Customer;
using Domain.Enums;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Policy;
using VirtuaCon;
using Ramp.Services.Helpers;
using Domain.Customer.Models.Forms;

namespace Ramp.Services.CommandHandler
{
    public class CustomDocumentCloneCommandHandler : ICommandHandlerAndValidator<CloneCommand<CustomDocument>>
    {
        private readonly IQueryExecutor _queryExecutor;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ITransientRepository<TrainingManual> _repository;
        private readonly ITransientRepository<DocumentCategory> _categoryRepository;
        private readonly ITransientRepository<Upload> _uploadRepository;
        private readonly ITransientRepository<DocumentUrl> _documentUrlRepository;


        private readonly ITransientRepository<CustomDocument> _customdocumentrepository;
        private readonly ITransientRepository<DocumentAuditTrack> _documentAuditRepository;
        private readonly IRepository<TrainingManual> _tmRepository;
        private readonly IRepository<Test> _testRepository;
        private readonly IRepository<CheckList> _clRepository;
        private readonly IRepository<Policy> _policyRepository;
        private readonly IRepository<Form> _formRepository;

        private readonly IRepository<TrainingManualChapter> _chapterRepositoryTM;
        private readonly IRepository<ConditionalTable> _conditionalTableRepository;
        private readonly IRepository<TestQuestion> _questionRepository;
        private readonly IRepository<TestQuestionAnswer> _answerRepository;
        private readonly IRepository<CheckListChapter> _chapterRepositoryCL;
        private readonly IRepository<PolicyContentBox> _contentRepositoryPolicy;
        private readonly IRepository<FormChapter> _chapterRepositoryFR;
        private readonly IRepository<FormField> _fieldRepositoryFR;

        public CustomDocumentCloneCommandHandler(IQueryExecutor queryExecutor,
            ITransientRepository<CustomDocument> customdocumentrepository,
            ITransientRepository<DocumentAuditTrack> documentAuditRepository,
            IRepository<TrainingManual> tmRepository,
            IRepository<Test> testRepository,
            IRepository<CheckList> clRepository,
            IRepository<Policy> policyRepository,
            IRepository<Form> formRepository,

            IRepository<TrainingManualChapter> chapterRepositoryTM,
            IRepository<ConditionalTable> conditionalTableRepository,
            IRepository<TestQuestion> questionRepository,
            IRepository<TestQuestionAnswer> answerRepository,
            IRepository<CheckListChapter> chapterRepositoryCL,
            IRepository<PolicyContentBox> contentRepositoryPolicy,
            IRepository<FormChapter> chapterRepositoryFR,
            IRepository<FormField> fieldRepositoryFR,

                                                 ICommandDispatcher commandDispatcher,
                                                 ITransientRepository<TrainingManual> repository,
                                                 ITransientRepository<DocumentCategory> categoryRepository,
                                                 ITransientRepository<DocumentUrl> documentUrlRepository,
                                                 ITransientRepository<Upload> uploadRepository)
        {
            _queryExecutor = queryExecutor;
            _commandDispatcher = commandDispatcher;
            _repository = repository;
            _categoryRepository = categoryRepository;
            _uploadRepository = uploadRepository;
            _documentUrlRepository = documentUrlRepository;

            _chapterRepositoryTM = chapterRepositoryTM;
            _conditionalTableRepository = conditionalTableRepository;
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
            _chapterRepositoryCL = chapterRepositoryCL;
            _contentRepositoryPolicy = contentRepositoryPolicy;
            _chapterRepositoryFR = chapterRepositoryFR;
            _fieldRepositoryFR = fieldRepositoryFR;

            _customdocumentrepository = customdocumentrepository;
            _documentAuditRepository = documentAuditRepository;
            _tmRepository = tmRepository;
            _testRepository = testRepository;
            _clRepository = clRepository;
            _policyRepository = policyRepository;
            _formRepository = formRepository;
        }
        public CommandResponse Execute(CloneCommand<CustomDocument> command)
        {          

            if (!string.IsNullOrWhiteSpace(command.SourceCompanyId))
                _customdocumentrepository.SetCustomerCompany(command.SourceCompanyId);

            var e = _customdocumentrepository.List.AsQueryable().FirstOrDefault(x => x.Id == command.Id.ToString() && !x.Deleted);


            var title = e.Title;
            if (command.SourceCompanyId == command.TargetCompanyId)
            {
                var matchText = e.Title.IndexOf("/") != -1 ? e.Title.Substring(0, e.Title.LastIndexOf("/")) : e.Title;
                var num = _queryExecutor.Execute<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery { MatchText = command.NewVersion ? matchText + "/V" : matchText + "/D" }).Count();
                e.Title = num == 0 && command.NewVersion ? $"{e.Title}/V{++num}" : e.Title;
                e.DocumentStatus = command.NewVersion ? Domain.Customer.DocumentStatus.Recalled : e.DocumentStatus;
                title = command.NewVersion ? $"{matchText}/V{++num}" : $"{matchText}/D{++num}";
            }
            _customdocumentrepository.SetCustomerCompany(command.TargetCompanyId);

            #region Custom Document Data

            var customdocentry = new CustomDocument
            {

                Id = Guid.NewGuid().ToString(),
                CreatedBy = e.CreatedBy,
                CreatedOn = DateTime.Now,
                ReferenceId =
                    _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
            };

            customdocentry.Title = title;

            var category = _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = e.Category?.Id });
            if (category != null)
                customdocentry.DocumentCategoryId = category.Id;

            var coverPicture = _queryExecutor.Execute<SyncDocumentCoverPictureQuery, Upload>(new SyncDocumentCoverPictureQuery
            {
                ExistingUploadId = e.CoverPicture?.Id,
                ModelId = e.CoverPicture?.Id
            });
            customdocentry.CoverPicture = coverPicture;
            customdocentry.CoverPictureId = coverPicture?.Id;

            var certificate = _queryExecutor.Execute<FetchByIdQuery, Certificate>(new FetchByIdQuery { Id = e.Certificate?.Id });
            customdocentry.Certificate = certificate;
            customdocentry.CertificateId = certificate?.Id;

            customdocentry.NotificationInteval = e.NotificationInteval;
            customdocentry.NotificationIntevalDaysBeforeExpiry = e.NotificationIntevalDaysBeforeExpiry;
            customdocentry.TestExpiresNumberDaysFromAssignment = e.TestExpiresNumberDaysFromAssignment;

            customdocentry.Description = e.Description;
            customdocentry.DocumentStatus = e.DocumentStatus;
            customdocentry.LastEditDate = e.CreatedOn;
            customdocentry.LastEditedBy = e.CreatedBy;
            customdocentry.Points = e.Points;
            customdocentry.PreviewMode = e.PreviewMode;
            customdocentry.Printable = e.Printable;
            customdocentry.IsGlobalAccessed = e.IsGlobalAccessed;
            customdocentry.IsMannualReview = e.IsMannualReview;
            customdocentry.IsResourceCentre = e.IsResourceCentre;
            var values = string.IsNullOrWhiteSpace(e.TrainingLabels) ? new string[0] : e.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var labels = _queryExecutor.Execute<SyncDocumentLabelsQuery, IEnumerable<Label>>(new SyncDocumentLabelsQuery
            {
                Values = values,
                ExistingModelIds = string.IsNullOrEmpty(customdocentry.TrainingLabels) ? Enumerable.Empty<string>() : customdocentry.TrainingLabels.Split(',')
            }).ToArray();
            customdocentry.TrainingLabels = labels.Any() ? string.Join(",", labels.Select(x => x.Name).ToArray()) : "";
            customdocentry.Approver = e.Approver;

            customdocentry.DocumentStatus = Domain.Customer.DocumentStatus.Draft;
            customdocentry.PublishStatus = 0;

            _customdocumentrepository.Add(customdocentry);
            _customdocumentrepository.SaveChanges();


            var documentAudit = new DocumentAuditTrack
            {
                Id = Guid.NewGuid(),
                LastEditDate = e.CreatedOn,
                LastEditedBy = e.CreatedBy,
                DocumentId = customdocentry.Id.ToString(),
                DocumentName = e.Title,
                DocumentType = DocumentType.TrainingManual.ToString(),
            };

            documentAudit.UserName = "Administrator";
            documentAudit.DocumentStatus = "Document Created";

            var docId = command.Id;
            List<DocumentUrl> docList = _documentUrlRepository.GetAll().Where(x => x.DocumentId.ToString() == docId.ToString()).ToList();
            if (docList.Count > 0)
            {
                foreach (var item in docList)
                {
                    _documentUrlRepository.Delete(item);
                }
                _documentUrlRepository.SaveChanges();
            }

            _documentAuditRepository.Add(documentAudit);
            _documentAuditRepository.SaveChanges();

            #endregion Custom Document Data

            #region	   Training Manual Data

            try
            {
                var existingentryTM = _tmRepository.List.Where(z => z.CustomDocummentId == Guid.Parse(command.Id.ToString())).FirstOrDefault();

                if (existingentryTM != null)
                {

                    var entryTM = new TrainingManual
                    {
                        Id = Guid.NewGuid().ToString(),
                        CreatedBy = existingentryTM.CreatedBy,
                        CreatedOn = DateTime.Now,
                        ReferenceId = _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                    };

                    var categoryTM = _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = existingentryTM.Category?.Id });
                    if (categoryTM != null) customdocentry.DocumentCategoryId = category.Id;

                    entryTM.CoverPicture = coverPicture;
                    entryTM.Description = existingentryTM.Description;
                    entryTM.DocumentStatus = customdocentry.DocumentStatus;
                    entryTM.LastEditDate = DateTime.Now;
                    entryTM.LastEditedBy = customdocentry.CreatedBy;
                    entryTM.Points = existingentryTM.Points;
                    entryTM.PreviewMode = existingentryTM.PreviewMode;
                    entryTM.Printable = existingentryTM.Printable;
                    entryTM.Title = existingentryTM.Title;
                    entryTM.IsGlobalAccessed = existingentryTM.IsGlobalAccessed;
                    var valuesTM = string.IsNullOrWhiteSpace(existingentryTM.TrainingLabels) ? new string[0] : existingentryTM.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    var labelsTM = _queryExecutor.Execute<SyncDocumentLabelsQuery, IEnumerable<Label>>(new SyncDocumentLabelsQuery
                    {
                        Values = valuesTM,
                        ExistingModelIds = string.IsNullOrEmpty(existingentryTM.TrainingLabels) ? Enumerable.Empty<string>() : existingentryTM.TrainingLabels.Split(',')
                    }).ToArray();

                    entryTM.TrainingLabels = labels.Any() ? string.Join(",", labels.Select(x => x.Name).ToArray()) : "";
                    entryTM.Approver = existingentryTM.Approver;
                    entryTM.ApproverId = existingentryTM.PublishStatus == DocumentPublishWorkflowStatus.Approved ? Guid.Parse(existingentryTM.CreatedBy) : customdocentry.PublishStatus == DocumentPublishWorkflowStatus.Declined ? Guid.Parse(customdocentry.CreatedBy) : Guid.Empty;
                    entryTM.PublishStatus = customdocentry.PublishStatus;
                    entryTM.IsCustomDocument = true;
                    entryTM.CustomDocummentId = Guid.Parse(customdocentry.Id.ToLower());

                    _tmRepository.Add(entryTM);
                    _tmRepository.SaveChanges();

                    #region    Training Manual Chapter

                    existingentryTM.Chapters.ToList().ForEach(c =>
                    {
                        //var _tmchapter = _chapterRepositoryTM.Find(c.Id);
                        var _tmchapter = new TrainingManualChapter
                        {
                            Id = Guid.NewGuid().ToString(),
                            Content = c.Content,
                            Title = c.Title,
                            IsAttached = c.IsAttached,
                            IsSignOff = c.IsSignOff,
                            NoteAllow = c.NoteAllow,
                            AttachmentRequired = c.AttachmentRequired,
                            CustomDocumentOrder = c.CustomDocumentOrder,
                            Deleted = c.Deleted,
                            Number = c.Number,
                            TrainingManualId = entryTM.Id.ToString(),
                            Uploads = c.Uploads
                           
                    };

                        //_tmchapter.TrainingManualId = entryTM.Id.ToString();

                        //_queryExecutor.Execute<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentUploadsQuery
                        //{
                        //    ExistingModelIds = _tmchapter.Uploads.Select(x => x.Id).ToArray(),
                        //    Models = c.Attachments.ToArray()
                        //}).ToList().ForEach(x => _tmchapter.Uploads.Add(x));
                        //_tmchapter.Uploads.ForEach(u => { u.Order = c.Attachments.ToList().IndexOf(c.Attachments.FirstOrDefault(x => x.Id == u.Id)) + 1; });


                        _queryExecutor.Execute<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentToolsUploadsQuery
                        {
                            ExistingModelIds = _tmchapter.ContentToolsUploads.Select(x => x.Id).ToArray(),
                            Models = GetUploadUrls(c.Content)
                        }).ToList().ForEach(x => _tmchapter.ContentToolsUploads.Add(x));


                        _chapterRepositoryTM.Add(_tmchapter);
                        _chapterRepositoryTM.SaveChanges();

                        ConditionalTable conditional = new ConditionalTable
                        {

                            ChapterID = Guid.Parse(_tmchapter.Id),
                            //TestAnswer = c.selectedTestAnswer,
                            //TestQuestion = c.selectedTestQuestion,
                            CustomDocumentID = existingentryTM.CustomDocummentId,
                            documentType = DocumentType.TrainingManual,
                            CreatedOn = DateTime.Now
                        };
                        addUpdateConditionalLogic(conditional);
                    });

                    #endregion Training Manual Chapter

                }

            }
            catch (Exception ex)
            {
                throw;
            }

            #endregion Training Manual Data

            #region	   Test data

            try
            {
                #region
                var existingentryTest = _testRepository.GetAll().Where(z => z.CustomDocummentId == Guid.Parse(command.Id.ToString())).FirstOrDefault();

                if (existingentryTest != null)
                {
                    var entryTest = new Test
                    {
                        Id = Guid.NewGuid().ToString(),
                        CreatedBy = existingentryTest.CreatedBy,
                        CreatedOn = DateTime.Now,
                        ReferenceId = _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                    };


                    var categoryTest = _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = existingentryTest.Category?.Id });
                    if (categoryTest != null)
                        entryTest.CategoryId = category.Id;

                    entryTest.Description = existingentryTest.Description;
                    entryTest.DocumentStatus = existingentryTest.DocumentStatus;
                    entryTest.LastEditDate = existingentryTest.CreatedOn;
                    entryTest.Points = existingentryTest.Points;
                    entryTest.PreviewMode = existingentryTest.PreviewMode;
                    entryTest.Printable = existingentryTest.Printable;
                    entryTest.Title = existingentryTest.Title;

                    entryTest.IsGlobalAccessed = existingentryTest.IsGlobalAccessed;
                    entryTest.Approver = existingentryTest.Approver;
                    entryTest.ApproverId = existingentryTest.PublishStatus == DocumentPublishWorkflowStatus.Approved ? Guid.Parse(existingentryTest.CreatedBy) : existingentryTest.PublishStatus == DocumentPublishWorkflowStatus.Declined ? Guid.Parse(existingentryTest.CreatedBy) : Guid.Empty;
                    entryTest.PublishStatus = (DocumentPublishWorkflowStatus)existingentryTest.PublishStatus;
                    entryTest.IsCustomDocument = true;
                    entryTest.CustomDocummentId = Guid.Parse(customdocentry.Id.ToLower());

                    _testRepository.Add(entryTest);
                    _testRepository.SaveChanges();


                    existingentryTest.Questions.ToList().ForEach(c =>
                    {
                        var quesrepo = new TestQuestion
                        {
                            Id = Guid.NewGuid().ToString(),
                            Question = c.Question,
                            Deleted = c.Deleted,
                            AnswerWeightage = c.AnswerWeightage,
                            CorrectAnswerId = c.CorrectAnswerId,
                            CheckRequired = c.CheckRequired,
                            Title = c.Title,
                            AttachmentRequired = c.AttachmentRequired,
                            NoteAllow = c.NoteAllow,
                            dynamicFields = c.dynamicFields,
                            IsSignOff = c.IsSignOff,
                            CustomDocumentOrder = c.CustomDocumentOrder,
                            Number = c.Number,
                            TestId = entryTest.Id.ToString(),
                            Uploads = c.Uploads
                        };

                        _questionRepository.Add(quesrepo);
                        _questionRepository.SaveChanges();

                        //_queryExecutor.Execute<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentUploadsQuery
                        //{
                        //    ExistingModelIds = e.Uploads.Select(x => x.Id).ToArray(),
                        //    Models = c.Attachments.ToArray()
                        //}).ToList().ForEach(x => e.Uploads.Add(x));
                        //e.Uploads.ToList().ForEach(u => { u.Order = c.Attachments.ToList().IndexOf(c.Attachments.FirstOrDefault(x => x.Id == u.Id)) + 1; });

                        //_queryExecutor.Execute<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentToolsUploadsQuery
                        //{
                        //    ExistingModelIds = quesrepo.ContentToolsUploads.Select(x => x.Id).ToArray(),
                        //    Models = c.ContentToolsUploads.ToArray()
                        //}).ToList().ForEach(x => e.ContentToolsUploads.Add(x));

                        c.Answers.ToList().ForEach(a =>
                        {
                            var ansrepo = new TestQuestionAnswer 
                            {
                                Id = Guid.NewGuid().ToString(),
                                Option = a.Option,
                                Deleted = a.Deleted,
                                Number = a.Number,
                                TestQuestionId = quesrepo.Id.ToString()
                            };

                            _answerRepository.Add(ansrepo);
                            _answerRepository.SaveChanges();
                        });


                        //below code added by softude

                        var existingconditionals = _conditionalTableRepository.List.Where(z => z.ChapterID == Guid.Parse(c.Id)).ToList();

                        if (existingconditionals.Count > 0) {

                            foreach (var item in existingconditionals)
                            {
                                ConditionalTable conditional = new ConditionalTable
                                {
                                    ChapterID = Guid.Parse(quesrepo.Id),
                                    TestAnswer = item.TestAnswer,
                                    TestQuestion = item.TestQuestion,
                                    CustomDocumentID = entryTest.CustomDocummentId,
                                    documentType = DocumentType.Test,
                                    CreatedOn = DateTime.Now
                                };
                                addUpdateConditionalLogic(conditional);
                            }
                        }
                    });

                }
                #endregion
            }
            catch (Exception ex)
            {
                throw;
            }

            #endregion Test data

            #region	   Activity Checkbox Data

            try
            {
                #region
                var existingentryCL = _clRepository.List.Where(z => z.CustomDocummentId == Guid.Parse(command.Id.ToString())).FirstOrDefault();

                if (existingentryCL != null)
                {

                    var entryCL = new CheckList
                    {
                        Id = Guid.NewGuid().ToString(),
                        CreatedBy = existingentryCL.CreatedBy,
                        CreatedOn = DateTime.Now,
                        ReferenceId =
                            _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                    };

                    var categoryCL = _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = existingentryCL.Category?.Id });
                    if (category != null)
                        entryCL.DocumentCategoryId = category.Id;


                    entryCL.Description = existingentryCL.Description;
                    entryCL.DocumentStatus = existingentryCL.DocumentStatus;
                    entryCL.LastEditDate = DateTime.Now;
                    entryCL.LastEditedBy = existingentryCL.CreatedBy;
                    entryCL.Points = existingentryCL.Points;
                    entryCL.PreviewMode = existingentryCL.PreviewMode;
                    entryCL.Printable = existingentryCL.Printable;
                    entryCL.Title = existingentryCL.Title;

                    entryCL.Approver = existingentryCL.Approver;
                    entryCL.ApproverId = existingentryCL.PublishStatus == DocumentPublishWorkflowStatus.Approved ? Guid.Parse(existingentryCL.CreatedBy) : existingentryCL.PublishStatus == DocumentPublishWorkflowStatus.Declined ? Guid.Parse(existingentryCL.CreatedBy) : Guid.Empty;
                    entryCL.PublishStatus = existingentryCL.PublishStatus;
                    entryCL.IsCustomDocument = true;
                    entryCL.CustomDocummentId = Guid.Parse(customdocentry.Id.ToLower());

                    _clRepository.Add(entryCL);
                    _clRepository.SaveChanges();


                    existingentryCL.Chapters.ToList().ForEach(b =>
                    {
                        var clchapter = new CheckListChapter 
                        {
                            Id = Guid.NewGuid().ToString(),
                            Content = b.Content,
                            Deleted = b.Deleted,
                            Title = b.Title,
                            AttachmentRequired = b.AttachmentRequired,
                            IsChecked = b.IsChecked,
                            NoteAllow = b.NoteAllow,
                            CheckRequired = b.CheckRequired,
                            IsSignOff = b.IsSignOff,
                            CustomDocumentOrder = b.CustomDocumentOrder,
                            IsConditionalLogic = b.IsConditionalLogic,
                            IsAttached = b.IsAttached,
                            Number = b.Number,
                            CheckListId = entryCL.Id.ToString(),
                            Uploads=b.Uploads
                        };

                        //_queryExecutor.Execute<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentUploadsQuery
                        //{
                        //    ExistingModelIds = e.Uploads.Select(x => x.Id).ToArray(),
                        //    Models = c.Attachments.ToArray()
                        //}).ToList().ForEach(x => e.Uploads.Add(x));
                        //e.Uploads.ForEach(u => { u.Order = c.Attachments.ToList().IndexOf(c.Attachments.FirstOrDefault(x => x.Id == u.Id)) + 1; });

                        //_queryExecutor.Execute<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentToolsUploadsQuery
                        //{
                        //    ExistingModelIds = e.ContentToolsUploads.Select(x => x.Id).ToArray(),
                        //    Models = GetUploadUrls(c.Content)
                        //}).ToList().ForEach(x => e.ContentToolsUploads.Add(x));

                        _chapterRepositoryCL.Add(clchapter);
                        _chapterRepositoryCL.SaveChanges();

                        var existingconditionals = _conditionalTableRepository.List.Where(z => z.ChapterID == Guid.Parse(b.Id)).ToList();

                        if (existingconditionals.Count > 0)
                        {

                            foreach (var item in existingconditionals)
                            {
                                ConditionalTable conditional = new ConditionalTable
                                {
                                    ChapterID = Guid.Parse(clchapter.Id),
                                    TestAnswer = item.TestAnswer,
                                    TestQuestion = item.TestQuestion,
                                    CustomDocumentID = entryCL.CustomDocummentId,
                                    documentType = DocumentType.Test,
                                    CreatedOn = DateTime.Now
                                };
                                addUpdateConditionalLogic(conditional);
                            }
                        }
                    });

                }
                #endregion
            }
            catch (Exception ex)
            {
                throw;
            }

            #endregion Activity Checkbox Data

            #region    Policy Data

            try
            {
                #region
                var existingentryPolicy = _policyRepository.List.Where(x => x.CustomDocummentId == Guid.Parse(command.Id.ToString())).FirstOrDefault();

                if (existingentryPolicy != null)
                {

                    var entryPolicy = new Policy
                    {
                        Id = Guid.NewGuid().ToString(),
                        CreatedBy = existingentryPolicy.CreatedBy,
                        CreatedOn = DateTime.Now,
                        ReferenceId = _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                    };

                    entryPolicy.Title = existingentryPolicy.Title;
                    entryPolicy.CallToAction = true;
                    entryPolicy.CallToActionMessage = existingentryPolicy.CallToActionMessage;
                    entryPolicy.IsGlobalAccessed = existingentryPolicy.IsGlobalAccessed;
                    entryPolicy.Approver = existingentryPolicy.Approver;
                    entryPolicy.ApproverId = existingentryPolicy.PublishStatus == DocumentPublishWorkflowStatus.Approved ? Guid.Parse(existingentryPolicy.CreatedBy) : existingentryPolicy.PublishStatus == DocumentPublishWorkflowStatus.Declined ? Guid.Parse(existingentryPolicy.CreatedBy) : Guid.Empty;
                    entryPolicy.PublishStatus = existingentryPolicy.PublishStatus;
                    entryPolicy.IsCustomDocument = true;
                    entryPolicy.CustomDocummentId = Guid.Parse(customdocentry.Id.ToLower());
                    entryPolicy.CustomDocumentOrder = existingentryPolicy.CustomDocumentOrder;

                    _policyRepository.Add(entryPolicy);
                    _policyRepository.SaveChanges();


                    existingentryPolicy.ContentBoxes.ToList().ForEach(d =>
                    {
                        var policycontentbox = new PolicyContentBox 
                        {
                            Id = Guid.NewGuid().ToString(),
                            Content = d.Content,
                            Deleted = d.Deleted,
                            Title = d.Title,
                            CustomDocumentOrder = d.CustomDocumentOrder,
                            IsAttached = d.IsAttached,
                            IsSignOff = d.IsSignOff,
                            NoteAllow = d.NoteAllow,
                            AttachmentRequired = d.AttachmentRequired,
                            IsConditionalLogic = d.IsConditionalLogic,
                            CheckRequired = d.CheckRequired,
                            Number = d.Number,
                            PolicyId = entryPolicy.Id.ToString(),
                            Uploads=d.Uploads
                        };


                        //_queryExecutor.Execute<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentUploadsQuery
                        //{
                        //    ExistingModelIds = e.Uploads.Select(x => x.Id).ToArray(),
                        //    Models = c.Attachments.ToArray()
                        //}).ToList().ForEach(x => e.Uploads.Add(x));
                        //e.Uploads.ToList().ForEach(u => { u.Order = c.Attachments.ToList().IndexOf(c.Attachments.FirstOrDefault(x => x.Id == u.Id)) + 1; });

                        //_queryExecutor.Execute<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentToolsUploadsQuery
                        //{
                        //    ExistingModelIds = e.ContentToolsUploads.Select(x => x.Id).ToArray(),
                        //    Models = GetUploadUrls(c.Content)
                        //}).ToList().ForEach(x => e.ContentToolsUploads.Add(x));

                        _contentRepositoryPolicy.Add(policycontentbox);
                        _contentRepositoryPolicy.SaveChanges();

                        ConditionalTable conditional = new ConditionalTable
                        {

                            ChapterID = Guid.Parse(policycontentbox.Id),
                            //TestAnswer = c.selectedTestAnswer,
                            //TestQuestion = c.selectedTestQuestion,
                            CustomDocumentID = existingentryPolicy.CustomDocummentId,
                            documentType = DocumentType.Policy,
                            CreatedOn = DateTime.Now
                        };
                        addUpdateConditionalLogic(conditional);
                    });

                }
                #endregion
            }
            catch (Exception ex)
            {
                throw;
            }

            #endregion Policy Data

            #region	Form Data

            try
            {
                #region
                var existingentryForm = _formRepository.GetAll().Where(z => z.CustomDocummentId == Guid.Parse(command.Id.ToString())).FirstOrDefault();

                if (existingentryForm != null)
                {
                    var entryForm = new Form
                    {
                        Id = Guid.NewGuid().ToString(),
                        CreatedBy = existingentryForm.CreatedBy,
                        CreatedOn = DateTime.Now,
                        ReferenceId = _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                    };


                    var categoryTest = _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = existingentryForm.Category?.Id });
                    if (categoryTest != null)
                        entryForm.CategoryId = category.Id;

                    entryForm.Description = existingentryForm.Description;
                    entryForm.DocumentStatus = existingentryForm.DocumentStatus;
                    entryForm.LastEditDate = existingentryForm.CreatedOn;
                    entryForm.Points = existingentryForm.Points;

                    entryForm.Printable = existingentryForm.Printable;
                    entryForm.Title = existingentryForm.Title;

                    entryForm.CustomDocummentId = Guid.Parse(customdocentry.Id.ToLower());

                    _formRepository.Add(entryForm);
                    _formRepository.SaveChanges();


                    existingentryForm.FormChapters.ToList().ForEach(c =>
                    {
                        var chapRepo = new FormChapter
                        {
                            Id = Guid.NewGuid().ToString(),
                            Content = c.Content.ToString(),
                            Deleted = c.Deleted,                           
                            CheckRequired = c.CheckRequired,
                            Title = c.Title,                           
                            CustomDocumentOrder = c.CustomDocumentOrder,
                            Number = c.Number,
                            FormId = entryForm.Id.ToString()
                        };

                        _chapterRepositoryFR.Add(chapRepo);
                        _chapterRepositoryFR.SaveChanges();

                        c.FormFields.ToList().ForEach(a =>
                        {
                            var fieldrepo = new FormField
                            {
                                Id = Guid.NewGuid().ToString(),
                                FormFieldName = a.FormFieldName,
                                FormChapterId = chapRepo.Id.ToString()
                            };

                            _fieldRepositoryFR.Add(fieldrepo);
                            _fieldRepositoryFR.SaveChanges();
                        });


                    });

                }
                #endregion
            }
            catch (Exception ex)
            {
                throw;
            }

            #endregion Test data

            command.Id = customdocentry.Id;
            _customdocumentrepository.SetCustomerCompany(command.SourceCompanyId);
            return null;
        }


        private void addUpdateConditionalLogic(ConditionalTable command)
        {
            try
            {
                if (command.TestAnswer != null && command.TestQuestion != null)
                {
                    var getConditionData = _conditionalTableRepository.GetAll().Where(z => z.ChapterID == command.ChapterID).FirstOrDefault();
                    var conditionData = getConditionData != null ? getConditionData : new ConditionalTable();
                    conditionData.ChapterID = command.ChapterID;
                    conditionData.CustomDocumentID = command.CustomDocumentID;
                    conditionData.documentType = command.documentType;
                    conditionData.TestAnswer = command.TestAnswer;
                    conditionData.TestQuestion = command.TestQuestion;
                    conditionData.CreatedOn = DateTime.Now;
                    if (getConditionData == null)
                    {
                        conditionData.Id = Guid.NewGuid().ToString();
                        _conditionalTableRepository.Add(conditionData);
                    }

                    _conditionalTableRepository.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<UploadFromContentToolsResultModel> GetUploadUrls(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return Enumerable.Empty<UploadFromContentToolsResultModel>();
            var imgTags = html.FindHTMLTags("img").Apply(tag => tag.FindHtmlAttr("src")).ToList();
            var divTags = html.FindHTMLTags("div", "ce-element--type-image").Apply(tag => tag.FindInlineCss("background-image")).RemoveEmpty().Select(x => x.SubstringAfter("url('").SubstringBefore("')")).ToList();
            return imgTags.Union(divTags).Select(x => new UploadFromContentToolsResultModel { url = x }).ToList();
        }


        private CreateOrUpdateCustomDocumentCommand Clone(CustomDocument e, IList<UploadContentCloneMapping> uploadCloneMappings, string title, string oldDocId = "")
        {

            var command = new CreateOrUpdateCustomDocumentCommand
            {
                Id = Guid.NewGuid().ToString(),
                DocumentStatus = Domain.Customer.DocumentStatus.Draft,
                Deleted = false,
                Description = e.Description,
                Points = e.Points,
                PreviewMode = e.PreviewMode,
                Printable = e.Printable,
                Title = title,
                TrainingLabels = e.TrainingLabels,
                IsGlobalAccessed = e.IsGlobalAccessed
            };
            //var docUrls = new List<DocumentUrlViewModel>();
            //if (oldDocId != "") {
            //	docUrls = _documentUrlRepository.GetAll().Where(x => x.DocumentId == oldDocId).ToList().Select(x => new DocumentUrlViewModel() {
            //		ChapterId = x.ChapterId,
            //		DocumentId = x.DocumentId,
            //		Url = x.Url
            //	}).ToList();

            //}

            var targetCategory = _categoryRepository.List.AsQueryable().First(x => x.Title == "Default");
            //var targetCategory = e.Category;
            command.Category = Project.Category_CategoryViewModelShort.Invoke(targetCategory);

            return command;

        }
        public IEnumerable<IValidationResult> Validate(CloneCommand<CustomDocument> command)
        {
            return Enumerable.Empty<IValidationResult>();
        }
    }
}
