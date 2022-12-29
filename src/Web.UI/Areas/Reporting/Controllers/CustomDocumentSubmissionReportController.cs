using com.sun.org.apache.bcel.@internal.generic;
using com.sun.xml.@internal.bind.v2.model.core;
using Common.Command;
using Common.Query;
using Common.Report;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.ScheduleReport;
using Domain.Customer.Models.Test;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Ramp.Contracts.Command.DocumentUsage;
using Ramp.Contracts.Command.TestSession;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.Query.AcrobatField;
using Ramp.Contracts.Query.CheckList;
using Ramp.Contracts.Query.CheckListChapterUserResult;
using Ramp.Contracts.Query.CheckListUserResult;
using Ramp.Contracts.Query.CustomDocument;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Memo;
using Ramp.Contracts.Query.Policy;
using Ramp.Contracts.Query.Report;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.Query.TestSession;
using Ramp.Contracts.Query.TrainingManual;
using Ramp.Contracts.Query.Upload;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;
using VirtuaCon;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Publishers;
using Web.UI.Code.Extensions;
using Web.UI.Controllers;
using static Ramp.Contracts.ViewModel.UserActivityAndPerformanceViewModel;

namespace Web.UI.Areas.Reporting.Controllers
{
    public class CustomDocumentSubmissionReportController : ExportController<CustomDocumentSummaryExportReportQuery>
    {
        // GET: Reporting/CheckListSubmissionReport
        public ActionResult Index(string id = null)
        {
            ViewBag.Id = id;
            ViewBag.CheckLists = ExecuteQuery<AllCustomDocumentListSubmissionReportQuery, IEnumerable<CustomDocumentCheckList>>(new AllCustomDocumentListSubmissionReportQuery()).Where(x => !string.IsNullOrWhiteSpace(x.Title) && x.Status != "Draft").Select(x => new MyCustumDocumentCheckList
            {
                Value = x.Title,
                Extra = x.Description,
                Id = x.Id
            }).ToList();

            return View();
        }

        [OutputCache(NoStore = true, Duration = 0)]
        [System.Web.Mvc.HttpGet]
        public ActionResult CustomDocumentPreview([FromUri] object id, string companyId = null, string checkUser = null)
        {
            if (companyId != null)
            {
                ExecuteCommand(new UpdateConnectionStringCommand { CompanyId = companyId });
            }
            var userDetails = ExecuteQuery<UserQueryParameter, Ramp.Contracts.ViewModel.UserViewModel>(new UserQueryParameter { UserId = Guid.Parse(checkUser) });
            ViewBag.FullName = userDetails.FullName;
            ViewBag.Links = new Dictionary<string, string>()
            {
                {"index", Url.Action("MyDocuments", "Document", new {Area = ""})},

                {"userFeedback:save", Url.Action("Save", "UserFeedback", new {Area = ""})},
                {"save",Url.Action("Save")},
                {"userFeedback:types", Url.Action("Types", "UserFeedback", new {Area = ""})},
                {"userFeedback:contentTypes", Url.Action("ContentTypes", "UserFeedback", new {Area = ""})},
                {"poll", Url.Action("TrackUsage", typeof(CheckList).Name, new {Area = ""})},
                {"inProgress", Url.Action("InProgress", "Test", new {Area = ""})},
                {"upload:posturl", Url.Action("Post","Upload",new { Area = ""}) },
                {"category:jsTree",Url.Action("JsTree","Category",new { Area = ""}) },
                {"generateId",Url.ActionLink<DefaultController>(a => a.GetGenerateId())},
                {"contentTools:PostFromContentToolsInitial",Url.Action("PostFromContentToolsInitial", "Upload", new { Area = "" }) },
                {"contentTools:PostFromContentToolsCommit", Url.Action("PostFromContentToolsCommit", "Upload", new { Area = "" }) },
                {"contentTools:RotateImage", Url.Action("RotateImage", "Upload", new { Area = "" }) },
            };
            var model = ExecuteQuery<FetchByIdQuery, CheckListModel>(new FetchByIdQuery { Id = id });
            if (model != null)
                Preview_PostProcess(model, companyId, checkUser);

            if (Thread.CurrentPrincipal.IsInStandardUserRole())
            {
                var userId = Thread.CurrentPrincipal.GetId().ToString();
                var documentType = DocumentType.Checklist;

                if (!ExecuteQuery<DocumentAssignedToUserQuery, bool>(new DocumentAssignedToUserQuery
                {
                    DocumentId = id.ToString(),
                    DocumentType = documentType,
                    UserId = userId
                }) && model != null && !model.IsGlobalAccessed)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                var testSession = ExecuteQuery<TestSessionQuery, TestSessionViewModel>(new TestSessionQuery
                {
                    UserId = userId
                });
                if (testSession != null)
                {
                    if ((testSession.EnableTimer && testSession.TimeLeft == TimeSpan.Zero) || (testSession.DocumentStatus.HasValue && testSession.DocumentStatus.Value == DocumentStatus.Recalled))
                    {
                        var testResult = ExecuteQuery<FetchByIdQuery<Test>, TestResultModel>(new FetchByIdQuery<Test> { Id = testSession.CurrentTestId });
                        var createTestResultCommand = JsonConvert.DeserializeObject<CreateTestResultCommand>(JsonConvert.SerializeObject(testResult)); // hack
                        createTestResultCommand.PortalContext = PortalContext.Current;
                        createTestResultCommand.UserId = userId;
                        createTestResultCommand.TimeLeft = testSession.TimeLeft;

                        ExecuteCommand(createTestResultCommand);
                        ExecuteCommand(new TestSessionEndCommand
                        {
                            UserId = userId
                        });
                    }
                    else if (documentType != DocumentType.Test ? !testSession.OpenTest : testSession.CurrentTestId != id.ToString())
                        return RedirectToAction("InProgress", "Test");
                }

                if (documentType != DocumentType.Test)
                {
                    var viewDate = DateTime.UtcNow;
                    //viewDate = DateTime.SpecifyKind(DateTime.ParseExact(viewDate.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null), DateTimeKind.Utc); // remove milliseconds
                    ExecuteCommand(new CreateOrUpdateDocumentUsageCommand
                    {
                        DocumentId = id.ToString(),
                        DocumentType = documentType,
                        UserId = userId,
                        ViewDate = viewDate
                    });
                    ViewBag.StartTime = viewDate.ToString();
                    ViewBag.TrackingInterval = ConfigurationManager.AppSettings["DocumentTrackingInterval"];
                }
            }

            ExecuteCommand(new UpdateConnectionStringCommand());

            Response.Cache.AppendCacheExtension("no-store, must-revalidate");
            return View(model);
        }

