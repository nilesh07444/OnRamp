using Common;
using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.DocumentTrack;
using Domain.Customer.Models.Forms;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Domain.Enums;
using Ramp.Contracts.Command.TrainingManual;
using Ramp.Contracts.CommandParameter.CustomDocument;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtuaCon;

namespace Ramp.Services.CommandHandler.CustomDocuments
{
    public class AddOrUpdateCustomDocumentCommandHandler : CommandHandlerBase<AddOrUpdateCustomDocumentCommand>
    {
        private readonly IRepository<CustomDocument> _customDocumentRepository;
        private readonly IRepository<Memo> _memoRepository;
        private readonly IRepository<AcrobatField> _acrobatFieldRepository;
        private readonly IRepository<Test> _testRepository;
        private readonly IRepository<Policy> _policyRepository;
        private readonly IRepository<CheckList> _clRepository;
        private readonly IRepository<ConditionalTable> _conditionalTableRepository;
        private readonly IRepository<TrainingManual> _tmRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IQueryExecutor _queryExecutor;
        private readonly IRepository<DocumentUrl> _documentUrlRepository;
        private readonly ITransientRepository<DocumentAuditTrack> _documentAuditRepository;
        private readonly ITransientRepository<TrainingManualChapter> _chapterRepositoryTM;
        private readonly ITransientRepository<CheckListChapter> _chapterRepositoryCL;
        private readonly ITransientRepository<MemoContentBox> _contentRepository;
        private readonly ITransientRepository<AcrobatFieldContentBox> _acrobatFieldContentRepository;
        private readonly ITransientRepository<TestQuestion> _questionRepository;
        private readonly ITransientRepository<TestQuestionAnswer> _answerRepository;
        private readonly ITransientRepository<PolicyContentBox> _contentRepositoryPolicy;
        private readonly IRepository<Form> _formRepository;
        private readonly ITransientRepository<FormChapter> _formchapterRepository;
        private readonly ITransientRepository<FormField> _formfieldRepository;

        public AddOrUpdateCustomDocumentCommandHandler(
            IRepository<CustomDocument> customDocumentRepository,
            IRepository<Test> testRepository,
            IRepository<Policy> policyRepository,
            IRepository<CheckList> clRepository,
            IRepository<TrainingManual> tmRepository,
            IRepository<Memo> memoRepository,             
            IRepository<AcrobatField> acrobatFieldRepository,           
            IRepository<StandardUser> standardUserRepository,
            IRepository<ConditionalTable> conditionalTableRepository,        
            IQueryExecutor queryExecutor,
            IRepository<DocumentUrl> documentUrlRepository,
            ITransientRepository<DocumentAuditTrack> documentAuditRepository,
            ITransientRepository<TrainingManualChapter> chapterRepositoryTM,
            ITransientRepository<CheckListChapter> chapterRepositoryCL,
            ITransientRepository<MemoContentBox> contentRepository,
            ITransientRepository<AcrobatFieldContentBox> acrobatFieldContentRepository,
            ITransientRepository<TestQuestion> questionRepository,
            ITransientRepository<TestQuestionAnswer> answerRepository,
            ITransientRepository<PolicyContentBox> contentRepositoryPolicy,
            IRepository<Form> formRepository,
            ITransientRepository<FormChapter> formchapterRepository,
            ITransientRepository<FormField> formfieldRepository

            )
        {
            _customDocumentRepository = customDocumentRepository;
            _testRepository = testRepository;
            _policyRepository = policyRepository;
            _clRepository = clRepository;
            _tmRepository = tmRepository;
            _memoRepository = memoRepository;             
            _acrobatFieldRepository = acrobatFieldRepository;           
            _standardUserRepository = standardUserRepository;
            _queryExecutor = queryExecutor;
            _documentUrlRepository = documentUrlRepository;
            _documentAuditRepository = documentAuditRepository;
            _conditionalTableRepository = conditionalTableRepository;
            _chapterRepositoryTM = chapterRepositoryTM;
            _chapterRepositoryCL = chapterRepositoryCL;
            _contentRepository = contentRepository;
            _acrobatFieldContentRepository = acrobatFieldContentRepository;
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
            _contentRepositoryPolicy = contentRepositoryPolicy;
            _formRepository = formRepository;
            _formchapterRepository = formchapterRepository;
            _formfieldRepository = formfieldRepository;
        }

        private bool Modelvalidation(AddOrUpdateCustomDocumentCommand command)
        {
            if (string.IsNullOrEmpty(command.Description))
            {
                return false;
            }

            if (string.IsNullOrEmpty(command.Title))
            {
                return false;
            }
            return true;
        }

