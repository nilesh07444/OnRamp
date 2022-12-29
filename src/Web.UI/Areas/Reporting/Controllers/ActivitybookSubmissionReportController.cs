using Common.Command;
using Common.Query;
using Common.Report;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.ScheduleReport;
using Domain.Customer.Models.Test;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Ramp.Contracts.Command.DocumentUsage;
using Ramp.Contracts.Command.TestSession;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.Query.CheckList;
using Ramp.Contracts.Query.CheckListChapterUserResult;
using Ramp.Contracts.Query.CheckListUserResult;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Report;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.Query.TestSession;
using Ramp.Contracts.Query.Upload;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
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

namespace Web.UI.Areas.Reporting.Controllers {
	public class ActivitybookSubmissionReportController : ExportController<CheckListSubmissionReportExportQuery> {


		// GET: Reporting/CheckListSubmissionReport
		public ActionResult Index(string id = null) {
			ViewBag.Id = id;
			ViewBag.CheckLists = ExecuteQuery<AllCheckListSubmissionReportQuery, IEnumerable<CheckListModel>>(new AllCheckListSubmissionReportQuery()).Where(x => !string.IsNullOrWhiteSpace(x.Title)).Select(x => new MyCheckList {
				Value = x.Title,
				Extra = x.Description,
				Id = x.Id
			}).ToList();

			return View();
		}