        public void Preview_PostProcess(CheckListModel model, string companyId = null, string userId = null)
        {
            if (model.CoverPicture != null)
            {
                model.CoverPicture.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(model.CoverPicture.Id, 300, 300, companyId)).Replace("/Reporting", "");
                model.CoverPicture.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(model.CoverPicture.Id, companyId)).Replace("/Reporting", "");
            }
            model.ContentModels.ToList();

            var assignedDocument = ExecuteQuery<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = userId, DocumentId = model.Id });

            if (assignedDocument != null)
            {

                var checkListUserResult = ExecuteQuery<CheckListUserResultQuery, CheckListUserResultViewModel>(new CheckListUserResultQuery { AssignedDocumentId = assignedDocument.Id });

                if (checkListUserResult != null)
                {
                    model.Status = checkListUserResult.Status;
                    model.SubmittedDate = checkListUserResult.SubmittedDate;
                }
                else
                {
                    model.Status = false;
                }

                foreach (var item in model.ContentModels)
                {

                    var checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = item.Id });

                    item.StandardUserAttachments = checkListChapterUserUpload;
                    item.StandardUserAttachments.ToList().ForEach(attachment =>
                    {
                        attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId)).Replace("/Reporting", "");
                        attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId)).Replace("/Reporting", "");
                        attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId)).Replace("/Reporting", "");
                    });

                    var checkListChapterUserResult = ExecuteQuery<CheckListChapterUserResultQuery, CheckListChapterUserResultViewModel>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = item.Id });
                    if (checkListChapterUserResult != null)
                    {
                        item.IsChecked = checkListChapterUserResult.IsChecked;
                        item.IssueDiscription = checkListChapterUserResult.IssueDiscription;
                    }
                    else
                        item.IsChecked = false;
                }
            }
            else
            {
                var checkListUserResult = ExecuteQuery<CheckListUserResultQuery, CheckListUserResultViewModel>(new CheckListUserResultQuery { DocumentId = model.Id, IsGlobalAccessed = true, UserId = userId });

                if (checkListUserResult != null)
                {
                    model.Status = checkListUserResult.Status;
                    model.SubmittedDate = checkListUserResult.SubmittedDate;
                }
                else
                {
                    model.Status = false;
                }

                foreach (var item in model.ContentModels)
                {

                    var checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { DocumentId = model.Id, UserId = userId, IsGlobalAccessed = true, CheckListChapterId = item.Id });

                    item.StandardUserAttachments = checkListChapterUserUpload;
                    item.StandardUserAttachments.ToList().ForEach(attachment =>
                    {
                        attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId)).Replace("/Reporting", "");
                        attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId)).Replace("/Reporting", "");
                        attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId)).Replace("/Reporting", "");
                    });

                    var checkListChapterUserResult = ExecuteQuery<CheckListChapterUserResultQuery, CheckListChapterUserResultViewModel>(new CheckListChapterUserResultQuery { DocumentId = model.Id, UserId = userId, IsGlobalAccessed = true, CheckListChapterId = item.Id });
                    if (checkListChapterUserResult != null)
                    {
                        item.IsChecked = checkListChapterUserResult.IsChecked;
                        item.IssueDiscription = checkListChapterUserResult.IssueDiscription;
                    }
                    else
                        item.IsChecked = false;
                }
            }

        }

        /// <summary>
        /// Get the checklist based on required selection option like date, checklist and status
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult GetCustomDocumentDetails(CustomDocumentSubmissionReportQuery query)
        {
            var checkLists = ExecuteQuery<CustomDocumentSubmissionReportQuery, IEnumerable<CustomdocumentInteractionModel>>(query);

            if (checkLists.Any())
                return new JsonResult { Data = checkLists, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return new JsonResult { Data = true, JsonRequestBehavior = JsonRequestBehavior.AllowGet };


            //var checkLists = ExecuteQuery<CustomDocumentSubmissionReportQuery, IEnumerable<ChecklistInteractionModel>>(query);

            //if (checkLists.Any())
            //    return new JsonResult { Data = checkLists, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //return new JsonResult { Data = true, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        //[System.Web.Http.HttpPost]
        //public JsonResult GetDeclineMessagesCD(CustomDocumentSubmissionReportQuery query) 
        //{
        //    var DeclineMessagesCD = ExecuteQuery<CustomDocumentSubmissionReportQuery, IEnumerable<CustomDocumentMessageCenter>>(query);

        //    return new JsonResult { Data = true, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //}


        public override ActionResult Zip([FromUri] CustomDocumentSummaryExportReportQuery query)
        {
            throw new NotImplementedException();
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult DownloadSubmissionsZip(CustomDocumentSubmissionReportQuery query)
        {
            query.PortalContext = PortalContext.Current;
            //  var checkLists =  ExecuteQuery<FetchByIdQuery, CustomDocumentModel>(new FetchByIdQuery { Id = query.CustomDocumentId });
            var customeDocumentSubmissionList = ExecuteQuery<CustomDocumentSubmissionReportQuery, IEnumerable<CustomdocumentInteractionModel>>(query).FirstOrDefault();

         //   var checkLists = ExecuteQuery<CustomDocumentSubmissionReportQuery, IEnumerable<CustomDocumentInteractionModel>>(query).Where(c => c.Id == query.CustomDocumentId);
           
            var memoryStream = new MemoryStream();
            var zipStream = new ZipOutputStream(memoryStream);
            zipStream.SetLevel(3);

            foreach (var chk in customeDocumentSubmissionList.Checklist)
            {

                var customDocumentList = ExecuteQuery<FetchByIdQuery, CustomDocumentModel>(new FetchByIdQuery { Id = chk.Id, userId = chk.UserId.ToString() });
                
                
                #region for xls checklist

                var chkQuery = new CheckListExportReportQuery();
                if (!string.IsNullOrEmpty(chkQuery.CompanyId))
                    PortalContext.Override(chkQuery.CompanyId.AsGuid());
                chkQuery.AddOnrampBranding = true;
                chkQuery.PortalContext = PortalContext.Current;
                chkQuery.UserId = Guid.Parse(chk.UserId);
                chkQuery.ResultId = query.CustomDocumentId;

                var model = ExecuteQuery<CheckListExportReportQuery, IExportModel>(chkQuery);
                if (string.IsNullOrEmpty(query.ToggleFilter))
                    query.ToggleFilter = model.Title;

                model.Title = chk.UserName.Trim() + "/" + chk.DocumentTitle.Trim().Replace("/", "") + ".xls";
                var stream = new MemoryStream();
                IReportDocumentWriter publisher = new ExcelReportPublisher();
                publisher.Write(model.Document, stream);
                stream.Position = 0;

                stream.Position = 0;
                var t = stream.ToArray();

                SaveFile(zipStream, model.Title, t);

                #endregion

                #region ["PDF"]

                var pdfModel = ExecuteQuery<PrintDocumentQuery<Domain.Customer.Models.CustomDocument>, IExportModel>(new PrintDocumentQuery<Domain.Customer.Models.CustomDocument>
                {
                    Id = query.CustomDocumentId,
                    AddOnrampBranding = true,
                    PortalContext = PortalContext.Current,
                    userId = chk.UserId.ToString(),
                });
                pdfModel.Title = chk.UserName.Trim() + "/" + pdfModel.Title.RemoveSpecialCharacters() + ".pdf";
              //  pdfModel.Title = pdfModel.Title.RemoveSpecialCharacters() + ".pdf";
                var pdfstream = new MemoryStream();
                IReportDocumentWriter pdfPublisher = new PdfReportPublisher();
                pdfPublisher.Write(pdfModel.Document, pdfstream);
                pdfstream.Position = 0;
                var pdft = pdfstream.ToArray();
                SaveFile(zipStream, pdfModel.Title, pdft);

                #endregion


                var assignedDocument = ExecuteQuery<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = Convert.ToString(chk.UserId), DocumentId = customDocumentList.Id });

                #region ["CheckList/Activity Book"]
                if (customDocumentList != null && customDocumentList.CLContentModels.Any())
                {
                    var chapters = customDocumentList.CLContentModels;

                    foreach (var item in chapters)
                    {
                        if (assignedDocument == null)
                        {
                            if (item.Attachments.Any())
                            {
                                foreach (var attachment in item.Attachments)
                                {
                                    ZipFiles(attachment.Id, zipStream, attachment.Name);
                                }
                            }
                        }
                        else
                        {
                            var checkListChapterUserUpload = new List<UploadResultViewModel>();
                            if (assignedDocument != null)
                            {
                                checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = item.Id, IsGlobalAccessed = false });
                            }
                            else
                            {
                                checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { DocumentId = customDocumentList.CLContentModels.FirstOrDefault().Id, UserId = chk.UserId.ToString(), IsGlobalAccessed = true, CheckListChapterId = item.Id });
                            }

                            foreach (var attachment in checkListChapterUserUpload)
                            {
                                ZipFiles(attachment.Id, zipStream, attachment.Name);
                            }
                        }
                    }
                }
                #endregion

                #region ["Test"]
                if (customDocumentList != null && customDocumentList.TestContentModels.Any())
                {
                    var chapters = customDocumentList.TestContentModels;
                    foreach (var item in chapters)
                    {
                        if (assignedDocument == null)
                        {
                            if (item.Attachments.Any())
                            {
                                foreach (var attachment in item.Attachments)
                                {
                                    ZipFiles(attachment.Id, zipStream, attachment.Name);
                                }
                            }
                        }
                        else
                        {
                            var ChapterUserUpload = new List<UploadResultViewModel>();
                            if (assignedDocument != null)
                            {
                                ChapterUserUpload = ExecuteQuery<TestChapterUserResultQuery, List<UploadResultViewModel>>(new TestChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TestChapterId = item.Id, IsGlobalAccessed = false });
                            }
                            else
                            {
                                ChapterUserUpload = ExecuteQuery<TestChapterUserResultQuery, List<UploadResultViewModel>>(new TestChapterUserResultQuery { DocumentId = customDocumentList.TestContentModels.FirstOrDefault().Id, UserId = chk.UserId.ToString(), IsGlobalAccessed = true, TestChapterId = item.Id });
                            }
                            foreach (var attachment in ChapterUserUpload)
                            {
                                ZipFiles(attachment.Id, zipStream, attachment.Name);
                            }
                        }
                    }
                }
                #endregion

                #region ["Memo"]
                if (customDocumentList != null && customDocumentList.MemoContentModels.Any())
                {
                    var chapters = customDocumentList.MemoContentModels;
                    foreach (var item in chapters)
                    {
                        if (assignedDocument == null)
                        {
                            if (item.Attachments.Any())
                            {
                                foreach (var attachment in item.Attachments)
                                {
                                    ZipFiles(attachment.Id, zipStream, attachment.Name);
                                }
                            }
                        }
                        else
                        {
                            var ChapterUserUpload = new List<UploadResultViewModel>();
                            if (assignedDocument != null)
                            {
                                ChapterUserUpload = ExecuteQuery<MemoChapterUserResultQuery, List<UploadResultViewModel>>(new MemoChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, MemoChapterId = item.Id, IsGlobalAccessed = false });
                            }
                            else
                            {
                                ChapterUserUpload = ExecuteQuery<MemoChapterUserResultQuery, List<UploadResultViewModel>>(new MemoChapterUserResultQuery { DocumentId = customDocumentList.MemoContentModels.FirstOrDefault().Id, UserId = chk.UserId.ToString(), IsGlobalAccessed = true, MemoChapterId = item.Id });
                            }
                            foreach (var attachment in ChapterUserUpload)
                            {
                                ZipFiles(attachment.Id, zipStream, attachment.Name);
                            }
                        }
                    }
                }
                #endregion

                #region ["Training"]
                if (customDocumentList != null && customDocumentList.TMContentModels.Any())
                {
                    var chapters = customDocumentList.TMContentModels;
                    foreach (var item in chapters)
                    {
                        if (assignedDocument == null)
                        {
                            if (item.Attachments.Any())
                            {
                                foreach (var attachment in item.Attachments)
                                {
                                    ZipFiles(attachment.Id, zipStream, attachment.Name);
                                }
                            }
                        }
                        else
                        {
                            var ChapterUserUpload = new List<UploadResultViewModel>();
                            if (assignedDocument != null)
                            {
                                ChapterUserUpload = ExecuteQuery<TrainingManualChapterUserResultQuery, List<UploadResultViewModel>>(new TrainingManualChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TrainingManualChapterId = item.Id, IsGlobalAccessed = false });
                            }
                            else
                            {
                                ChapterUserUpload = ExecuteQuery<TrainingManualChapterUserResultQuery, List<UploadResultViewModel>>(new TrainingManualChapterUserResultQuery { DocumentId = customDocumentList.TMContentModels.FirstOrDefault().Id, UserId = chk.UserId.ToString(), IsGlobalAccessed = true, TrainingManualChapterId = item.Id });
                            }
                            foreach (var attachment in ChapterUserUpload)
                            {
                                ZipFiles(attachment.Id, zipStream, attachment.Name);
                            }
                        }
                    }
                }
                #endregion

                #region ["Policy"]
                if (customDocumentList != null && customDocumentList.PolicyContentModels.Any())
                {
                    var chapters = customDocumentList.PolicyContentModels;
                    foreach (var item in chapters)
                    {
                        if (assignedDocument == null)
                        {
                            if (item.Attachments.Any())
                            {
                                foreach (var attachment in item.Attachments)
                                {
                                    ZipFiles(attachment.Id, zipStream, attachment.Name);
                                }
                            }
                        }
                        else
                        {
                            var ChapterUserUpload = new List<UploadResultViewModel>();
                            if (assignedDocument != null)
                            {
                                ChapterUserUpload = ExecuteQuery<PolicyContentBoxUserResultQuery, List<UploadResultViewModel>>(new PolicyContentBoxUserResultQuery { AssignedDocumentId = assignedDocument.Id, PolicyContentBoxId = item.Id.ToString(), IsGlobalAccessed = false });
                            }
                            else
                            {
                                ChapterUserUpload = ExecuteQuery<PolicyContentBoxUserResultQuery, List<UploadResultViewModel>>(new PolicyContentBoxUserResultQuery { DocumentId = customDocumentList.PolicyContentModels.FirstOrDefault().Id, UserId = chk.UserId.ToString(), IsGlobalAccessed = true, PolicyContentBoxId = item.Id });
                            }
                            foreach (var attachment in ChapterUserUpload)
                            {
                                ZipFiles(attachment.Id, zipStream, attachment.Name);
                            }
                        }
                    }
                }
                #endregion

                #region ["Acrobat Field"]
                if (customDocumentList != null && customDocumentList.AcrobatFieldContentModels.Any())
                {
                    var chapters = customDocumentList.AcrobatFieldContentModels;
                    foreach (var item in chapters)
                    {
                        if (assignedDocument == null)
                        {
                            if (item.Attachments.Any())
                            {
                                foreach (var attachment in item.Attachments)
                                {
                                    ZipFiles(attachment.Id, zipStream, attachment.Name);
                                }
                            }
                        }
                        else
                        {
                            var ChapterUserUpload = new List<UploadResultViewModel>();
                            if (assignedDocument != null)
                            {
                                ChapterUserUpload = ExecuteQuery<AcrobatFieldChapterUserResultQuery, List<UploadResultViewModel>>(new AcrobatFieldChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, AcrobatFieldChapterId = item.Id, IsGlobalAccessed = false });
                            }
                            else
                            {
                                ChapterUserUpload = ExecuteQuery<AcrobatFieldChapterUserResultQuery, List<UploadResultViewModel>>(new AcrobatFieldChapterUserResultQuery { DocumentId = customDocumentList.AcrobatFieldContentModels.FirstOrDefault().Id, UserId = chk.UserId.ToString(), IsGlobalAccessed = true, AcrobatFieldChapterId = item.Id });
                            }

                            foreach (var attachment in ChapterUserUpload)
                            {
                                ZipFiles(attachment.Id, zipStream, attachment.Name);
                            }
                        }
                    }
                }
                #endregion
            }

            zipStream.IsStreamOwner = false;
            zipStream.Close();
            memoryStream.Position = 0;
            var fileName = query.ToggleFilter + ".zip";
            Response.AddHeader("filename", fileName);
            return new FileStreamResult(memoryStream, "application/octet-stream");
        }


        /// <summary>
        /// this one used to download the specific checklist with pdf and other attachments
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public ActionResult DownloadPrintExcel([FromUri] CustomDocumentSummaryExportReportQuery query)
        {

            if (!string.IsNullOrEmpty(query.CompanyId))
                PortalContext.Override(query.CompanyId.AsGuid());
            query.AddOnrampBranding = true;
            query.PortalContext = PortalContext.Current;
            query.UserId = query.UserId;

            var memoryStream = new MemoryStream();
            var zipStream = new ZipOutputStream(memoryStream);
            zipStream.SetLevel(3);

            var customDocumentList = ExecuteQuery<FetchByIdQuery, CustomDocumentModel>(new FetchByIdQuery { Id = query.CustomDocumentID, userId=query.UserId.ToString() });
            query.IsChecklistTracked = customDocumentList.IsChecklistTracked;

            #region for xls checklist

            var model = ExecuteQuery<CustomDocumentSummaryExportReportQuery, IExportModel>(query);

            model.Title = customDocumentList.Title.Trim().Replace("/", "-") + ".xls";
            var stream = new MemoryStream();
            IReportDocumentWriter publisher = new ExcelReportPublisher();
            publisher.Write(model.Document, stream);
            stream.Position = 0;
            var t = stream.ToArray();
            SaveFile(zipStream, model.Title, t);
            #endregion

            #region ["PDF"]

            var pdfModel = ExecuteQuery<PrintDocumentQuery<Domain.Customer.Models.CustomDocument>, IExportModel>(new PrintDocumentQuery<Domain.Customer.Models.CustomDocument>
            {
                Id = query.CustomDocumentID,
                AddOnrampBranding = true,
                PortalContext = PortalContext.Current,
                userId = query.UserId.ToString(),
            });
            pdfModel.Title = pdfModel.Title.RemoveSpecialCharacters() + ".pdf";
            var pdfstream = new MemoryStream();
            IReportDocumentWriter pdfPublisher = new PdfReportPublisher();
            pdfPublisher.Write(pdfModel.Document, pdfstream);
            pdfstream.Position = 0;
            var pdft = pdfstream.ToArray();
            SaveFile(zipStream, pdfModel.Title, pdft);

            #endregion


            var assignedDocument = ExecuteQuery<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = Convert.ToString(query.UserId), DocumentId = customDocumentList.Id });

            #region ["CheckList/Activity Book"]
            if (customDocumentList != null && customDocumentList.CLContentModels.Any())
            {
                var chapters = customDocumentList.CLContentModels;

                foreach (var item in chapters)
                {
                    if (assignedDocument == null)
                    {
                        if (item.Attachments.Any())
                        {
                            foreach (var attachment in item.Attachments)
                            {
                                ZipFiles(attachment.Id, zipStream, attachment.Name);
                            }
                        }
                    }
                    else
                    {
                        var checkListChapterUserUpload = new List<UploadResultViewModel>();
                        if (assignedDocument != null)
                        {
                            checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = item.Id, IsGlobalAccessed = false });
                        }
                        else
                        {
                            checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { DocumentId = customDocumentList.CLContentModels.FirstOrDefault().Id, UserId = query.UserId.ToString(), IsGlobalAccessed = true, CheckListChapterId = item.Id });
                        }

                        foreach (var attachment in checkListChapterUserUpload)
                        {
                            ZipFiles(attachment.Id, zipStream, attachment.Name);
                        }
                    }
                }
            }
            #endregion

            #region ["Test"]
            if (customDocumentList != null && customDocumentList.TestContentModels.Any())
            {
                var chapters = customDocumentList.TestContentModels;
                foreach (var item in chapters)
                {
                    if (assignedDocument == null)
                    {
                        if (item.Attachments.Any())
                        {
                            foreach (var attachment in item.Attachments)
                            {
                                ZipFiles(attachment.Id, zipStream, attachment.Name);
                            }
                        }
                    }
                    else
                    {
                        var ChapterUserUpload = new List<UploadResultViewModel>();
                        if (assignedDocument != null)
                        {
                            ChapterUserUpload = ExecuteQuery<TestChapterUserResultQuery, List<UploadResultViewModel>>(new TestChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TestChapterId = item.Id, IsGlobalAccessed = false });
                        }
                        else
                        {
                            ChapterUserUpload = ExecuteQuery<TestChapterUserResultQuery, List<UploadResultViewModel>>(new TestChapterUserResultQuery { DocumentId = customDocumentList.TestContentModels.FirstOrDefault().Id, UserId = query.UserId.ToString(), IsGlobalAccessed = true, TestChapterId = item.Id });
                        }
                        foreach (var attachment in ChapterUserUpload)
                        {
                            ZipFiles(attachment.Id, zipStream, attachment.Name);
                        }
                    }
                }
            }
            #endregion

            #region ["Memo"]
            if (customDocumentList != null && customDocumentList.MemoContentModels.Any())
            {
                var chapters = customDocumentList.MemoContentModels;
                foreach (var item in chapters)
                {
                    if (assignedDocument == null)
                    {
                        if (item.Attachments.Any())
                        {
                            foreach (var attachment in item.Attachments)
                            {
                                ZipFiles(attachment.Id, zipStream, attachment.Name);
                            }
                        }
                    }
                    else
                    {
                        var ChapterUserUpload = new List<UploadResultViewModel>();
                        if (assignedDocument != null)
                        {
                            ChapterUserUpload = ExecuteQuery<MemoChapterUserResultQuery, List<UploadResultViewModel>>(new MemoChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, MemoChapterId = item.Id, IsGlobalAccessed = false });
                        }
                        else
                        {
                            ChapterUserUpload = ExecuteQuery<MemoChapterUserResultQuery, List<UploadResultViewModel>>(new MemoChapterUserResultQuery { DocumentId = customDocumentList.MemoContentModels.FirstOrDefault().Id, UserId = query.UserId.ToString(), IsGlobalAccessed = true, MemoChapterId = item.Id });
                        }
                        foreach (var attachment in ChapterUserUpload)
                        {
                            ZipFiles(attachment.Id, zipStream, attachment.Name);
                        }
                    }
                }
            }
            #endregion

            #region ["Training"]
            if (customDocumentList != null && customDocumentList.TMContentModels.Any())
            {
                var chapters = customDocumentList.TMContentModels;
                foreach (var item in chapters)
                {
                    if (assignedDocument == null)
                    {
                        if (item.Attachments.Any())
                        {
                            foreach (var attachment in item.Attachments)
                            {
                                ZipFiles(attachment.Id, zipStream, attachment.Name);
                            }
                        }
                    }
                    else
                    {
                        var ChapterUserUpload = new List<UploadResultViewModel>();
                        if (assignedDocument != null)
                        {
                            ChapterUserUpload = ExecuteQuery<TrainingManualChapterUserResultQuery, List<UploadResultViewModel>>(new TrainingManualChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TrainingManualChapterId = item.Id, IsGlobalAccessed = false });
                        }
                        else
                        {
                            ChapterUserUpload = ExecuteQuery<TrainingManualChapterUserResultQuery, List<UploadResultViewModel>>(new TrainingManualChapterUserResultQuery { DocumentId = customDocumentList.TMContentModels.FirstOrDefault().Id, UserId = query.UserId.ToString(), IsGlobalAccessed = true, TrainingManualChapterId = item.Id });
                        }
                        foreach (var attachment in ChapterUserUpload)
                        {
                            ZipFiles(attachment.Id, zipStream, attachment.Name);
                        }
                    }
                }
            }
            #endregion

            #region ["Policy"]
            if (customDocumentList != null && customDocumentList.PolicyContentModels.Any())
            {
                var chapters = customDocumentList.PolicyContentModels;
                foreach (var item in chapters)
                {
                    if (assignedDocument == null)
                    {
                        if (item.Attachments.Any())
                        {
                            foreach (var attachment in item.Attachments)
                            {
                                ZipFiles(attachment.Id, zipStream, attachment.Name);
                            }
                        }
                    }
                    else
                    {
                        var ChapterUserUpload = new List<UploadResultViewModel>();
                        if (assignedDocument != null)
                        {
                            ChapterUserUpload = ExecuteQuery<PolicyContentBoxUserResultQuery, List<UploadResultViewModel>>(new PolicyContentBoxUserResultQuery { AssignedDocumentId = assignedDocument.Id, PolicyContentBoxId = item.Id.ToString(), IsGlobalAccessed = false });
                        }
                        else
                        {
                            ChapterUserUpload = ExecuteQuery<PolicyContentBoxUserResultQuery, List<UploadResultViewModel>>(new PolicyContentBoxUserResultQuery { DocumentId = customDocumentList.PolicyContentModels.FirstOrDefault().Id, UserId = query.UserId.ToString(), IsGlobalAccessed = true, PolicyContentBoxId = item.Id });
                        }
                        foreach (var attachment in ChapterUserUpload)
                        {
                            ZipFiles(attachment.Id, zipStream, attachment.Name);
                        }
                    }
                }
            }
            #endregion

            #region ["Acrobat Field"]
            if (customDocumentList != null && customDocumentList.AcrobatFieldContentModels.Any())
            {
                var chapters = customDocumentList.AcrobatFieldContentModels;
                foreach (var item in chapters)
                {
                    if (assignedDocument == null)
                    {
                        if (item.Attachments.Any())
                        {
                            foreach (var attachment in item.Attachments)
                            {
                                ZipFiles(attachment.Id, zipStream, attachment.Name);
                            }
                        }
                    }
                    else
                    {
                        var ChapterUserUpload = new List<UploadResultViewModel>();
                        if (assignedDocument != null)
                        {
                            ChapterUserUpload = ExecuteQuery<AcrobatFieldChapterUserResultQuery, List<UploadResultViewModel>>(new AcrobatFieldChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, AcrobatFieldChapterId = item.Id, IsGlobalAccessed = false });
                        }
                        else
                        {
                            ChapterUserUpload = ExecuteQuery<AcrobatFieldChapterUserResultQuery, List<UploadResultViewModel>>(new AcrobatFieldChapterUserResultQuery { DocumentId = customDocumentList.AcrobatFieldContentModels.FirstOrDefault().Id, UserId = query.UserId.ToString(), IsGlobalAccessed = true, AcrobatFieldChapterId = item.Id });
                        }

                        foreach (var attachment in ChapterUserUpload)
                        {
                            ZipFiles(attachment.Id, zipStream, attachment.Name);
                        }
                    }
                }
            }
            #endregion

            zipStream.IsStreamOwner = false;
            zipStream.Close();
            memoryStream.Position = 0;
            var fileName = customDocumentList.Title + ".zip";
            Response.AddHeader("filename", fileName);
            return new FileStreamResult(memoryStream, "application/octet-stream");
        }

        #region written by ashok to save file in zip and download zip
        public void ZipFiles(string id, ZipOutputStream zipStream, string name)
        {
            var upload = ExecuteQuery<FetchUploadByIdQuery, Upload>(new FetchUploadByIdQuery { Id = id });

            if (upload != null && upload.Data.Any())
            {
                SaveFile(zipStream, name, upload.Data);
            }

        }

        public void SaveFile(ZipOutputStream zipStream, string name, byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var attachmentEntry = new ZipEntry(ZipEntry.CleanName(name))
                {
                    Size = stream.Length
                };
                zipStream.PutNextEntry(attachmentEntry);
                byte[] buffer = new byte[4096];
                int count = stream.Read(buffer, 0, buffer.Length);
                while (count > 0)
                {
                    zipStream.Write(buffer, 0, count);
                    count = stream.Read(buffer, 0, buffer.Length);
                    if (!Response.IsClientConnected)
                    {
                        break;
                    }
                }
                zipStream.CloseEntry();
            };
        }
        #endregion


        /// <summary>
        /// this is used to download the all user checklists reports under one zip
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>     
        //[System.Web.Mvc.HttpGet]
        //public ActionResult ZipCheckList(CustomDocumentSubmissionReportQuery query)
        //{

        //    var portalContext = PortalContext.Current;
        //    var fromDate = query.FromDate;
        //    var toDate = query.ToDate;

        //    string folderPath = Server.MapPath(ConfigurationManager.AppSettings["downloadPath"] + ConfigurationManager.AppSettings["DocPath"]);


        //    var checkLists = ExecuteQuery<CustomDocumentSubmissionReportQuery, IEnumerable<CustomDocumentInteractionModel>>(query);

        //    var docList = new List<string>();

        //    foreach (var checklist in checkLists)
        //    {

        //        foreach (var item in checklist.Chec)
        //        {
        //            var checkListQuery = new UserActivityAndPerformanceReportExportQuery
        //            {
        //                UserId = item.UserId,
        //                FromDate = fromDate,
        //                ToDate = toDate,
        //                PortalContext = portalContext,
        //            };
        //            var model = ExecuteQuery<UserActivityAndPerformanceReportExportQuery, IExportModel>(checkListQuery);

        //            model.Title = item.UserName + ".xls";
        //            var stream = new MemoryStream();
        //            IReportDocumentWriter publisher = new ExcelReportPublisher();
        //            publisher.Write(model.Document, stream);
        //            stream.Position = 0;

        //            if (!Directory.Exists(folderPath))
        //            {
        //                Directory.CreateDirectory(folderPath);
        //            }

        //            FileStream file = new FileStream(Path.Combine(folderPath, model.Title), FileMode.Create, FileAccess.Write);
        //            stream.WriteTo(file);
        //            file.Close();
        //            stream.Close();

        //            docList.Add(folderPath + model.Title);
        //        }
        //    }

        //    try
        //    {

        //        var fileName = string.Format($"CheckList Submission Report.zip");

        //        string tempOutPutPath = folderPath + fileName;

        //        using (ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(tempOutPutPath)))
        //        {
        //            s.SetLevel(9); // 0-9, 9 being the highest compression  

        //            byte[] buffer = new byte[4096];

        //            for (int i = 0; i < docList.Count; i++)
        //            {

        //                ZipEntry entry = new ZipEntry(Path.GetFileName(docList[i]))
        //                {
        //                    DateTime = DateTime.Now,
        //                    IsUnicodeText = true
        //                };
        //                s.PutNextEntry(entry);

        //                using (FileStream fs = System.IO.File.OpenRead(docList[i]))
        //                {
        //                    int sourceBytes;
        //                    do
        //                    {
        //                        sourceBytes = fs.Read(buffer, 0, buffer.Length);
        //                        s.Write(buffer, 0, sourceBytes);
        //                    } while (sourceBytes > 0);
        //                }
        //            }
        //            s.Finish();
        //            s.Flush();
        //            s.Close();

        //        }

        //        byte[] finalResult = System.IO.File.ReadAllBytes(tempOutPutPath);
        //        if (System.IO.File.Exists(tempOutPutPath))
        //            System.IO.File.Delete(tempOutPutPath);

        //        if (finalResult == null || !finalResult.Any())
        //            throw new Exception(string.Format("No Files found with pdf"));

        //        Response.AddHeader("filename", fileName);

        //        return File(finalResult, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.AllowAnonymous]
        public void DownloadSummary_EXCEL(string Occurance)
        {
            var getAllReport = ExecuteQuery<FetchAllScheduleReportQuery, List<ScheduleReportModel>>(new FetchAllScheduleReportQuery { }).Where(z => z.Occurences == Occurance && z.ReportAssignedId == "5").ToList();
            foreach (var d in getAllReport)
            {
                var data = ExecuteQuery<FetchByIdQuery, ScheduleReportVM>(new FetchByIdQuery
                {
                    Id = d.Id
                });
                var lstActivity = ReportParam.ReturnParams(data.Params, "CustomDocument");
                CustomDocumentSubmissionReportQuery query = new CustomDocumentSubmissionReportQuery
                {
                    ToggleFilter = "User Name,Viewed,Date Assigned,Date Viewed,Checks Completed,Date Submitted,Status,Access,Group",

                    CustomDocumentIds = lstActivity,
                    CustomDocumentId = lstActivity[0],
                    ScheduleName = data.ScheduleName,
                    Recepients = data.RecipientsList,
                    //  FromDate = ExtensionMethods.FirstDayOfMonth_NewMethod(DateTime.Now.Date),
                    //  ToDate = ExtensionMethods.LastDayOfMonth_NewMethod(DateTime.Now.Date),
                };

                DownloadSummaryEXCEL(query);
            }
        }


        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult DownloadSummaryEXCEL(CustomDocumentSubmissionReportQuery query)
        {
            query.PortalContext = PortalContext.Current;
            CustomDocumentScheduleReportQuery exportQuery = new CustomDocumentScheduleReportQuery()
            {
                PortalContext = query.PortalContext,
                CustomDocumentSubmissionReportQuery = query,

            };
            exportQuery.Recepients = query.Recepients;
            return Publish(ExecuteQuery<CustomDocumentScheduleReportQuery, IExportModel>(exportQuery), new ExcelReportPublisher(), "application/vnd.ms-excel", "xls");
        }

        //public void ExportSummaryEXCEL(CustomDocumentSubmissionReportQuery query) {
        //		query.PortalContext = PortalContext.Current;
        //		ChecklistSummaryExportReportQuery exportQuery = new ChecklistSummaryExportReportQuery() {
        //			PortalContext=query.PortalContext,
        //			CustomDocumentSubmissionReportQuery=query
        //		};
        //		Publish(ExecuteQuery<ChecklistSummaryExportReportQuery, IExportModel>(exportQuery), new ExcelReportPublisher(), "application/vnd.ms-excel", "xls");
        //	}

        public ActionResult Detail([FromUri] AcrobetFieldDetailsQuery query)
        {
            query.DocumentType = DocumentType.custom;
            var model = ExecuteQuery<AcrobetFieldDetailsQuery, AcrobatFieldDetailsViewModel>(query);

            return View(model);
        }

    }


}