        public override CommandResponse Execute(AddOrUpdateCustomDocumentCommand command)
        {

            if (command.TMContentModels.Count() != 0)
                command.TrainingMannual = true;
            if (command.CLContentModels.Count() != 0)
                command.CheckList = true;
            if (command.MemoContentModels.Count() != 0)
                command.Memo = true;
            if (command.AcrobatFieldContentModels.Count() != 0)
                command.AcrobatField = true;
            if (command.PolicyContentModels.Count() != 0)
                command.Policy = true;            
            if (command.TestContentModels.Count() != 0)
                command.Test = true;
            if (command.FormContentModels.Count() != 0)
                command.Form = true;

            var response = new CommandResponse();
            var tableEntry = new CustomDocument();
            if (Modelvalidation(command))
            {
                try
                {
                    #region ["Parent Data"]
                    var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
                    var standardUser = _standardUserRepository.Find(userId.ConvertToGuid());
                    var entry = _customDocumentRepository.Find(command.Id);
                    var isNew = entry == null;
                    if (isNew) 
                    {
                        entry = new CustomDocument
                        {
                            Id = command.Id,
                            CreatedBy = userId,
                            CreatedOn = command.CreatedOn,
                            ReferenceId = _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                        };
                    }                       

                    var collaborators = entry?.Collaborators ?? command.Collaborators?.ToList() ?? new List<StandardUser>();
                    collaborators.RemoveAll(x => x == null);
                    if (standardUser != null && !collaborators.Contains(standardUser))
                    {
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
                    entry.CoverPictureId = coverPicture?.Id;

                    var certificate = _queryExecutor.Execute<FetchByIdQuery, Certificate>(new FetchByIdQuery { Id = command.Certificate?.Id });
                    entry.Certificate = certificate;
                    entry.CertificateId = certificate?.Id;

                    entry.NotificationInteval = command.NotificationInteval;
                    entry.NotificationIntevalDaysBeforeExpiry = command.NotificationIntevalDaysBeforeExpiry;
                    entry.TestExpiresNumberDaysFromAssignment = command.TestExpiresNumberDaysFromAssignment;

                    entry.Description = command.Description;
                    entry.DocumentStatus = command.DocumentStatus;
                    entry.LastEditDate = command.CreatedOn;
                    entry.LastEditedBy = userId;
                    entry.Points = command.Points;
                    entry.PreviewMode = command.PreviewMode;
                    entry.Printable = command.Printable;
                    entry.Title = command.Title;
                    entry.IsGlobalAccessed = command.IsGlobalAccessed;
                    entry.IsMannualReview = command.IsMannualReview;
                    entry.IsResourceCentre = command.IsResourceCentre;
                    var values = string.IsNullOrWhiteSpace(command.TrainingLabels) ? new string[0] : command.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    
                    var labels = _queryExecutor.Execute<SyncDocumentLabelsQuery, IEnumerable<Label>>(new SyncDocumentLabelsQuery
                    {
                        Values = values,
                        ExistingModelIds = string.IsNullOrEmpty(entry.TrainingLabels) ? Enumerable.Empty<string>() : entry.TrainingLabels.Split(',')
                    }).ToArray();
                    
                    entry.TrainingLabels = labels.Any() ? string.Join(",", labels.Select(x => x.Name).ToArray()) : "";
                    entry.Approver = command.Approver;
                    entry.ApproverId = command.PublishStatus == DocumentPublishWorkflowStatus.Approved ? Guid.Parse(userId) : command.PublishStatus == DocumentPublishWorkflowStatus.Declined ? Guid.Parse(userId) : Guid.Empty;
                    entry.PublishStatus = command.PublishStatus;


                    if (isNew)
                    _customDocumentRepository.Add(entry);
                    _customDocumentRepository.SaveChanges();

                    var documentAudit = new DocumentAuditTrack
                    {
                        Id = Guid.NewGuid(),
                        LastEditDate = command.CreatedOn,
                        LastEditedBy = userId,
                        DocumentId = entry.Id.ToString(),
                        DocumentName = command.Title,
                        DocumentType = DocumentType.TrainingManual.ToString(),
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
                        documentAudit.DocumentStatus = "Document Created";
                    else
                    {
                        documentAudit.DocumentStatus = "Document" + " " + (command.DocumentStatus == DocumentStatus.Draft ? "Edited" : command.DocumentStatus.ToString());
                    }

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

                    if (isNew)
                        _documentAuditRepository.Add(documentAudit);
                    _documentAuditRepository.SaveChanges();
                    #endregion

                    #region ["TrainingManual"]
                    if (command.TrainingMannual)
                    {
                        try
                        {
                            #region
                            var entryTM = _tmRepository.List.Where(z => z.CustomDocummentId == Guid.Parse(command.Id.ToString())).FirstOrDefault();
                            var isNewTM = entryTM == null;
                            if (isNewTM) entryTM = new TrainingManual
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CreatedBy = userId,
                                    CreatedOn = DateTime.Now.Date,
                                    ReferenceId =_queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                                };                            

                            var categoryTM = _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = command.Category?.Id });
                            if (categoryTM != null) entry.DocumentCategoryId = category.Id;
                            
                            //var coverPictureTM = _queryExecutor.Execute<SyncDocumentCoverPictureQuery, Upload>(new SyncDocumentCoverPictureQuery
                            //{
                            //    ExistingUploadId = entry.CoverPicture?.Id,                               
                            //});
                            
                            //entryTM.CoverPicture = coverPicture;

                            entryTM.Description = command.Description;
                            
                            entryTM.DocumentStatus = command.DocumentStatus;
                            
                            entryTM.LastEditDate = DateTime.Now.Date;
                            
                            entryTM.LastEditedBy = userId;
                            
                            entryTM.Points = command.Points;
                            
                            entryTM.PreviewMode = command.PreviewMode;                     
                            
                            entryTM.Printable = command.Printable;
                            
                            entryTM.Title = command.Title;
                            
                            entryTM.IsGlobalAccessed = command.IsGlobalAccessed;
                            
                            var valuesTM = string.IsNullOrWhiteSpace(command.TrainingLabels) ? new string[0] : command.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            
                            var labelsTM = _queryExecutor.Execute<SyncDocumentLabelsQuery, IEnumerable<Label>>(new SyncDocumentLabelsQuery
                            {
                                Values = valuesTM,
                                ExistingModelIds = string.IsNullOrEmpty(entry.TrainingLabels) ? Enumerable.Empty<string>() : entry.TrainingLabels.Split(',')
                            }).ToArray();
                            
                            entryTM.TrainingLabels = labels.Any() ? string.Join(",", labels.Select(x => x.Name).ToArray()) : "";
                            
                            entryTM.Approver = command.Approver;
                            
                            entryTM.ApproverId = command.PublishStatus == DocumentPublishWorkflowStatus.Approved ? Guid.Parse(userId) : command.PublishStatus == DocumentPublishWorkflowStatus.Declined ? Guid.Parse(userId) : Guid.Empty;
                            
                            entryTM.PublishStatus = command.PublishStatus;
                            
                            entryTM.IsCustomDocument = true;
                            
                            entryTM.CustomDocummentId = Guid.Parse(command.Id.ToLower());


                            SyncChapters(command.TMContentModels, entryTM);

                            foreach (var model in command.TMContentModels)
                            {
                                if (string.IsNullOrEmpty(model.Title)) {
                                    throw new MissingFieldException();
                                }
                                if (model.ChapterDocLinks != null && model.ChapterDocLinks != "" && model.ChapterDocNames != null && model.ChapterDocNames != "")
                                {
                                    string[] urls = model.ChapterDocLinks.Split(',');
                                    string[] names = model.ChapterDocNames.Split(',');
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
                                                        DocumentId = docId.ToString(),
                                                        Url = urls[j],
                                                        ChapterId = model.Id,
                                                        Name = names[i]
                                                    };
                                                    webFrame.Add(docUrl);
                                                }
                                            }
                                        }

                                        foreach (var link in webFrame)
                                        {
                                            _documentUrlRepository.Add(link);
                                        }
                                        _documentUrlRepository.SaveChanges();
                                    }
                                }
                            }