		[OutputCache(NoStore = true, Duration = 0)]
		[System.Web.Mvc.HttpGet]
		public  ActionResult ActivitybookPreview([FromUri] object id, string companyId = null, string checkUser = null) {
			if (companyId != null) {
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

			if (Thread.CurrentPrincipal.IsInStandardUserRole()) {
				var userId = Thread.CurrentPrincipal.GetId().ToString();
				var documentType = DocumentType.Checklist;

				if (!ExecuteQuery<DocumentAssignedToUserQuery, bool>(new DocumentAssignedToUserQuery {
					DocumentId = id.ToString(),
					DocumentType = documentType,
					UserId = userId
				}) && model!=null && !model.IsGlobalAccessed) {
					return RedirectToAction("AccessDenied", "Account");
				}

				var testSession = ExecuteQuery<TestSessionQuery, TestSessionViewModel>(new TestSessionQuery {
					UserId = userId
				});
				if (testSession != null) {
					if ((testSession.EnableTimer && testSession.TimeLeft == TimeSpan.Zero) || (testSession.DocumentStatus.HasValue && testSession.DocumentStatus.Value == DocumentStatus.Recalled)) {
						var testResult = ExecuteQuery<FetchByIdQuery<Test>, TestResultModel>(new FetchByIdQuery<Test> { Id = testSession.CurrentTestId });
						var createTestResultCommand = JsonConvert.DeserializeObject<CreateTestResultCommand>(JsonConvert.SerializeObject(testResult)); // hack
						createTestResultCommand.PortalContext = PortalContext.Current;
						createTestResultCommand.UserId = userId;
						createTestResultCommand.TimeLeft = testSession.TimeLeft;

						ExecuteCommand(createTestResultCommand);
						ExecuteCommand(new TestSessionEndCommand {
							UserId = userId
						});
					} else if (documentType != DocumentType.Test ? !testSession.OpenTest : testSession.CurrentTestId != id.ToString())
						return RedirectToAction("InProgress", "Test");
				}

				if (documentType != DocumentType.Test) {
					var viewDate = DateTime.UtcNow;
					//viewDate = DateTime.SpecifyKind(DateTime.ParseExact(viewDate.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null), DateTimeKind.Utc); // remove milliseconds
					ExecuteCommand(new CreateOrUpdateDocumentUsageCommand {
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

		public  void Preview_PostProcess(CheckListModel model, string companyId = null,string userId = null) {
			if (model.CoverPicture != null) {
				model.CoverPicture.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(model.CoverPicture.Id, 300, 300, companyId)).Replace("/Reporting", "");
				model.CoverPicture.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(model.CoverPicture.Id, companyId)).Replace("/Reporting", "");
			}
			model.ContentModels.ToList();

			var assignedDocument = ExecuteQuery<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = userId, DocumentId = model.Id });

			if (assignedDocument != null) {

				var checkListUserResult = ExecuteQuery<CheckListUserResultQuery, CheckListUserResultViewModel>(new CheckListUserResultQuery { AssignedDocumentId = assignedDocument.Id });

				if (checkListUserResult != null) {
					model.Status = checkListUserResult.Status;
					model.SubmittedDate = checkListUserResult.SubmittedDate;
				} else {
					model.Status = false;
				}

				foreach (var item in model.ContentModels) {

					var checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = item.Id });

					item.StandardUserAttachments = checkListChapterUserUpload;
					item.StandardUserAttachments.ToList().ForEach(attachment => {
						attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId)).Replace("/Reporting","");
						attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId)).Replace("/Reporting", "");
						attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId)).Replace("/Reporting", "");
					});

					var checkListChapterUserResult = ExecuteQuery<CheckListChapterUserResultQuery, CheckListChapterUserResultViewModel>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = item.Id });
					if (checkListChapterUserResult != null) {
						item.IsChecked = checkListChapterUserResult.IsChecked;
						item.IssueDiscription = checkListChapterUserResult.IssueDiscription;
					} else
						item.IsChecked = false;
				}
			} else {
				var checkListUserResult = ExecuteQuery<CheckListUserResultQuery, CheckListUserResultViewModel>(new CheckListUserResultQuery { DocumentId = model.Id,IsGlobalAccessed=true,UserId=userId });

				if (checkListUserResult != null) {
					model.Status = checkListUserResult.Status;
					model.SubmittedDate = checkListUserResult.SubmittedDate;
				} else {
					model.Status = false;
				}

				foreach (var item in model.ContentModels) {

					var checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { DocumentId = model.Id,UserId=userId,IsGlobalAccessed=true, CheckListChapterId = item.Id });

					item.StandardUserAttachments = checkListChapterUserUpload;
					item.StandardUserAttachments.ToList().ForEach(attachment => {
						attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId)).Replace("/Reporting", "");
						attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId)).Replace("/Reporting", "");
						attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId)).Replace("/Reporting", "");
					});

					var checkListChapterUserResult = ExecuteQuery<CheckListChapterUserResultQuery, CheckListChapterUserResultViewModel>(new CheckListChapterUserResultQuery { DocumentId = model.Id,UserId=userId,IsGlobalAccessed=true, CheckListChapterId = item.Id });
					if (checkListChapterUserResult != null) {
						item.IsChecked = checkListChapterUserResult.IsChecked;
						item.IssueDiscription = checkListChapterUserResult.IssueDiscription;
					} else
						item.IsChecked = false;
				}
			}

		}

		/// <summary>
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		[System.Web.Http.HttpPost]
		public JsonResult GetActivitybookDetails(CheckListSubmissionReportQuery query) {
			var checkLists = ExecuteQuery<CheckListSubmissionReportQuery, IEnumerable<ChecklistInteractionModel>>(query);

			if (checkLists.Any())
				return new JsonResult { Data = checkLists, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			return new JsonResult { Data = true, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		public override ActionResult Zip([FromUri] CheckListSubmissionReportExportQuery query) {
			throw new NotImplementedException();
		}

		[System.Web.Mvc.HttpGet]
		public ActionResult DownloadSubmissionsZip(CheckListSubmissionReportQuery query) {
			query.PortalContext = PortalContext.Current;

			var checkLists = ExecuteQuery<CheckListSubmissionReportQuery, IEnumerable<ChecklistInteractionModel>>(query).Where(c=>c.Id==query.CheckListId).FirstOrDefault();

			var memoryStream = new MemoryStream();
			var zipStream = new ZipOutputStream(memoryStream);
			zipStream.SetLevel(3);

			foreach (var chk in checkLists.Checklist) {

				var checkList = ExecuteQuery<FetchByIdQuery, CheckListModel>(new FetchByIdQuery { Id = query.CheckListId });

				#region for xls checklist

				var chkQuery = new CheckListExportReportQuery();
				if (!string.IsNullOrEmpty(chkQuery.CompanyId))
					PortalContext.Override(chkQuery.CompanyId.AsGuid());
				chkQuery.AddOnrampBranding = true;
				chkQuery.PortalContext = PortalContext.Current;
				chkQuery.UserId = Guid.Parse(chk.UserId);
				chkQuery.ResultId = query.CheckListId;

				var model = ExecuteQuery<CheckListExportReportQuery, IExportModel>(chkQuery);
				if (string.IsNullOrEmpty(query.ToggleFilter))
					query.ToggleFilter = model.Title;

				model.Title = chk.UserName.Trim()+"/"+ checkList.Title.Trim().Replace("/","")+".xls";
				var stream = new MemoryStream();
				IReportDocumentWriter publisher = new ExcelReportPublisher();
				publisher.Write(model.Document, stream);
				stream.Position = 0;

				stream.Position = 0;
				var t = stream.ToArray();

				SaveFile(zipStream, model.Title, t);

				#endregion

				var assignedDocument = ExecuteQuery<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = Convert.ToString(chkQuery.UserId), DocumentId = chkQuery.ResultId });

				if (checkList != null && checkList.ContentModels.Any()) {
					var chapters = checkList.ContentModels;

					foreach (var item in chapters) {
						if (item.StandardUserAttachments.Any()) {

							foreach (var attachment in item.StandardUserAttachments) {

								ZipFiles(attachment.Id, zipStream,  attachment.Name);

							}

						}
						var checkListChapterUserUpload = new List<UploadResultViewModel>();
						if (assignedDocument != null) {
							 checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = item.Id ,IsGlobalAccessed=false });
						} else {
							checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { DocumentId = checkList.Id,UserId=chk.UserId,IsGlobalAccessed=true, CheckListChapterId = item.Id });
						}
						if (checkListChapterUserUpload.Any()) {

							foreach (var attachment in checkListChapterUserUpload) {

								ZipFiles(attachment.Id, zipStream,  attachment.Name);

							}
						}
					}
				}
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
		[System.Web.Mvc.AllowAnonymous]
		public ActionResult DownloadPrintExcel([FromUri] CheckListExportReportQuery query) {

			if (!string.IsNullOrEmpty(query.CompanyId))
				PortalContext.Override(query.CompanyId.AsGuid());
			query.AddOnrampBranding = true;
			query.PortalContext = PortalContext.Current;
			query.UserId = query.UserId;

			var memoryStream = new MemoryStream();
			var zipStream = new ZipOutputStream(memoryStream);
			zipStream.SetLevel(3);

			var checkList = ExecuteQuery<FetchByIdQuery, CheckListModel>(new FetchByIdQuery { Id = query.ResultId });
			query.IsChecklistTracked = checkList.IsChecklistTracked;
			#region for xls checklist

			var model = ExecuteQuery<CheckListExportReportQuery, IExportModel>(query);

			model.Title = checkList.Title.Trim().Replace("/","-") + ".xls";
			var stream = new MemoryStream();
			IReportDocumentWriter publisher = new ExcelReportPublisher();
			publisher.Write(model.Document, stream);
			stream.Position = 0;

			var t = stream.ToArray();

			SaveFile(zipStream, model.Title, t);

			#endregion


			var assignedDocument = ExecuteQuery<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = Convert.ToString(query.UserId), DocumentId = query.ResultId });

			if (checkList != null && checkList.ContentModels.Any()) {
				var chapters = checkList.ContentModels;

				foreach (var item in chapters) {
					if (item.StandardUserAttachments.Any()) {

						foreach (var attachment in item.StandardUserAttachments) {

							ZipFiles(attachment.Id, zipStream, attachment.Name);

						}

					}
					var checkListChapterUserUpload = new List<UploadResultViewModel>();
					if (assignedDocument != null) {
					 checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = item.Id,IsGlobalAccessed=false });
					} else {
						checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { DocumentId = query.ResultId ,UserId=query.UserId.ToString(),IsGlobalAccessed=true, CheckListChapterId = item.Id });
					}

					if (checkListChapterUserUpload.Any()) {

						foreach (var attachment in checkListChapterUserUpload) {

							ZipFiles(attachment.Id, zipStream, attachment.Name);

						}
					}
				}
			}

			zipStream.IsStreamOwner = false;
			zipStream.Close();
			memoryStream.Position = 0;
			var fileName = checkList.Title + ".zip";
			Response.AddHeader("filename", fileName);
			return new FileStreamResult(memoryStream, "application/octet-stream");
		}
		#region written by ashok to save file in zip and download zip
		public void ZipFiles(string id, ZipOutputStream zipStream, string name) {
			var upload = ExecuteQuery<FetchUploadByIdQuery, Upload>(new FetchUploadByIdQuery { Id = id });

			if (upload != null && upload.Data.Any()) {
				SaveFile(zipStream, name, upload.Data);
			}

		}

		public void SaveFile(ZipOutputStream zipStream, string name, byte[] data) {
			using (var stream = new MemoryStream(data)) {
				var attachmentEntry = new ZipEntry(ZipEntry.CleanName(name)) {
					Size = stream.Length
				};
				zipStream.PutNextEntry(attachmentEntry);
				byte[] buffer = new byte[4096];
				int count = stream.Read(buffer, 0, buffer.Length);
				while (count > 0) {
					zipStream.Write(buffer, 0, count);
					count = stream.Read(buffer, 0, buffer.Length);
					if (!Response.IsClientConnected) {
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
		[System.Web.Mvc.HttpGet]
		[System.Web.Mvc.AllowAnonymous]
		public ActionResult ZipCheckList(CheckListSubmissionReportQuery query) {

			var portalContext = PortalContext.Current;
			var fromDate = query.FromDate;
			var toDate = query.ToDate;

			string folderPath = Server.MapPath(ConfigurationManager.AppSettings["downloadPath"] + ConfigurationManager.AppSettings["DocPath"]);
			
			
			var checkLists = ExecuteQuery<CheckListSubmissionReportQuery, IEnumerable<ChecklistInteractionModel>>(query);

			var docList = new List<string>();

			foreach (var checklist in checkLists) {

				foreach (var item in checklist.Checklist) {
					var checkListQuery = new UserActivityAndPerformanceReportExportQuery {
						UserId = item.UserId,
						FromDate = fromDate,
						ToDate = toDate,
						PortalContext = portalContext,
					};
					var model = ExecuteQuery<UserActivityAndPerformanceReportExportQuery, IExportModel>(checkListQuery);

					model.Title = item.UserName + ".xls";
					var stream = new MemoryStream();
					IReportDocumentWriter publisher = new ExcelReportPublisher();
					publisher.Write(model.Document, stream);
					stream.Position = 0;

					if (!Directory.Exists(folderPath)) {
						Directory.CreateDirectory(folderPath);
					}

					FileStream file = new FileStream(Path.Combine(folderPath, model.Title), FileMode.Create, FileAccess.Write);
					stream.WriteTo(file);
					file.Close();
					stream.Close();

					docList.Add(folderPath + model.Title);
				}
			}

			try {

				var fileName = string.Format($"CheckList Submission Report.zip");

				string tempOutPutPath = folderPath + fileName;
				
				using (ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(tempOutPutPath))) {
					s.SetLevel(9); // 0-9, 9 being the highest compression  

					byte[] buffer = new byte[4096];

					for (int i = 0; i < docList.Count; i++) {

						ZipEntry entry = new ZipEntry(Path.GetFileName(docList[i])) {
							DateTime = DateTime.Now,
							IsUnicodeText = true
						};
						s.PutNextEntry(entry);

						using (FileStream fs = System.IO.File.OpenRead(docList[i])) {
							int sourceBytes;
							do {
								sourceBytes = fs.Read(buffer, 0, buffer.Length);
								s.Write(buffer, 0, sourceBytes);
							} while (sourceBytes > 0);
						}
					}
					s.Finish();
					s.Flush();
					s.Close();

				}

				byte[] finalResult = System.IO.File.ReadAllBytes(tempOutPutPath);
				if (System.IO.File.Exists(tempOutPutPath))
					System.IO.File.Delete(tempOutPutPath);

				if (finalResult == null || !finalResult.Any())
					throw new Exception(string.Format("No Files found with pdf"));
							   				 
				Response.AddHeader("filename", fileName);
				
				return File(finalResult, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
			}
			catch (Exception ex) {
				throw ex;
			}
		}
		[System.Web.Mvc.HttpGet]
		[System.Web.Mvc.AllowAnonymous]
		public void DownloadSummaryEXCELPass(string Occurance)
		{
			var getAllReport = ExecuteQuery<FetchAllScheduleReportQuery, List<ScheduleReportModel>>(new FetchAllScheduleReportQuery { }).Where(z => z.Occurences == Occurance && z.ReportAssignedId == "6").ToList();
			foreach (var d in getAllReport)
			{
				var data = ExecuteQuery<FetchByIdQuery, ScheduleReportVM>(new FetchByIdQuery
				{
					Id = d.Id
				});
				var lstActivity = ReportParam.ReturnParams(data.Params, "ActivityBook");
				CheckListSubmissionReportQuery query = new CheckListSubmissionReportQuery
				{
					ToggleFilter = "User Name,Viewed,Date Assigned,Date Viewed,Checks Completed,Date Submitted,Status,Access,Group",
					CheckListIds = lstActivity,
					CheckListId = lstActivity[0],
					ScheduleName = data.ScheduleName,
					Recepients = data.RecipientsList,
					FromDate = ReportParam.FromDate,
					ToDate = ReportParam.ToDate,
				};

				DownloadSummaryEXCEL(query);
			}
		}


		[System.Web.Mvc.HttpGet]
		[System.Web.Mvc.AllowAnonymous]
		public ActionResult DownloadSummaryEXCEL(CheckListSubmissionReportQuery query) {
			query.PortalContext = PortalContext.Current;
			ChecklistSummaryExportReportQuery exportQuery = new ChecklistSummaryExportReportQuery() {
				PortalContext=query.PortalContext,
				CheckListSubmissionReportQuery=query
			};
			return Publish(ExecuteQuery<ChecklistSummaryExportReportQuery, IExportModel>(exportQuery), new ExcelReportPublisher(), "application/vnd.ms-excel", "xls");
		}

	}
	public class MyCheckList {
		public string Id { get; set; }
		public string Value { get; set; }
		public string Extra { get; set; }

	}
}