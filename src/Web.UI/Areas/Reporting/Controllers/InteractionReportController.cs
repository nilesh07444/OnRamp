using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using Common.Report;
using Domain.Customer;
using ICSharpCode.SharpZipLib.Zip;
using Ramp.Contracts.Query.DocumentCategory;
using Ramp.Contracts.Query.Label;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using VirtuaCon;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Publishers;
using Web.UI.Controllers;
using Enumerable = System.Linq.Enumerable;
using Ramp.Security.Authorization;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.Query.Report;
using Common.Query;
using Domain.Customer.Models.ScheduleReport;

namespace Web.UI.Areas.Reporting.Controllers
{
    public class InteractionReportController : ExportController<InteractionReportExportQuery>
    {
        // GET
        public ActionResult Index()
        {
            var categories = ExecuteQuery<DocumentCategoryListQuery, IEnumerable<JSTreeViewModel>>(new DocumentCategoryListQuery()).ToList();
            ViewBag.Categories = IndentedCategories(categories, "#", 0); ;


           
                  var customerCompanyUserQueryParameter = new CustomerCompanyUserQueryParameter
                  {
                      CompanyId = PortalContext.Current.UserCompany.Id,
                      CompanyName = PortalContext.Current.UserCompany.CompanyName,
                      UserId = Guid.Empty,
                      LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
                  };
            CompanyUserViewModel companyUserViewModel =
                ExecuteQuery<CustomerCompanyUserQueryParameter, CompanyUserViewModel>(customerCompanyUserQueryParameter);
            var datas=companyUserViewModel.UserList.Where(z=>z.Department != null).Select(z=>z.Department).Distinct().ToList();
            ViewBag.Departments = datas;
            
          ViewBag.Groups = ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(
                new AllGroupsByCustomerAdminQueryParameter
                {
                    CompanyId = PortalContext.Current.UserCompany.Id
                }).Select(g => new SerializableSelectListItem
                {
                    Value = g.GroupId.ToString(),
                    Text = g.Title
                }).OrderBy(x => x.Text);

            var labels = ExecuteQuery<TrainingLabelListQuery, IEnumerable<TrainingLabelListModel>>(new TrainingLabelListQuery());
            ViewBag.Labels = labels;
            return View();
        }

