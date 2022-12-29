using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Common.Query;
using Common.Report;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Test;
using ICSharpCode.SharpZipLib.Zip;
using Ramp.Contracts.Query.Bundle;
using Ramp.Contracts.Query.CheckList;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Group;
using Ramp.Contracts.Query.Label;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.Query.Upload;
using Ramp.Contracts.Query.User;
using Ramp.Contracts.QueryParameter.Account;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.QueryParameter.Upload;
using Domain.Customer.Models.ScheduleReport;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using Ramp.Services.Helpers;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Publishers;
using Web.UI.Code.Extensions;
using Web.UI.Controllers;

using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.Query.Report;

namespace Web.UI.Areas.Reporting.Controllers
{
    public class UserActivityAndPerformanceReportController : ExportController<UserActivityAndPerformanceReportExportQuery>
    {
        // GET: Reporting/UserActivityAndPerformanceReport
        //readonly ITransientRepository<Test_Result> _testResultRepository;
        //readonly ITransientRepository<CheckListChapterUserUploadResult> _checkListChapterUserUploadResultRepository;
        //public UserActivityAndPerformanceReportController(ITransientRepository<Test_Result> repository, ITransientRepository<CheckListChapterUserUploadResult> checkListChapterUserUploadResultRepository)
        //{
        //	_testResultRepository = repository;
        //	_checkListChapterUserUploadResultRepository = checkListChapterUserUploadResultRepository;
        //}
        public ActionResult Index(string id = null)
        {

            var customerCompanyUserQueryParameter = new CustomerCompanyUserQueryParameter
            {
                CompanyId = PortalContext.Current.UserCompany.Id,
                CompanyName = PortalContext.Current.UserCompany.CompanyName,
                UserId = Guid.Empty,
                LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
            };
            CompanyUserViewModel companyUserViewModel =
                ExecuteQuery<CustomerCompanyUserQueryParameter, CompanyUserViewModel>(customerCompanyUserQueryParameter);
            var datas = companyUserViewModel.UserList.Where(z => z.Department != null).Select(z => z.Department).Distinct().ToList();
            ViewBag.Departments = datas;

            if (Thread.CurrentPrincipal.IsInStandardUserRole() && (Thread.CurrentPrincipal.IsInStandardUserRole() && !Thread.CurrentPrincipal.IsInAdminRole()) && (string.IsNullOrEmpty(id) || id != Thread.CurrentPrincipal.GetId().ToString()))
            {
                return RedirectToAction("AccessDenied", "Account", new { Area = "" });
            }

            ViewBag.Id = id;
            ViewBag.Users = ExecuteQuery<StandardUsersQuery, IEnumerable<UserModelShort>>(new StandardUsersQuery()).Where(x => !string.IsNullOrWhiteSpace(x.Name)).Select(x => new UserData
            {
                Value = x.Name,
                Extra = x.Email,
                Id = x.Id.ToString()
            }).ToList();

            //var groups = new List<GroupViewModelShort>();
            //groups.AddRange(ExecuteQuery<GroupsWithUsersQuery, IEnumerable<GroupViewModelShort>>(new GroupsWithUsersQuery()));
            //ViewBag.Groups = groups;

            ViewBag.Groups = ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(
                new AllGroupsByCustomerAdminQueryParameter
                {
                    CompanyId = PortalContext.Current.UserCompany.Id
                }).Select(g => new GroupViewModelShort
                {
                    Id = g.GroupId,
                    Name = g.Title,
                    Selected = false
                }).OrderBy(x => x.Name);

            var labels = ExecuteQuery<TrainingLabelListQuery, IEnumerable<TrainingLabelListModel>>(new TrainingLabelListQuery());
            ViewBag.Labels = labels;
            return View();
        }
        [HttpGet]
        public ActionResult ExportIndexToExcel(UserActivityAndPerformanceUserListReportQuery query)
        {

            query.PortalContext = PortalContext.Current;
            return Publish(ExecuteQuery<UserActivityAndPerformanceUserListReportQuery, IExportModel>(query), new ExcelReportPublisher(), "application/vnd.ms-excel", "xls");
        }
        [HttpPost]
        public JsonResult GetUserDetails(UserActivityAndPerformanceUserListReportQuery query)
        {
            var users = ExecuteQuery<UserActivityAndPerformanceUserListReportQuery, IEnumerable<UserViewModel>>(query);

            List<UserViewModel> newUsers = new List<UserViewModel>();

            foreach (var user in users.ToList())
            {

                var standardUserGroups = ExecuteQuery<StandardUserGroupByUserIdQuery, StandardUserGroupViewModel>(new StandardUserGroupByUserIdQuery() { UserId = user.Id.ToString() });

                if (standardUserGroups != null)
                {
                    newUsers.Add(user);
                }
            }

            if (users.Any())
                return new JsonResult
                {
                    Data = (newUsers),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength =Int32.MaxValue,
                    ContentType = "application/json"

                };
            return new JsonResult { Data = true, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [HttpGet]
        public ActionResult IndividualReport(string Tags, string UserId, DateTime? FromDate, DateTime? ToDate)
        {

            ViewData["UserId"] = UserId;

            var data = ExecuteQuery<UserActivityAndPerformanceReportQuery, UserActivityAndPerformanceViewModel>(
              new UserActivityAndPerformanceReportQuery
              {
                  UserId = UserId,
                  FromDate = FromDate,
                  ToDate = ToDate,
                  Tags = Tags,
                  PortalContext = PortalContext.Current,
                  IsChecklistEnable = PortalContext.Current.UserCompany.EnableChecklistDocument,
                  EnableGlobalAccessDocuments = PortalContext.Current.UserCompany.EnableGlobalAccessDocuments
              });
            if (!PortalContext.Current.UserCompany.EnableChecklistDocument)
            {
                data.Feedback = data.Feedback.Where(c => c.DocumentType != "Checklist");
                data.PointsStatement = data.PointsStatement.Where(c => c.Type != "Checklist").ToList();
            }

            return View(data);
        }
        public override ActionResult Zip([System.Web.Http.FromUri] UserActivityAndPerformanceReportExportQuery query)
        {
            throw new NotImplementedException();
        }

        #region added by ashok
        /// <summary>
        /// this is used to download the all user activity performance reports under one zip
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        [AllowAnonymous]
        public void ScheduleUserActivity(string Occurance)
        {
            var getAllReport = ExecuteQuery<FetchAllScheduleReportQuery, List<ScheduleReportModel>>(new FetchAllScheduleReportQuery { }).Where(z => z.Occurences == Occurance && z.ReportAssignedId == "5").ToList();
            foreach (var d in getAllReport)
            {
                var data = ExecuteQuery<FetchByIdQuery, ScheduleReportVM>(new FetchByIdQuery
                {
                    Id = d.Id
                });

                UserActivityAndPerformanceUserListReportQuery query = new UserActivityAndPerformanceUserListReportQuery
                {
                    FromDate = ReportParam.FromDate,
                    ToDate = ReportParam.ToDate,
                    UserIds = ReportParam.ReturnParams(data.Params, "Users"),
                    GroupIds = ReportParam.ReturnParams(data.Params, "Groups"),
                    ScheduleName = data.ScheduleName,
                    Recepients = data.RecipientsList
                };

                ZipUAP(query);
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult ZipUAP(UserActivityAndPerformanceUserListReportQuery query)
        {

            var portalContext = PortalContext.Current;

            var userIds = query.UserIds;
            var fromDate = query.FromDate;
            var toDate = query.ToDate;
            string folderPath = ConfigurationManager.AppSettings["downloadPath"] + ConfigurationManager.AppSettings["DocPath"];

            var users = ExecuteQuery<UserActivityAndPerformanceUserListReportQuery, IEnumerable<UserViewModel>>(query).Select(c => c.Id).ToList();

            var memoryStream = new MemoryStream();
            var zipStream = new ZipOutputStream(memoryStream);
            zipStream.SetLevel(9);

            var docList = new List<string>();
            string userName = "";
            foreach (var item in users)
            {

                var testQuery = new UserActivityAndPerformanceReportExportQuery
                {
                    UserId = item.ToString(),
                    FromDate = fromDate.Value.Date,
                    ToDate = toDate,
                    PortalContext = portalContext,
                    ToggleFilter = query.ToggleFilter
                };
                List<IExportModel> excelExports = new List<IExportModel>();
                List<IExportModel> pdfExports = new List<IExportModel>();

                var data = ExecuteQuery<UserActivityAndPerformanceReportQuery, UserActivityAndPerformanceViewModel>(
                  new UserActivityAndPerformanceReportQuery
                  {
                      UserId = item.ToString(),
                      FromDate = fromDate,
                      ToDate = toDate,
                      PortalContext = portalContext,
                      IsChecklistEnable = PortalContext.Current.UserCompany.EnableChecklistDocument,
                      EnableGlobalAccessDocuments = PortalContext.Current.UserCompany.EnableGlobalAccessDocuments
                  });

                var userInfo = ExecuteQuery<UserProfileQuery, EditUserProfileViewModel>(new UserProfileQuery() { UserId = item });
                userName = userInfo.FullName.Trim();

                var allCheckList = data.CheckLisInteractions.Concat(data.CheckListGlobalInteractions);
                var allTestList = data.TestResultList.Concat(data.TestResultGlobalList);

                //var allMemoList = data.MemoInteractions.Concat(data.MemoGlobalInteractions);
                //var allPolicyList = data.PolicyInteractions.Concat(data.PolicyGlobalInteractions);
                //var allTrainingManualList = data.TrainingManualInteractions.Concat(data.TrainingManualGlobalInteractions);

                var model = ExecuteQuery<UserActivityAndPerformanceReportExportQuery, IExportModel>(testQuery);

                model.Title = userInfo.FullName.Trim() + "/" + userInfo.FullName.Trim() + ".xls";
                excelExports.Add(model);

                foreach (var checklist in allCheckList)
                {
                    var chklist = ExecuteQuery<FetchByIdQuery, CheckListModel>(new FetchByIdQuery() { Id = checklist.Id });
                    foreach (var chapter in chklist.ContentModels)
                    {
                        if (chapter.Attachments.Any())
                        {
                            foreach (var attachment in chapter.Attachments)
                            {
                                var newTitle = checklist.DocumentTitle.Trim().Replace("/", " ");
                                var title = userInfo.FullName.Trim() + "/Checklist/" + newTitle + "/" + attachment.Name;
                                ZipFile(attachment.Id, zipStream, title);
                            }
                        }
                        //	var userUploads = _checkListChapterUserUploadResultRepository.List.Where(x => x.CheckListChapterId == chapter.Id && x.UserId == item.ToString()).ToList();
                        var userUploads = ExecuteQuery<FetchUserQuery, List<CheckListChapterUserUploadResult>>(new FetchUserQuery
                        {
                            Id = chapter.Id,
                            UserId = item
                        });
                        if (userUploads != null & userUploads.Any())
                        {
                            foreach (var attachment in userUploads)
                            {
                                var newTitle = checklist.DocumentTitle.Trim().Replace("/", " ");
                                var title = userInfo.FullName.Trim() + "/Checklist/" + newTitle + "/" + attachment.Upload.Name;
                                ZipFile(attachment.Upload.Id, zipStream, title);
                            }
                        }

                    }
                    var checklistModel = ExecuteQuery<CheckListExportReportQuery, IExportModel>(new CheckListExportReportQuery()
                    {
                        PortalContext = portalContext,
                        UserId = item,
                        ResultId = checklist.Id
                    });
                    var docTitle = checklist.DocumentTitle.Trim().Replace("/", " ");
                    checklistModel.Title = userInfo.FullName.Trim() + "/Checklist/" + docTitle + "/" + docTitle + ".xls";
                    excelExports.Add(checklistModel);

                }

                var Createdfrom = fromDate.Value.Date;
                var Createdto = toDate;
                //	var test_Results_list = _testResultRepository.List.AsQueryable().Where(c => c.UserId == item.ToString()).ToList();
                var test_Results_list = ExecuteQuery<FetchUserQuery, List<Test_Result>>(new FetchUserQuery()
                {
                    UserId = item

                });
                var test_Results = test_Results_list.Where(c => c.Created >= Createdfrom && c.Created <= Createdto);

                foreach (var result in test_Results)
                {
                    var test_title = result.Test.Title.Replace("/", " ");

                    if (result.Certificate != null && result.CertificateId != null)
                    {
                        var upload = ExecuteQuery<FetchUploadQueryParameter, UploadModel>(new FetchUploadQueryParameter
                        {
                            Id = result.Certificate.Id ?? result.CertificateId
                        });
                        Response.Clear();
                        MemoryStream ms = new MemoryStream(upload.Data);
                        string filename = upload.Name;
                        ms.WriteTo(Response.OutputStream);
                        var certificates = userInfo.FullName.Trim() + "/Test/" + test_title + "/certificate_" + test_title + ".pdf";
                        SaveFile(zipStream, certificates, ms.ToArray());
                    }
                    var testResult = ExecuteQuery<TestExportReportQuery, IExportModel>(new TestExportReportQuery
                    {
                        AddOnrampBranding = true,
                        PortalContext = portalContext,
                        ResultId = result.Id
                    });
                    testResult.Title = userInfo.FullName.Trim() + "/Test/" + test_title + "/" + test_title + ".pdf";
                    pdfExports.Add(testResult);
                }


                #region To download zip
                foreach (var excelModel in excelExports)
                {
                    var stream = new MemoryStream();
                    IReportDocumentWriter excelPublisher = new ExcelReportPublisher();
                    excelPublisher.Write(excelModel.Document, stream);
                    stream.Position = 0;

                    var t = stream.ToArray();
                    SaveFile(zipStream, excelModel.Title, t);
                }
                foreach (var pdfModel in pdfExports)
                {
                    var stream = new MemoryStream();
                    IReportDocumentWriter pdfPublisher = new PdfReportPublisher();
                    pdfPublisher.Write(pdfModel.Document, stream);
                    stream.Position = 0;

                    var t = stream.ToArray();
                    SaveFile(zipStream, pdfModel.Title, t);
                }
            }
            zipStream.IsStreamOwner = false;
            zipStream.Close();

            memoryStream.Position = 0;
            string filePaths = null;
            var fileName = "UserActivityAndPerformanceReport" + ".zip";
            if (query.Recepients != null)
            {
                if (PortalContext.Current != null)
                {
                    Response.AddHeader("filename", fileName);

                    filePaths = Server.MapPath(Path.Combine("~/Download/", fileName));
                    System.IO.File.WriteAllBytes(filePaths, memoryStream.ToArray());
                }
                new SendEmail().addAttachmentSendEmail(memoryStream, query.Recepients, fileName, "application/octet-stream", filePaths);

                if (!string.IsNullOrEmpty(filePaths))
                {
                    FileInfo file = new FileInfo(filePaths);

                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }
            }
            return new FileStreamResult(memoryStream, "application/octet-stream");
            #endregion

        }

        [HttpGet]
        public ActionResult UserZipUAP(UserActivityAndPerformanceUserListReportQuery query)
        {
            var portalContext = PortalContext.Current;

            var userIds = query.UserIds;
            var fromDate = query.FromDate;
            var toDate = query.ToDate;
            string folderPath = ConfigurationManager.AppSettings["downloadPath"] + ConfigurationManager.AppSettings["DocPath"];

            var users = ExecuteQuery<UserActivityAndPerformanceUserListReportQuery, IEnumerable<UserViewModel>>(query).Select(c => c.Id).ToList();

            var memoryStream = new MemoryStream();
            var zipStream = new ZipOutputStream(memoryStream);
            zipStream.SetLevel(9);

            string userName = "";
            foreach (var item in users)
            {

                var testQuery = new UserActivityAndPerformanceReportExportQuery
                {
                    UserId = item.ToString(),
                    FromDate = fromDate,
                    ToDate = toDate,
                    PortalContext = portalContext,
                    ToggleFilter = query.ToggleFilter
                };
                List<IExportModel> excelExports = new List<IExportModel>();
                List<IExportModel> pdfExports = new List<IExportModel>();

                var data = ExecuteQuery<UserActivityAndPerformanceReportQuery, UserActivityAndPerformanceViewModel>(
                  new UserActivityAndPerformanceReportQuery
                  {
                      UserId = item.ToString(),
                      FromDate = fromDate,
                      ToDate = toDate,
                      PortalContext = portalContext,
                      IsChecklistEnable = PortalContext.Current.UserCompany.EnableChecklistDocument,
                      EnableGlobalAccessDocuments = PortalContext.Current.UserCompany.EnableGlobalAccessDocuments
                  });

                var userInfo = ExecuteQuery<UserProfileQuery, EditUserProfileViewModel>(new UserProfileQuery() { UserId = item });
                userName = userInfo.FullName.Trim();

                var allCheckList = data.CheckLisInteractions.Concat(data.CheckListGlobalInteractions);
                var allTestList = data.TestResultList.Concat(data.TestResultGlobalList);

                var model = ExecuteQuery<UserActivityAndPerformanceReportExportQuery, IExportModel>(testQuery);

                model.Title = userInfo.FullName.Trim() + ".xls";
                excelExports.Add(model);

                foreach (var checklist in allCheckList)
                {
                    var chklist = ExecuteQuery<FetchByIdQuery, CheckListModel>(new FetchByIdQuery() { Id = checklist.Id });
                    foreach (var chapter in chklist.ContentModels)
                    {
                        if (chapter.Attachments.Any())
                        {
                            foreach (var attachment in chapter.Attachments)
                            {
                                var newTitle = checklist.DocumentTitle.Trim().Replace("/", " ");
                                var title = "Checklist/" + newTitle + "/" + attachment.Name;
                                ZipFile(attachment.Id, zipStream, title);
                            }
                        }
                        //	var userUploads = _checkListChapterUserUploadResultRepository.List.Where(x => x.CheckListChapterId == chapter.Id && x.UserId == item.ToString()).ToList();
                        var userUploads = ExecuteQuery<FetchUserQuery, List<CheckListChapterUserUploadResult>>(new FetchUserQuery
                        {
                            Id = chapter.Id,
                            UserId = item
                        });
                        if (userUploads != null & userUploads.Any())
                        {
                            foreach (var attachment in userUploads)
                            {
                                var newTitle = checklist.DocumentTitle.Trim().Replace("/", " ");
                                var title = "Checklist/" + newTitle + "/" + attachment.Upload.Name;
                                ZipFile(attachment.Upload.Id, zipStream, title);
                            }
                        }

                    }
                    var checklistModel = ExecuteQuery<CheckListExportReportQuery, IExportModel>(new CheckListExportReportQuery()
                    {
                        PortalContext = portalContext,
                        UserId = item,
                        ResultId = checklist.Id
                    });
                    var docTitle = checklist.DocumentTitle.Trim().Replace("/", " ");
                    checklistModel.Title = "Checklist/" + docTitle + "/" + docTitle + ".xls";
                    excelExports.Add(checklistModel);
                }

                var Createdfrom = fromDate.Value.Date;
                var Createdto = toDate;
                //	var test_Results_list = _testResultRepository.List.AsQueryable().Where(c => c.UserId == item.ToString()).ToList();
                var test_Results_list = ExecuteQuery<FetchUserQuery, List<Test_Result>>(new FetchUserQuery()
                {
                    UserId = item

                }); var test_Results = test_Results_list.Where(c => c.Created >= Createdfrom && c.Created <= Createdto);

                foreach (var result in test_Results)
                {
                    var test_title = result.Test.Title.Replace("/", " ");

                    if (result.Certificate != null && result.CertificateId != null)
                    {
                        var upload = ExecuteQuery<FetchUploadQueryParameter, UploadModel>(new FetchUploadQueryParameter
                        {
                            Id = result.Certificate.Id ?? result.CertificateId
                        });
                        Response.Clear();
                        MemoryStream ms = new MemoryStream(upload.Data);
                        string filename = upload.Name;
                        ms.WriteTo(Response.OutputStream);
                        var certificates = "Test/" + test_title + "/certificate_" + test_title + ".pdf";
                        SaveFile(zipStream, certificates, ms.ToArray());
                    }
                    var testResult = ExecuteQuery<TestExportReportQuery, IExportModel>(new TestExportReportQuery
                    {
                        AddOnrampBranding = true,
                        PortalContext = portalContext,
                        ResultId = result.Id
                    });
                    testResult.Title = "Test/" + test_title + "/" + test_title + ".pdf";
                    pdfExports.Add(testResult);
                }


                foreach (var excelModel in excelExports)
                {
                    var stream = new MemoryStream();
                    IReportDocumentWriter excelPublisher = new ExcelReportPublisher();
                    excelPublisher.Write(excelModel.Document, stream);
                    stream.Position = 0;

                    var t = stream.ToArray();
                    SaveFile(zipStream, excelModel.Title, t);
                }
                foreach (var pdfModel in pdfExports)
                {
                    var stream = new MemoryStream();
                    IReportDocumentWriter pdfPublisher = new PdfReportPublisher();
                    pdfPublisher.Write(pdfModel.Document, stream);
                    stream.Position = 0;

                    var t = stream.ToArray();
                    SaveFile(zipStream, pdfModel.Title, t);
                }
            }
            zipStream.IsStreamOwner = false;
            zipStream.Close();
            memoryStream.Position = 0;
            var fileName = userName + ".zip";
            Response.AddHeader("filename", fileName);

            var filePaths = Server.MapPath(Path.Combine("~/Download/", fileName));
            System.IO.File.WriteAllBytes(filePaths, memoryStream.ToArray());
            new SendEmail().addAttachmentSendEmail(memoryStream, query.Recepients, fileName, "application/octet-stream", filePaths);

            return new FileStreamResult(memoryStream, "application/octet-stream");
        }
        #endregion
        public void SaveFile(ZipOutputStream zipStream, string name, byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var attachmentEntry = new ZipEntry(ZipEntry.CleanName(name))
                {
                    Size = stream.Length,
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

        public void ZipFile(string id, ZipOutputStream zipStream, string name)
        {
            var upload = ExecuteQuery<FetchUploadByIdQuery, Upload>(new FetchUploadByIdQuery { Id = id });

            if (upload != null && upload.Data.Any())
            {

                using (var stream = new MemoryStream(upload.Data))
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

        }

    }
    public class UserData
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public string Extra { get; set; }

    }
}