                            if (isNewTM)
                                _tmRepository.Add(entryTM);
                            _tmRepository.SaveChanges();
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            response.Id = command.Id.ToString();
                            response.ErrorMessage = ex.Message;
                        }
                    }

                    #endregion

                    #region ["Checklist/ActivityBook"]
                    if (command.CheckList)
                    {
                        try
                        {
                            #region
                            var entryCL = _clRepository.List.Where(z => z.CustomDocummentId == Guid.Parse(command.Id.ToString())).FirstOrDefault();
                            var isNewCL = entryCL == null;
                            if (isNewCL)
                                entryCL = new CheckList
                                {
                                    Id = command.Id.ToString(),
                                    CreatedBy = userId,
                                    CreatedOn = DateTime.UtcNow,
                                    ReferenceId =
                                        _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                                };

                            var categoryCL = _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = command.Category?.Id });
                            if (category != null) entryCL.DocumentCategoryId = category.Id;

                            entryCL.Description = command.Description;
                            entryCL.DocumentStatus = command.DocumentStatus;
                            entryCL.LastEditDate = DateTime.UtcNow;
                            entryCL.LastEditedBy = userId;
                            entryCL.Points = command.Points;
                            entryCL.PreviewMode = command.PreviewMode;
                            entryCL.Printable = command.Printable;
                            entryCL.Title = command.Title;                            
                            entryCL.Approver = command.Approver;
                            entryCL.ApproverId = command.PublishStatus == DocumentPublishWorkflowStatus.Approved ? Guid.Parse(userId) : command.PublishStatus == DocumentPublishWorkflowStatus.Declined ? Guid.Parse(userId) : Guid.Empty;
                            entryCL.PublishStatus = command.PublishStatus;
                            entryCL.IsCustomDocument = true;
                            entryCL.CustomDocummentId = Guid.Parse(command.Id.ToLower());

                            SyncChaptersCL(command.CLContentModels, entryCL);

                            foreach (var model in command.CLContentModels)
                            {
                                if (string.IsNullOrEmpty(model.Title))
                                {
                                    throw new MissingFieldException();
                                }
                                if (model.ChapterDocLinks != null && model.ChapterDocLinks != "" && model.ChapterDocNames != null && model.ChapterDocNames != "")
                                {
                                    string[] urls = model.ChapterDocLinks.Split(',');
                                    string[] names = model.ChapterDocNames.Split(',');
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
                                                        DocumentId = docId.ToString(),
                                                        Url = urls[j],
                                                        ChapterId = model.Id,
                                                        Name = names[i]
                                                    };
                                                    webFrame.Add(docUrl);
                                                }
                                            }
                                        }

                                        foreach (var link in webFrame)
                                        {
                                            _documentUrlRepository.Add(link);
                                        }
                                        _documentUrlRepository.SaveChanges();
                                    }
                                }
                            }

                            if (isNewCL)
                                _clRepository.Add(entryCL);
                            _clRepository.SaveChanges();

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            response.Id = command.Id.ToString();
                            response.ErrorMessage = ex.Message;
                        }

                    }

                    #endregion

                    #region ["Test"]
                    if (command.Test)
                    {

                        try
                        {
                            #region
                            var entryTest = _testRepository.GetAll().Where(z => z.CustomDocummentId == Guid.Parse(command.Id.ToString())).FirstOrDefault();
                            var isNewTest = entryTest == null;
                            if (isNewTest)
                                entryTest = new Test
                                {
                                    Id = command.Id.ToString(),
                                    CreatedBy = userId,
                                    CreatedOn = command.CreatedOn,
                                    ReferenceId = _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                                };

                            var collaboratorsTest = entry?.Collaborators ?? command.Collaborators?.ToList() ?? new List<StandardUser>();
                            collaborators.RemoveAll(x => x == null); // command.Collaborators is populated with [null]
                            if (standardUser != null && !collaboratorsTest.Contains(standardUser))
                            {
                                collaboratorsTest.Add(standardUser);
                            }
                            DocumentCommandHandler.SyncCollaborators(entryTest, collaboratorsTest);

                            var categoryTest = _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = command.Category?.Id });
                            if (categoryTest != null)
                                entryTest.CategoryId = category.Id;
                            //var coverPictureTest = _queryExecutor.Execute<SyncDocumentCoverPictureQuery, Upload>(new SyncDocumentCoverPictureQuery
                            //{
                            //    ExistingUploadId = entry.CoverPicture?.Id,                            
                            //});
                            
                            //entry.CoverPicture = coverPicture;
                            
                            entryTest.Description = command.Description;
                            entryTest.DocumentStatus = command.DocumentStatus;
                            entryTest.LastEditDate = command.CreatedOn;
                            entryTest.Points = command.Points;
                            entryTest.PreviewMode = command.PreviewMode;
                            entryTest.Printable = command.Printable;

                            #region Added by Softude
                            entryTest.Title = command.Title;
                            
                            #endregion


                            //entryTest.PassMarks = command.PassMarks;
                            //entryTest.Duration = command.Duration ?? 0;
                            //entryTest.MaximumAttempts = command.MaximumAttempts;
                            //entryTest.IntroductionContent = command.IntroductionContent;
                            //entryTest.LastEditedBy = userId;
                            //entryTest.NotificationInteval = command.NotificationInteval;
                            //entryTest.NotificationIntevalDaysBeforeExpiry = command.NotificationIntevalDaysBeforeExpiry;
                            //entryTest.TestExpiresNumberDaysFromAssignment = command.TestExpiresNumberDaysFromAssignment;
                            //entryTest.AssignMarksToQuestions = command.AssignMarksToQuestions;
                            //entryTest.TestReview = command.TestReview;
                            //entryTest.RandomizeQuestions = command.RandomizeQuestions;
                            //entryTest.EmailSummary = command.EmailSummary;
                            //entryTest.HighlightAnswersOnSummary = command.HighlightAnswersOnSummary;
                            //entryTest.OpenTest = command.OpenTest;
                            //entryTest.EnableTimer = command.EnableTimer;
                            entryTest.IsGlobalAccessed = command.IsGlobalAccessed;
                            entryTest.Approver = command.Approver;
                            entryTest.ApproverId = command.PublishStatus == DocumentPublishWorkflowStatus.Approved ? Guid.Parse(userId) : command.PublishStatus == DocumentPublishWorkflowStatus.Declined ? Guid.Parse(userId) : Guid.Empty;
                            entryTest.PublishStatus = (DocumentPublishWorkflowStatus)command.PublishStatus;
                            entryTest.IsCustomDocument = true;
                            entryTest.CustomDocummentId = Guid.Parse(command.Id.ToLower());


                            SyncQuestions(command.TestContentModels, entryTest);
                            if (isNewTest)
                                _testRepository.Add(entryTest);
                            _testRepository.SaveChanges();

                            #endregion
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }

                    #endregion

                    #region ["Memo"]
                    if (command.Memo)
                    {

                        try
                        {
                            #region
                            var entryMemo = _memoRepository.List.Where(x => x.CustomDocummentId == Guid.Parse(command.Id)).FirstOrDefault();
                            var isNewMemo = entryMemo == null;
                            if (isNewMemo)
                                entryMemo = new Memo
                                {
                                    Id = command.Id.ToString(),
                                    CreatedBy = userId,
                                    CreatedOn = command.CreatedOn,
                                    ReferenceId = _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                                };

                            //var collaboratorsMemo = entryMemo?.Collaborators ?? command.Collaborators?.ToList() ?? new List<StandardUser>();
                            //collaboratorsMemo.RemoveAll(x => x == null); // command.Collaborators is populated with [null]
                            //if (standardUser != null && !collaboratorsMemo.Contains(standardUser))
                            //{
                            //    collaboratorsMemo.Add(standardUser);
                            //}
                            //DocumentCommandHandler.SyncCollaborators(entryMemo, collaboratorsMemo);

                            var categoryMemoMemo = _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = command.Category?.Id });
                            if (category != null)
                                entryMemo.CategoryId = category.Id;
                            var coverPictureMemo = _queryExecutor.Execute<SyncDocumentCoverPictureQuery, Upload>(new SyncDocumentCoverPictureQuery
                            {
                                ExistingUploadId = entry.CoverPicture?.Id,
                                //ModelId = command.CoverPicture?.Id
                            });
                            entryMemo.CoverPicture = coverPicture;

                            entryMemo.Description = command.Description;
                            entryMemo.DocumentStatus = command.DocumentStatus;
                            entryMemo.LastEditDate = command.CreatedOn;
                            entryMemo.LastEditedBy = userId;
                            entryMemo.Points = command.Points;
                            entryMemo.PreviewMode = command.PreviewMode;
                            entryMemo.Printable = command.Printable;
                            entryMemo.Title = command.Title;
                            entryMemo.IsGlobalAccessed = command.IsGlobalAccessed;
                            entryMemo.Approver = command.Approver;
                            entryMemo.ApproverId = command.PublishStatus == DocumentPublishWorkflowStatus.Approved ? Guid.Parse(userId) : command.PublishStatus == DocumentPublishWorkflowStatus.Declined ? Guid.Parse(userId) : Guid.Empty;
                            entryMemo.PublishStatus = command.PublishStatus;
                            entryMemo.IsCustomDocument = true;
                            entryMemo.CustomDocummentId = Guid.Parse(command.Id.ToLower()); //added by GK


                            SyncContent(command.MemoContentModels, entryMemo);

                            foreach (var model in command.MemoContentModels)
                            {
                                if (string.IsNullOrEmpty(model.Title))
                                {
                                    throw new MissingFieldException();
                                }
                                if (model.ChapterDocLinks != null && model.ChapterDocLinks != "" && model.ChapterDocNames != null && model.ChapterDocNames != "")
                                {
                                    string[] urls = model.ChapterDocLinks.Split(',');
                                    string[] names = model.ChapterDocNames.Split(',');
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
                                                        DocumentId = docId.ToString(),
                                                        Url = urls[j],
                                                        ChapterId = model.Id,
                                                        Name = names[i]
                                                    };
                                                    webFrame.Add(docUrl);
                                                }
                                            }
                                        }

                                        foreach (var link in webFrame)
                                        {
                                            _documentUrlRepository.Add(link);
                                        }
                                        _documentUrlRepository.SaveChanges();
                                    }
                                }
                            }
                            if (isNewMemo)
                                _memoRepository.Add(entryMemo);
                            _memoRepository.SaveChanges();

                            //ConditionalTable conditional = new ConditionalTable {
                            //    ChapterID = entryMemo.Id, TestAnswer=entryMemo

                            //};
                            //addUpdateConditionalLogic(command);
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            response.Id = command.Id.ToString();
                            response.ErrorMessage = ex.Message;
                        }


                    }

                    #endregion

                    #region ["Policy"]
                    if (command.Policy)
                    {
                        try
                        {
                            #region
                            var entryPolicy = _policyRepository.List.Where(x => x.CustomDocummentId == Guid.Parse(command.Id.ToString())).FirstOrDefault();
                            var isNewPolicy = entryPolicy == null;
                            if (isNewPolicy)
                                entryPolicy = new Policy
                                {
                                    Id = command.Id.ToString(),
                                    CreatedBy = userId,
                                    CreatedOn = command.CreatedOn,
                                    ReferenceId = _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                                };

                            entryPolicy.Title = command.PolicyContent.Title;
                            entryPolicy.CallToAction = true;
                            entryPolicy.CallToActionMessage = command.PolicyContent.CallToActionMessage;
                            entryPolicy.IsGlobalAccessed = command.PolicyContent.IsGlobalAccessed;
                            entryPolicy.Approver = command.PolicyContent.Approver;
                            entryPolicy.ApproverId = command.PolicyContent.PublishStatus == DocumentPublishWorkflowStatus.Approved ? Guid.Parse(userId) : command.PublishStatus == DocumentPublishWorkflowStatus.Declined ? Guid.Parse(userId) : Guid.Empty;
                            entryPolicy.PublishStatus = command.PolicyContent.PublishStatus;
                            entryPolicy.IsCustomDocument = true;
                            entryPolicy.CustomDocummentId = Guid.Parse(command.Id.ToLower());
                            entryPolicy.CustomDocumentOrder = command.PolicyContent.CustomDocumentOrder;

                            
                            //********************* This Block Has Been Modified By Softude *******************************
                            SyncContentPolicy(command.PolicyContentModels, entryPolicy);
                            //********************************* Ended *****************************************************
                            foreach (var model in command.PolicyContentModels)
                            {
                                if (string.IsNullOrEmpty(model.Title))
                                {
                                    throw new MissingFieldException();
                                }
                                if (model.ChapterDocLinks != null && model.ChapterDocLinks != "" && model.ChapterDocNames != null && model.ChapterDocNames != "")
                                {
                                    string[] urls = model.ChapterDocLinks.Split(',');
                                    string[] names = model.ChapterDocNames.Split(',');
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
                                                        DocumentId = docId.ToString(),
                                                        Url = urls[j],
                                                        ChapterId = model.Id,
                                                        Name = names[i]
                                                    };
                                                    webFrame.Add(docUrl);
                                                }
                                            }
                                        }

                                        foreach (var link in webFrame)
                                        {
                                            _documentUrlRepository.Add(link);
                                        }
                                        _documentUrlRepository.SaveChanges();
                                    }
                                }
                            }
                            if (isNewPolicy)
                                _policyRepository.Add(entryPolicy);
                            _policyRepository.SaveChanges();
                            ConditionalTable conditional = new ConditionalTable
                            {

                                ChapterID = Guid.Parse(entryPolicy.Id),
                                TestAnswer = command.PolicyContent.selectedTestAnswer,
                                TestQuestion = command.PolicyContent.selectedTestQuestion,
                                CustomDocumentID =Guid.Parse( command.Id),
                                documentType = DocumentType.Policy,
                                CreatedOn = DateTime.Now
                            };
                            addUpdateConditionalLogic(conditional);
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            response.Id = command.Id.ToString();
                            response.ErrorMessage = ex.Message;
                        }
                    }

                    #endregion

                    #region ["AcrobatField"]
                    if (command.AcrobatField)
                    {

                        try
                        {
                            #region
                            var entryAcrobatField = _acrobatFieldRepository.List.Where(x => x.CustomDocummentId == Guid.Parse(command.Id)).FirstOrDefault();
                            var isNewAcrobatField = entryAcrobatField == null;
                            if (isNewAcrobatField)
                                entryAcrobatField = new AcrobatField
                                {
                                    Id = command.Id.ToString(),
                                    CreatedBy = userId,
                                    CreatedOn = command.CreatedOn,
                                    ReferenceId = _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                                };

                            //var collaboratorsAcrobatField = entryAcrobatField?.Collaborators ?? command.Collaborators?.ToList() ?? new List<StandardUser>();
                            //collaboratorsAcrobatField.RemoveAll(x => x == null); // command.Collaborators is populated with [null]
                            //if (standardUser != null && !collaboratorsAcrobatField.Contains(standardUser))
                            //{
                            //    collaboratorsAcrobatField.Add(standardUser);
                            //}
                            //DocumentCommandHandler.SyncCollaborators(entryAcrobatField, collaboratorsAcrobatField);

                            var categoryAcrobatField = _queryExecutor.Execute<FetchByIdQuery, DocumentCategory>(new FetchByIdQuery { Id = command.Category?.Id });
                            if (category != null)
                                entryAcrobatField.CategoryId = category.Id;
                            var coverPictureAcrobatField = _queryExecutor.Execute<SyncDocumentCoverPictureQuery, Upload>(new SyncDocumentCoverPictureQuery
                            {
                                ExistingUploadId = entry.CoverPicture?.Id,
                                //ModelId = command.CoverPicture?.Id
                            });
                            entryAcrobatField.CoverPicture = coverPicture;

                            entryAcrobatField.Description = command.Description;
                            entryAcrobatField.DocumentStatus = command.DocumentStatus;
                            entryAcrobatField.LastEditDate = command.CreatedOn;
                            entryAcrobatField.LastEditedBy = userId;
                            entryAcrobatField.Points = command.Points;
                            entryAcrobatField.PreviewMode = command.PreviewMode;
                            entryAcrobatField.Printable = command.Printable;
                            entryAcrobatField.Title = command.Title;
                            entryAcrobatField.IsGlobalAccessed = command.IsGlobalAccessed;
                            entryAcrobatField.Approver = command.Approver;
                            entryAcrobatField.ApproverId = command.PublishStatus == DocumentPublishWorkflowStatus.Approved ? Guid.Parse(userId) : command.PublishStatus == DocumentPublishWorkflowStatus.Declined ? Guid.Parse(userId) : Guid.Empty;
                            entryAcrobatField.PublishStatus = command.PublishStatus;
                            entryAcrobatField.IsCustomDocument = true;
                            entryAcrobatField.CustomDocummentId = Guid.Parse(command.Id.ToLower()); //added by GK


                            SyncContent(command.AcrobatFieldContentModels, entryAcrobatField);

                            foreach (var model in command.AcrobatFieldContentModels)
                            {
                                if (string.IsNullOrEmpty(model.Title))
                                {
                                    throw new MissingFieldException();
                                }
                                if (model.ChapterDocLinks != null && model.ChapterDocLinks != "" && model.ChapterDocNames != null && model.ChapterDocNames != "")
                                {
                                    string[] urls = model.ChapterDocLinks.Split(',');
                                    string[] names = model.ChapterDocNames.Split(',');
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
                                                        DocumentId = docId.ToString(),
                                                        Url = urls[j],
                                                        ChapterId = model.Id,
                                                        Name = names[i]
                                                    };
                                                    webFrame.Add(docUrl);
                                                }
                                            }
                                        }

                                        foreach (var link in webFrame)
                                        {
                                            _documentUrlRepository.Add(link);
                                        }
                                        _documentUrlRepository.SaveChanges();
                                    }
                                }
                            }
                            if (isNewAcrobatField)
                                _acrobatFieldRepository.Add(entryAcrobatField);
                            _acrobatFieldRepository.SaveChanges();

                            //ConditionalTable conditional = new ConditionalTable {
                            //    ChapterID = entryMemo.Id, TestAnswer=entryMemo

                            //};
                            //addUpdateConditionalLogic(command);
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            response.Id = command.Id.ToString();
                            response.ErrorMessage = ex.Message;
                        }


                    }

                    #endregion

                    #region ["Form"]

                    if (command.Form) 
                    {
                        try
                        {
                            var entryForm = _formRepository.GetAll().Where(z => z.CustomDocummentId == Guid.Parse(command.Id.ToString())).FirstOrDefault();
                            var isNewForm = entryForm == null;

                            if (isNewForm)
                                entryForm = new Form
                                {
                                    Id = command.Id.ToString(),
                                    CreatedBy = userId,
                                    CreatedOn = command.CreatedOn,
                                    ReferenceId = _queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery())
                                };

                            entryForm.Title = command.Title;
                            entryForm.Description = command.Description;
                            entryForm.CustomDocummentId = Guid.Parse(command.Id.ToLower());

                            SyncFormChapter(command.FormContentModels, entryForm);

                            if (isNewForm)
                                _formRepository.Add(entryForm);
                            _formRepository.SaveChanges();

                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    response.Id = command.Id.ToString();
                    response.ErrorMessage = ex.Message;
                }
            }

            return response;
        }

        #region ["Conditional Logic"]
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

        #endregion

        #region Training Mannual 
        private void SyncChapters(IEnumerable<TrainingManualChapterModel> command, TrainingManual entry)
        {
            var existingIds = entry.Chapters.Where(c => c.Deleted).Select(c => c.Id.ToString()).ToArray();
            var commandIds = command.Where(c => !c.Deleted).Select(x => x.Id).ToArray();
            var removedChapters = existingIds.Where(id => !commandIds.Contains(id));

            CreateOrUpdateChapters(entry, command.Where(c => !c.Deleted && !removedChapters.Contains(c.Id)).ToArray()).ToList().ForEach(e => entry.Chapters.Add(e));

            removedChapters.ForEach(id =>
            {
                var e = _chapterRepositoryTM.Find(id);
                if (e != null)
                {
                    e.Deleted = true;
                    foreach (var u in e.ContentToolsUploads)
                        u.Deleted = true;
                    foreach (var u in e.Uploads)
                        u.Deleted = true;
                }
            });

            entry.Chapters.ForEach(c => { c.Number = command.ToList().IndexOf(command.FirstOrDefault(x => x.Id == c.Id)) + 1; });

        }

        private IEnumerable<TrainingManualChapter> CreateOrUpdateChapters(TrainingManual entry, params TrainingManualChapterModel[] models)
        {
            var chapters = new List<TrainingManualChapter>();

            models.ToList().ForEach(c =>
            {
                var e = _chapterRepositoryTM.Find(c.Id);
                var isNew = e == null;

                if (e == null)
                    e = new TrainingManualChapter { Id = c.Id.ToString(), TrainingManualId = entry.Id, TrainingManual = entry };
                e.Content = c.Content;
                //e.Deleted = c.Deleted;
                e.Title = c.Title;
                e.IsAttached = c.IsAttached;
                e.IsSignOff = c.IsSignOff;
                e.NoteAllow = c.NoteAllow;
                e.AttachmentRequired = c.AttachmentRequired;
                e.CustomDocumentOrder = c.CustomDocumentOrder;
                e.IsConditionalLogic = c.IsConditionalLogic;

                _queryExecutor.Execute<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentUploadsQuery
                {
                    ExistingModelIds = e.Uploads.Select(x => x.Id).ToArray(),
                    Models = c.Attachments.ToArray()
                }).ToList().ForEach(x => e.Uploads.Add(x));
                e.Uploads.ForEach(u => { u.Order = c.Attachments.ToList().IndexOf(c.Attachments.FirstOrDefault(x => x.Id == u.Id)) + 1; });

                _queryExecutor.Execute<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentToolsUploadsQuery
                {
                    ExistingModelIds = e.ContentToolsUploads.Select(x => x.Id).ToArray(),
                    Models = GetUploadUrls(c.Content)
                }).ToList().ForEach(x => e.ContentToolsUploads.Add(x));

                if (isNew)
                    chapters.Add(e);

                ConditionalTable conditional = new ConditionalTable
                {

                    ChapterID = Guid.Parse(e.Id),
                    TestAnswer = c.selectedTestAnswer,
                    TestQuestion = c.selectedTestQuestion,
                    CustomDocumentID = entry.CustomDocummentId,
                    documentType = DocumentType.TrainingManual,
                    CreatedOn = DateTime.Now
                };
                addUpdateConditionalLogic(conditional);
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
        #endregion

        #region CheckList
        private void SyncChaptersCL(IEnumerable<CheckListChapterModel> command, CheckList entry)
        {
            var existingIds = entry.Chapters.Where(c => !c.Deleted).Select(c => c.Id.ToString()).ToArray();
            var commandIds = command.Where(c => !c.Deleted).Select(x => x.Id).ToArray();
            var removedChapters = existingIds.Where(id => !commandIds.Contains(id));

            CreateOrUpdateChapters(entry, command.Where(c => !c.Deleted && !removedChapters.Contains(c.Id)).ToArray()).ToList().ForEach(e => entry.Chapters.Add(e));

            removedChapters.ForEach(id =>
            {
                var e = _chapterRepositoryCL.Find(id);
                if (e != null)
                {
                    e.Deleted = true;
                    foreach (var u in e.ContentToolsUploads)
                        u.Deleted = true;
                    foreach (var u in e.Uploads)
                        u.Deleted = true;
                }
            });

            entry.Chapters.ForEach(c => { c.Number = command.ToList().IndexOf(command.FirstOrDefault(x => x.Id == c.Id)) + 1; });

        }

        private IEnumerable<CheckListChapter> CreateOrUpdateChapters(CheckList entry, params CheckListChapterModel[] models)
        {
            var chapters = new List<CheckListChapter>();

            models.ToList().ForEach(c =>
            {
                var e = _chapterRepositoryCL.Find(c.Id);
                var isNew = e == null;

                if (e == null)
                    e = new CheckListChapter { Id = c.Id.ToString(), CheckListId = entry.Id, CheckList = entry };
                e.Content = c.Content;
                //e.Deleted = c.Deleted;
                e.Title = c.Title;
                e.AttachmentRequired = c.AttachmentRequired;
                e.IsChecked = c.IsChecked;
                e.NoteAllow = c.NoteAllow;
                e.CheckRequired = c.CheckRequired;
                e.IsSignOff = c.IsSignOff;
                e.CustomDocumentOrder = c.CustomDocumentOrder;
                e.IsConditionalLogic = c.IsConditionalLogic;
                e.IsAttached = c.IsAttached;

                _queryExecutor.Execute<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentUploadsQuery
                {
                    ExistingModelIds = e.Uploads.Select(x => x.Id).ToArray(),
                    Models = c.Attachments.ToArray()
                }).ToList().ForEach(x => e.Uploads.Add(x));
                e.Uploads.ForEach(u => { u.Order = c.Attachments.ToList().IndexOf(c.Attachments.FirstOrDefault(x => x.Id == u.Id)) + 1; });

                _queryExecutor.Execute<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentToolsUploadsQuery
                {
                    ExistingModelIds = e.ContentToolsUploads.Select(x => x.Id).ToArray(),
                    Models = GetUploadUrls(c.Content)
                }).ToList().ForEach(x => e.ContentToolsUploads.Add(x));

                if (isNew)
                    chapters.Add(e);

                ConditionalTable conditional = new ConditionalTable
                {

                    ChapterID = Guid.Parse(e.Id),
                    TestAnswer = c.selectedTestAnswer,
                    TestQuestion = c.selectedTestQuestion,
                    CustomDocumentID = entry.CustomDocummentId,
                    documentType = DocumentType.Checklist,
                    CreatedOn = DateTime.Now
                };
                addUpdateConditionalLogic(conditional);
            });
            return chapters;
        }
        //public IEnumerable<UploadFromContentToolsResultModel> GetUploadUrlsCL(string html)
        //{
        //	if (string.IsNullOrWhiteSpace(html))
        //		return Enumerable.Empty<UploadFromContentToolsResultModel>();
        //	var imgTags = html.FindHTMLTags("img").Apply(tag => tag.FindHtmlAttr("src")).ToList();
        //	var divTags = html.FindHTMLTags("div", "ce-element--type-image").Apply(tag => tag.FindInlineCss("background-image")).RemoveEmpty().Select(x => x.SubstringAfter("url('").SubstringBefore("')")).ToList();
        //	return imgTags.Union(divTags).Select(x => new UploadFromContentToolsResultModel { url = x }).ToList();
        //}
        #endregion

        #region Memo
        private void SyncContent(IEnumerable<MemoContentBoxModel> command, Memo entry)
        {
            var existingIds = entry.ContentBoxes.Where(c => !c.Deleted).Select(c => c.Id.ToString()).ToArray();
            var commandIds = command.Where(c => !c.Deleted).Select(x => x.Id).ToArray();
            var removedChapters = existingIds.Where(id => !commandIds.Contains(id));

            CreateOrUpdateContent(entry, command.Where(c => !c.Deleted && !removedChapters.Contains(c.Id)).ToArray()).ToList().ForEach(e => entry.ContentBoxes.Add(e));

            removedChapters.ToList().ForEach(id =>
            {
                var e = _contentRepository.Find(id);
                if (e != null)
                {
                    e.Deleted = true;
                    foreach (var u in e.ContentToolsUploads)
                        u.Deleted = true;
                    foreach (var u in e.Uploads)
                        u.Deleted = true;
                }
            });

            entry.ContentBoxes.ToList().ForEach(c => { c.Number = command.ToList().IndexOf(command.FirstOrDefault(x => x.Id == c.Id)) + 1; });

        }
        private IEnumerable<MemoContentBox> CreateOrUpdateContent(Memo entry, params MemoContentBoxModel[] models)
        {
            var chapters = new List<MemoContentBox>();
            models.ToList().ForEach(c =>
            {
                var e = _contentRepository.Find(c.Id);
                var isNew = e == null;
                if (e == null)
                    e = new MemoContentBox { Id = c.Id.ToString(), MemoId = entry.Id, Memo = entry };
                e.Content = c.Content;
                //e.Deleted = c.Deleted;
                e.Title = c.Title;
                e.IsAttached = c.IsAttached;
                e.IsSignOff = c.IsSignOff;
                e.NoteAllow = c.NoteAllow;
                e.AttachmentRequired = c.AttachmentRequired;
                e.CustomDocumentOrder = c.CustomDocumentOrder;

                _queryExecutor.Execute<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentUploadsQuery
                {
                    ExistingModelIds = e.Uploads.Select(x => x.Id).ToArray(),
                    Models = c.Attachments.ToArray()
                }).ToList().ForEach(x => e.Uploads.Add(x));
                e.Uploads.ToList().ForEach(u => { u.Order = c.Attachments.ToList().IndexOf(c.Attachments.FirstOrDefault(x => x.Id == u.Id)) + 1; });

                _queryExecutor.Execute<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentToolsUploadsQuery
                {
                    ExistingModelIds = e.ContentToolsUploads.Select(x => x.Id).ToArray(),
                    Models = GetUploadUrls(c.Content)
                }).ToList().ForEach(x => e.ContentToolsUploads.Add(x));

                if (isNew)
                    chapters.Add(e);

                ConditionalTable conditional = new ConditionalTable
                {

                    ChapterID = Guid.Parse(e.Id),
                    TestAnswer = c.selectedTestAnswer,
                    TestQuestion = c.selectedTestQuestion,
                    CustomDocumentID = entry.CustomDocummentId,
                    documentType = DocumentType.Memo,
                    CreatedOn=DateTime.Now
                };
                addUpdateConditionalLogic(conditional);
            });
            return chapters;
        }
        #endregion

        #region  Form

        private void SyncFormChapter(IEnumerable<FormChapterModel> command, Form entry)
        {
            var existingIds = entry.FormChapters.Where(c => c.Deleted).Select(c => c.Id.ToString()).ToArray();
            var commandIds = command.Where(c => !c.Deleted).Select(x => x.Id).ToArray();
            var removedFormChapters = existingIds.Where(id => !commandIds.Contains(id));

            CreateOrUpdateFormChapter(entry, command.Where(c => !c.Deleted && !removedFormChapters.Contains(c.Id)).ToArray()).ToList().ForEach(e => entry.FormChapters.Add(e));

            entry.FormChapters.ToList().ForEach(c => { c.Number = command.ToList().IndexOf(command.FirstOrDefault(x => x.Id == c.Id)) + 1; });

        }

        private IEnumerable<FormChapter> CreateOrUpdateFormChapter(Form entry, params FormChapterModel[] models)
        {
            var formchapter = new List<FormChapter>();
            models.ToList().ForEach(c =>
            {
                var e = _formchapterRepository.Find(c.Id);
                var isNew = e == null;
                if (e == null)
                    e = new FormChapter { Id = c.Id.ToString(), FormId = entry.Id, Form = entry };
                e.Title = c.Title;
                //e.Deleted = c.Deleted;
                e.Content = c.Content;
                e.IsConditionalLogic = c.IsConditionalLogic;
                e.CheckRequired = c.CheckRequired;
                e.CustomDocumentOrder = c.CustomDocumentOrder;

                //_queryExecutor.Execute<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentUploadsQuery
                //{
                //    ExistingModelIds = e.Uploads.Select(x => x.Id).ToArray(),
                //    Models = c.Attachments.ToArray()
                //}).ToList().ForEach(x => e.Uploads.Add(x));
                //e.Uploads.ToList().ForEach(u => { u.Order = c.Attachments.ToList().IndexOf(c.Attachments.FirstOrDefault(x => x.Id == u.Id)) + 1; });

                //_queryExecutor.Execute<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentToolsUploadsQuery
                //{
                //    ExistingModelIds = e.ContentToolsUploads.Select(x => x.Id).ToArray(),
                //    Models = c.ContentToolsUploads.ToArray()
                //}).ToList().ForEach(x => e.ContentToolsUploads.Add(x));

                SyncFormFields(e, c.FormFields.ToArray());

                if (isNew)
                    formchapter.Add(e);

                ConditionalTable conditional = new ConditionalTable
                {

                    ChapterID = Guid.Parse(e.Id),
                    TestAnswer = c.selectedTestAnswer,
                    TestQuestion = c.selectedTestQuestion,
                    CustomDocumentID = entry.CustomDocummentId,
                    CreatedOn = DateTime.Now
                };
                addUpdateConditionalLogic(conditional);

            });
            return formchapter;
        }

        private void SyncFormFields(FormChapter entry, params FormFieldModel[] models)
        {
            var existingIds = entry.FormFields.Where(c => !c.Deleted).Select(c => c.Id.ToString()).ToArray();
            var commandIds = models.Where(c => !c.Deleted).Select(x => x.Id).ToArray();
            var removed = existingIds.Where(id => !commandIds.Contains(id));
            CreateOrUpdateFormFields(entry, models.Where(c => !c.Deleted && !removed.Contains(c.Id)).ToArray()).ToList().ForEach(e => entry.FormFields.Add(e));

            removed.ToList().ForEach(id =>
            {
                var e = _formfieldRepository.Find(id);
                if (e != null)
                    e.Deleted = true;
            });
            entry.FormFields.ToList().ForEach(c => { c.Number = models.ToList().IndexOf(models.FirstOrDefault(x => x.Id == c.Id)) + 1; });

        }

        private IEnumerable<FormField> CreateOrUpdateFormFields(FormChapter entry, params FormFieldModel[] models)
        {
            var formfieldsdata = new List<FormField>();
            models.ToList().ForEach(c =>
            {
                var e = _formfieldRepository.Find(c.Id);
                var isNew = e == null;
                if (e == null)
                    e = new FormField { Id = c.Id.ToString(), FormChapterId = entry.Id, FormChapter = entry };
                e.FormFieldName = c.FormFieldName;
                e.FormFieldDescription = c.FormFieldDescription;
                //e.Deleted = c.Deleted;

                if (isNew)
                    formfieldsdata.Add(e);

            });
            return formfieldsdata;
        }


        #endregion 

        #region Test

        private void SyncQuestions(IEnumerable<TestQuestionModel> command, Test entry)
        {
            var existingIds = entry.Questions.Where(c => c.Deleted).Select(c => c.Id.ToString()).ToArray();
            var commandIds = command.Where(c => !c.Deleted).Select(x => x.Id).ToArray();
            var removedQuestions = existingIds.Where(id => !commandIds.Contains(id));

            CreateOrUpdateQuestions(entry, command.Where(c => !c.Deleted && !removedQuestions.Contains(c.Id)).ToArray()).ToList().ForEach(e => entry.Questions.Add(e));

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

            entry.Questions.ToList().ForEach(c => { c.Number = command.ToList().IndexOf(command.FirstOrDefault(x => x.Id == c.Id)) + 1; });

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
                //e.Deleted = c.Deleted;
                e.AnswerWeightage = c.Marks;
                e.CorrectAnswerId = c.CorrectAnswerId;
                e.CheckRequired = c.CheckRequired;

                #region Added By Softude
                e.Title = c.Title;
                #endregion

                e.AttachmentRequired = c.AttachmentRequired;
                e.NoteAllow = c.NoteAllow;
                e.dynamicFields = c.dynamicFields;
                e.IsSignOff = c.IsSignOff;
                e.CustomDocumentOrder = c.CustomDocumentOrder;

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

                //below code added by softude

                ConditionalTable conditional = new ConditionalTable
                {

                    ChapterID = Guid.Parse(e.Id),
                    TestAnswer = c.selectedTestAnswer,
                    TestQuestion = c.selectedTestQuestion,
                    CustomDocumentID = entry.CustomDocummentId,
                    documentType = DocumentType.Test,
                    CreatedOn = DateTime.Now
                };
                addUpdateConditionalLogic(conditional);

            });
            return questions;
        }
        private void SyncQuestionAnswers(TestQuestion entry, params TestQuestionAnswerModel[] models)
        {
            var existingIds = entry.Answers.Where(c => !c.Deleted).Select(c => c.Id.ToString()).ToArray();
            var commandIds = models.Where(c => !c.Deleted).Select(x => x.Id).ToArray();
            var removed = existingIds.Where(id => !commandIds.Contains(id));
            CreateOrUpdateQuestionAnswers(entry, models.Where(c => !c.Deleted && !removed.Contains(c.Id)).ToArray()).ToList().ForEach(e => entry.Answers.Add(e));

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

        #endregion

        #region policy
        private void SyncContentPolicy(IEnumerable<PolicyContentBoxModel> command, Policy entry)  /*done changes by softude*/
        {
            var existingIds = entry.ContentBoxes.Where(c => !c.Deleted).Select(c => c.Id.ToString()).ToArray();

            var commandIds = command.Where(c => !c.Deleted).Select(x => x.Id).ToArray();

            //var commandIds = PolicyContent.Id;

            var removedChapters = existingIds.Where(id => !commandIds.Contains(id));

            CreateOrUpdateContentPolicy(entry, command.Where(c => !c.Deleted && !removedChapters.Contains(c.Id)).ToArray()).ToList().ForEach(e => entry.ContentBoxes.Add(e));

            removedChapters.ToList().ForEach(id =>
            {
                var e = _contentRepository.Find(id);
                if (e != null)
                {
                    e.Deleted = true;
                    foreach (var u in e.ContentToolsUploads)
                        u.Deleted = true;
                    foreach (var u in e.Uploads)
                        u.Deleted = true;
                }
            });
           
            entry.ContentBoxes.ToList().ForEach(c => { c.Number = command.ToList().IndexOf(command.FirstOrDefault(x => x.Id == c.Id)) + 1; });

        }
        private IEnumerable<PolicyContentBox> CreateOrUpdateContentPolicy(Policy entry, params PolicyContentBoxModel[] models)
        {
            var chapters = new List<PolicyContentBox>();            

            models.ToList().ForEach(c =>
            {
                var e = _contentRepositoryPolicy.Find(c.Id);
                var isNew = e == null;
                if (e == null)
                    e = new PolicyContentBox { Id = c.Id.ToString(), PolicyId = entry.Id, Policy = entry };
                e.Content = c.Content;
                //e.Deleted = c.Deleted;
                e.Title = c.Title;
                e.CustomDocumentOrder = c.CustomDocumentOrder;
                //********************* This Block Has Been Added By Softude *******************************
                e.IsAttached = c.IsAttached;
                e.IsSignOff = c.IsSignOff;
                e.NoteAllow = c.NoteAllow;
                e.AttachmentRequired = c.AttachmentRequired;
                e.IsConditionalLogic = c.IsConditionalLogic;
                e.CheckRequired = c.CheckRequired;
                //****************************************** Ended *****************************************

                _queryExecutor.Execute<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentUploadsQuery
                {
                    ExistingModelIds = e.Uploads.Select(x => x.Id).ToArray(),
                    Models = c.Attachments.ToArray()
                }).ToList().ForEach(x => e.Uploads.Add(x));
                e.Uploads.ToList().ForEach(u => { u.Order = c.Attachments.ToList().IndexOf(c.Attachments.FirstOrDefault(x => x.Id == u.Id)) + 1; });

                _queryExecutor.Execute<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentToolsUploadsQuery
                {
                    ExistingModelIds = e.ContentToolsUploads.Select(x => x.Id).ToArray(),
                    Models = GetUploadUrls(c.Content)
                }).ToList().ForEach(x => e.ContentToolsUploads.Add(x));

                if (isNew)
                    chapters.Add(e);

                ConditionalTable conditional = new ConditionalTable
                {

                    ChapterID = Guid.Parse(e.Id),
                    TestAnswer = c.selectedTestAnswer,
                    TestQuestion = c.selectedTestQuestion,
                    CustomDocumentID = entry.CustomDocummentId,
                    documentType = DocumentType.Policy,
                    CreatedOn = DateTime.Now
                };
                addUpdateConditionalLogic(conditional);
            });
            return chapters;
        }
        #endregion
        
        #region AcrobatField
        private void SyncContent(IEnumerable<AcrobatFieldContentBoxModel> command, AcrobatField entry)
        {
            var existingIds = entry.ContentBoxes.Where(c => !c.Deleted).Select(c => c.Id.ToString()).ToArray();
            var commandIds = command.Where(c => !c.Deleted).Select(x => x.Id).ToArray();
            var removedChapters = existingIds.Where(id => !commandIds.Contains(id));

            CreateOrUpdateContent(entry, command.Where(c => !c.Deleted && !removedChapters.Contains(c.Id)).ToArray()).ToList().ForEach(e => entry.ContentBoxes.Add(e));

            removedChapters.ToList().ForEach(id =>
            {
                var e = _contentRepository.Find(id);
                if (e != null)
                {
                    e.Deleted = true;
                    foreach (var u in e.ContentToolsUploads)
                        u.Deleted = true;
                    foreach (var u in e.Uploads)
                        u.Deleted = true;
                }
            });

            entry.ContentBoxes.ToList().ForEach(c => { c.Number = command.ToList().IndexOf(command.FirstOrDefault(x => x.Id == c.Id)) + 1; });

        }
        private IEnumerable<AcrobatFieldContentBox> CreateOrUpdateContent(AcrobatField entry, params AcrobatFieldContentBoxModel[] models)
        {
            var chapters = new List<AcrobatFieldContentBox>();
            models.ToList().ForEach(c =>
            {
                var e = _acrobatFieldContentRepository.Find(c.Id);
                var isNew = e == null;
                if (e == null)
                    e = new AcrobatFieldContentBox { Id = c.Id.ToString(), AcrobatFieldId = entry.Id, AcrobatField = entry };
                e.Content = c.Content;
                //e.Deleted = c.Deleted;
                e.Title = c.Title;
                e.IsAttached = c.IsAttached;
                e.IsSignOff = c.IsSignOff;
                e.NoteAllow = c.NoteAllow;
                e.AttachmentRequired = c.AttachmentRequired;
                e.CustomDocumentOrder = c.CustomDocumentOrder;

                _queryExecutor.Execute<SyncDocumentContentUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentUploadsQuery
                {
                    ExistingModelIds = e.Uploads.Select(x => x.Id).ToArray(),
                    Models = c.Attachments.ToArray()
                }).ToList().ForEach(x => e.Uploads.Add(x));
                e.Uploads.ToList().ForEach(u => { u.Order = c.Attachments.ToList().IndexOf(c.Attachments.FirstOrDefault(x => x.Id == u.Id)) + 1; });

                _queryExecutor.Execute<SyncDocumentContentToolsUploadsQuery, IEnumerable<Upload>>(new SyncDocumentContentToolsUploadsQuery
                {
                    ExistingModelIds = e.ContentToolsUploads.Select(x => x.Id).ToArray(),
                    Models = GetUploadUrls(c.Content)
                }).ToList().ForEach(x => e.ContentToolsUploads.Add(x));

                if (isNew)
                    chapters.Add(e);

                ConditionalTable conditional = new ConditionalTable
                {

                    ChapterID = Guid.Parse(e.Id),
                    TestAnswer = c.selectedTestAnswer,
                    TestQuestion = c.selectedTestQuestion,
                    CustomDocumentID = entry.CustomDocummentId,
                    documentType = DocumentType.AcrobatField,
                    CreatedOn=DateTime.Now
                };
                addUpdateConditionalLogic(conditional);
            });
            return chapters;
        }
        #endregion

    }
}