        [System.Web.Mvc.HttpPost]
        public JsonResult InteractionReportData(string trainingLabels, string[] groupIds,string[] departments, string[] categoryIds, DocumentType[] documentTypes, DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue || !toDate.HasValue)
                return new JsonResult { Data = null, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            var vm = ExecuteQuery<InteractionReportQuery, InteractionReportViewModel>(new InteractionReportQuery
            {

                TrainingLabels = trainingLabels,
                FromDate = fromDate.Value,
                ToDate = toDate.Value,
                GroupIds = groupIds,
                Departments=departments,
                CategoryIds = categoryIds,
                DocumentTypes = documentTypes
            });

            return new JsonResult { Data = vm, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult Detail([FromUri] InteractionReportDetailQuery query)
        {
            var model = ExecuteQuery<InteractionReportDetailQuery, InteractionReportDetailViewModel>(query);

            return View(model);
        }

        private IEnumerable<SerializableSelectListItem> IndentedCategories(List<JSTreeViewModel> categories, string parentId, int indent)
        {
            var result = new List<SerializableSelectListItem>();
            var children = categories.Where(c => c.parent == parentId).ToList();
            if (!children.Any())
            {
                return Enumerable.Empty<SerializableSelectListItem>();
            }

            foreach (var child in children)
            {
                result.Add(new SerializableSelectListItem
                {
                    Text = $"{new string('\u00a0', indent * 2)}{child.text}",
                    Value = child.id
                });
                result.AddRange(IndentedCategories(categories, child.id, indent + 1));
            }

            return result;
        }

        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.AllowAnonymous]
        public void DownloadExcelLog(string Occurance)
        {
            var getAllReport = ExecuteQuery<FetchAllScheduleReportQuery, List<ScheduleReportModel>>(new FetchAllScheduleReportQuery { }).Where(z => z.Occurences == Occurance && z.ReportAssignedId == "3").ToList();
            foreach (var d in getAllReport)
            {
                var data = ExecuteQuery<FetchByIdQuery, ScheduleReportVM>(new FetchByIdQuery
                {
                    Id = d.Id
                });
                DocumentType[] documentsType = null;
               ReportParam.ReturnDocumentType(data.Params, out documentsType);
                InteractionReportExportQuery query = new InteractionReportExportQuery
                {                    
                    DocumentTypes = documentsType,
                    CategoryIds = ReportParam.ReturnParams(data.Params, "Categories").ToArray(),
                    GroupIds = ReportParam.ReturnParams(data.Params, "Groups").ToArray(),
                    ScheduleName = data.ScheduleName,
                    Recepients = data.RecipientsList,
                    FromDate = ReportParam.FromDate,
                    ToDate = ReportParam.ToDate,
                };
                DownloadEXCEL(query);
            }
        }


        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.AllowAnonymous]
        public override ActionResult DownloadEXCEL([FromUri] InteractionReportExportQuery query)
        {
            query.PortalContext = PortalContext.Current;
            List<IExportModel> excelExports = new List<IExportModel>();

            var vm = ExecuteQuery<InteractionReportQuery, InteractionReportViewModel>(query);

            var checkLists = vm.CheckListInteractions.Concat(vm.GlobalCheckListInteractions).ToList();
            foreach (var item in checkLists)
            {
                var exported = ExecuteQuery<InteractionReportDetailExportQuery, IExportModel>(new InteractionReportDetailExportQuery
                {
                    DocumentId = item.DocumentId,
                    DocumentType = DocumentType.Checklist,
                    FromDate = query.FromDate,
                    ToDate = query.ToDate,
                    PortalContext = query.PortalContext
                });
                exported.Title = "Checklist/" + item.Title.Replace("/", " ") + ".xls";
                excelExports.Add(exported);
            }

            var trainingManuals = vm.TrainingManualInteractions.Concat(vm.GlobalTrainingManualInteractions).ToList();
            foreach (var item in trainingManuals)
            {
                var exported = ExecuteQuery<InteractionReportDetailExportQuery, IExportModel>(new InteractionReportDetailExportQuery
                {
                    DocumentId = item.DocumentId,
                    DocumentType = DocumentType.TrainingManual,
                    FromDate = query.FromDate,
                    ToDate = query.ToDate,
                    PortalContext = query.PortalContext
                });
                exported.Title = "TrainingManual/" + item.Title.Replace("/", " ") + ".xls";
                excelExports.Add(exported);
            }

            var memos = vm.MemoInteractions.Concat(vm.GlobalMemoInteractions).ToList();
            foreach (var item in memos)
            {
                var exported = ExecuteQuery<InteractionReportDetailExportQuery, IExportModel>(new InteractionReportDetailExportQuery
                {
                    DocumentId = item.DocumentId,
                    DocumentType = DocumentType.Memo,
                    FromDate = query.FromDate,
                    ToDate = query.ToDate,
                    PortalContext = query.PortalContext
                });
                exported.Title = "Memo/" + item.Title.Replace("/", " ") + ".xls";
                excelExports.Add(exported);
            }

            var policies = vm.PolicyInteractions.Concat(vm.GlobalPolicyInteractions).ToList();
            foreach (var item in policies)
            {
                var exported = ExecuteQuery<InteractionReportDetailExportQuery, IExportModel>(new InteractionReportDetailExportQuery
                {
                    DocumentId = item.DocumentId,
                    DocumentType = DocumentType.Policy,
                    FromDate = query.FromDate,
                    ToDate = query.ToDate,
                    PortalContext = query.PortalContext
                });
                exported.Title = "Policy/" + item.Title.Replace("/", " ") + ".xls";
                excelExports.Add(exported);
            }

            var test = vm.TestInteractions.Concat(vm.GlobalTestInteractions).ToList();
            foreach (var item in test)
            {
                var exported = ExecuteQuery<InteractionReportDetailExportQuery, IExportModel>(new InteractionReportDetailExportQuery
                {
                    DocumentId = item.DocumentId,
                    DocumentType = DocumentType.Test,
                    FromDate = query.FromDate,
                    ToDate = query.ToDate,
                    PortalContext = query.PortalContext
                });
                exported.Title = "Test/" + item.Title.Replace("/", " ") + ".xls";
                excelExports.Add(exported);
            }


            var memoryStream = new MemoryStream();
            var zipStream = new ZipOutputStream(memoryStream);
            zipStream.SetLevel(3);
            var stream = new MemoryStream();
            foreach (var excelModel in excelExports)
            {

                IReportDocumentWriter excelPublisher = new ExcelReportPublisher();
                excelPublisher.Write(excelModel.Document, stream);
                stream.Position = 0;

                var t = stream.ToArray();
                SaveFile(zipStream, excelModel.Title, t);
            }

            zipStream.IsStreamOwner = false;
            zipStream.Close();
            memoryStream.Position = 0;
            var fileName = "InteractionReport" + ".zip";
            string filePaths = null;
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
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult DownloadSubmissionsZip(string type, InteractionReportDetailExportQuery query)
        {
            query.PortalContext = PortalContext.Current;
            List<IExportModel> excelExports = new List<IExportModel>();

            DocumentType[] documentTypes = new DocumentType[] { DocumentType.Checklist, DocumentType.Memo, DocumentType.Policy, DocumentType.Test, DocumentType.TrainingManual };
            var interactionModel = new InteractionReportQuery { FromDate = query.FromDate, ToDate = query.ToDate, DocumentTypes = documentTypes };

            var vm = ExecuteQuery<InteractionReportQuery, InteractionReportViewModel>(interactionModel);
            List<InteractionReportViewModel.InteractionModel> interactions = new List<InteractionReportViewModel.InteractionModel>();
            List<InteractionReportViewModel.PolicyInteractionModel> policyInteractions = new List<InteractionReportViewModel.PolicyInteractionModel>();
            List<InteractionReportViewModel.TestInteractionModel> testInteractions = new List<InteractionReportViewModel.TestInteractionModel>();

            var docType = DocumentType.Checklist;
            switch (type)
            {
                case "checkList":
                    interactions = vm.CheckListInteractions as List<InteractionReportViewModel.InteractionModel>;
                    docType = DocumentType.Checklist;
                    break;
                case "globalcheckList":
                    interactions = vm.GlobalCheckListInteractions as List<InteractionReportViewModel.InteractionModel>;
                    docType = DocumentType.Checklist;
                    break;
                case "memo":
                    interactions = vm.MemoInteractions as List<InteractionReportViewModel.InteractionModel>;
                    docType = DocumentType.Memo;
                    break;
                case "globalmemo":
                    interactions = vm.GlobalMemoInteractions as List<InteractionReportViewModel.InteractionModel>;
                    docType = DocumentType.Memo;
                    break;
                case "trainingManual":
                    interactions = vm.TrainingManualInteractions as List<InteractionReportViewModel.InteractionModel>;
                    docType = DocumentType.TrainingManual;
                    break;
                case "globalTrainingManual":
                    interactions = vm.GlobalTrainingManualInteractions as List<InteractionReportViewModel.InteractionModel>;
                    docType = DocumentType.TrainingManual;
                    break;
                case "policy":
                    policyInteractions = vm.PolicyInteractions as List<InteractionReportViewModel.PolicyInteractionModel>;
                    docType = DocumentType.Policy;
                    break;
                case "globalpolicy":
                    policyInteractions = vm.GlobalPolicyInteractions as List<InteractionReportViewModel.PolicyInteractionModel>;
                    docType = DocumentType.Policy;
                    break;
                case "test":
                    testInteractions = vm.TestInteractions as List<InteractionReportViewModel.TestInteractionModel>;
                    docType = DocumentType.Test;
                    break;
                case "globaltest":
                    testInteractions = vm.GlobalTestInteractions as List<InteractionReportViewModel.TestInteractionModel>;
                    docType = DocumentType.Test;
                    break;
                default:
                    interactions = vm.CheckListInteractions as List<InteractionReportViewModel.InteractionModel>;
                    docType = DocumentType.Checklist;
                    break;
            }
            if (docType != DocumentType.Policy && docType != DocumentType.Test)
            {
                foreach (var item in interactions)
                {
                    var exported = ExecuteQuery<InteractionReportDetailExportQuery, IExportModel>(new InteractionReportDetailExportQuery
                    {
                        DocumentId = item.DocumentId,
                        DocumentType = docType,
                        FromDate = query.FromDate,
                        ToDate = query.ToDate,
                        PortalContext = query.PortalContext
                    });
                    exported.Title = docType + "/" + item.Title.Replace("/", " ") + ".xls";
                    excelExports.Add(exported);
                }
            }
            else if (docType == DocumentType.Policy)
            {
                foreach (var item in policyInteractions)
                {
                    var exported = ExecuteQuery<InteractionReportDetailExportQuery, IExportModel>(new InteractionReportDetailExportQuery
                    {
                        DocumentId = item.DocumentId,
                        DocumentType = docType,
                        FromDate = query.FromDate,
                        ToDate = query.ToDate,
                        PortalContext = query.PortalContext
                    });
                    exported.Title = docType + "/" + item.Title.Replace("/", " ") + ".xls";
                    excelExports.Add(exported);
                }
            }
            else
            {
                foreach (var item in testInteractions)
                {
                    var exported = ExecuteQuery<InteractionReportDetailExportQuery, IExportModel>(new InteractionReportDetailExportQuery
                    {
                        DocumentId = item.DocumentId,
                        DocumentType = docType,
                        FromDate = query.FromDate,
                        ToDate = query.ToDate,
                        PortalContext = query.PortalContext
                    });
                    exported.Title = docType + "/" + item.Title.Replace("/", " ") + ".xls";
                    excelExports.Add(exported);
                }
            }

            var memoryStream = new MemoryStream();
            var zipStream = new ZipOutputStream(memoryStream);
            zipStream.SetLevel(3);

            foreach (var excelModel in excelExports)
            {
                var stream = new MemoryStream();
                IReportDocumentWriter excelPublisher = new ExcelReportPublisher();
                excelPublisher.Write(excelModel.Document, stream);
                stream.Position = 0;

                var t = stream.ToArray();
                SaveFile(zipStream, excelModel.Title, t);
            }

            zipStream.IsStreamOwner = false;
            zipStream.Close();
            memoryStream.Position = 0;
            var fileName = "InteractionReport" + ".zip";
            Response.AddHeader("filename", fileName);
            return new FileStreamResult(memoryStream, "application/octet-stream");
        }

        public void SaveFile(ZipOutputStream zipStream, string name, byte[] data)
        {
            try
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
                        if (PortalContext.Current!=null && !Response.IsClientConnected)
                        {
                            break;
                        }
                    }
                    zipStream.CloseEntry();

                };
            }
            catch (Exception ex)
            {

            }
        }


        [System.Web.Mvc.HttpGet]
        public virtual ActionResult DownloadDetailEXCEL([FromUri] InteractionReportDetailExportQuery query)
        {
            query.PortalContext = PortalContext.Current;
            return Publish(ExecuteQuery<InteractionReportDetailExportQuery, IExportModel>(query), new ExcelReportPublisher(), "application/vnd.ms-excel", "xls");
        }

        public override ActionResult Zip(InteractionReportExportQuery query)
        {
            throw new NotImplementedException();
        }
    }
}