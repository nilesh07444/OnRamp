using Common.Command;
using Common.Events;
using Common.Query;
using Common.Report;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using ICSharpCode.SharpZipLib.Zip;
using Ramp.Contracts.Command.CheckList;
using Ramp.Contracts.Command.DocumentUsage;
using Ramp.Contracts.CommandParameter;
using Ramp.Contracts.CommandParameter.CheckList;
using Ramp.Contracts.Events.DocumentWorkflow;
using Ramp.Contracts.Query.CheckList;
using Ramp.Contracts.Query.CheckListChapterUserResult;
using Ramp.Contracts.Query.CheckListUserResult;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Label;
using Ramp.Contracts.Query.Upload;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Publishers;
using Web.UI.Code.Extensions;
using Web.UI.Helpers;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using RouteAttribute = System.Web.Mvc.RouteAttribute;

namespace Web.UI.Controllers {
	public class ActivitybookController : KnockoutDocumentController<CheckListQuery, CheckListListModel, CheckList, CheckListModel, CreateOrUpdateCheckListCommand> {

		public override void Edit_PostProcess(CheckListModel model, string companyId = null, DocumentUsageStatus? status = null, string userid = null) {
			if (model.CoverPicture != null) {
				model.CoverPicture.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(model.CoverPicture.Id, 300, 300, companyId));
				model.CoverPicture.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(model.CoverPicture.Id, companyId));
			}
			model.ContentModels.ToList().ForEach(content => {
				content.Attachments.ToList().ForEach(attachment => {
					attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
					attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
					attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
				});
				content.ContentToolsUploads.ToList().ForEach(attachment => {
					attachment.url = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.url, null, null, companyId));
				});
			});
			var trainigLabels = new List<string>();
			foreach (var item in model.TrainingLabels.Split(',')) {
				var label = ExecuteQuery<FetchByNameQuery, TrainingLabelModel>(new FetchByNameQuery() { Name = item });
				trainigLabels.Add(label.Id);
			}
			model.LabelIds = string.Join(",", trainigLabels);
			var labels = ExecuteQuery<TrainingLabelListQuery, IEnumerable<TrainingLabelListModel>>(new TrainingLabelListQuery());
			ViewBag.Labels = labels.OrderBy(c => c.Name).ToList();

			//neeraj
			var x = PortalContext.Current.UserCompany.Id;
			var ca = ExecuteQuery<FetchAllRecordsQuery, IEnumerable<StandardUser>>(new FetchAllRecordsQuery());
			var l = new List<StandardUser>();
			foreach (var c in ca.ToList())
			{
				if (c.Id != User.GetId())
				{
					l.Add(c);
				}
			}

			ViewBag.ContentApprovers = l;
			//get all approver name from document Id
			if (model.Approver != null)
			{
				List<string> names = new List<string>();

				string[] Ids = model.Approver.Split(',');

				foreach (var id in Ids)
				{
					var userDetail = ExecuteQuery<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery { Id = id });

					names.Add(userDetail.UserName);
				}

				ViewBag.DocumentApprovers = string.Join(",", names);
			}
		}
		protected override void Preview_PostProcess(CheckListModel model, string companyId, string checkUser = null, bool isGlobal = false, DocumentUsageStatus? status = null) {

			ViewBag.IsGlobalAccessed = model.IsGlobalAccessed;

			Edit_PostProcess(model, companyId);
		}

		[OutputCache(NoStore = true, Duration = 0)]
		[System.Web.Mvc.HttpGet]
		public ActionResult ActivitybookComplete([FromUri] object id, string companyId = null, bool IsGlobal = false) {

			if (companyId != null) {
				ExecuteCommand(new UpdateConnectionStringCommand { CompanyId = companyId });
			}
			var userId = Thread.CurrentPrincipal.GetId().ToString();
			var viewDate = DateTime.UtcNow;
			viewDate = DateTime.SpecifyKind(DateTime.ParseExact(viewDate.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null), DateTimeKind.Utc); // remove milliseconds

			ExecuteCommand(new CreateOrUpdateDocumentUsageCommand {
				DocumentId = id.ToString(),
				DocumentType = GetDocumentType(),
				UserId = userId,
				ViewDate = viewDate,
				IsGlobalAccessed = IsGlobal
			});
			ViewBag.IsGlobalAccessed = IsGlobal;
			ViewBag.Links = new Dictionary<string, string>()
			{
				{"index", Url.Action("MyDocuments", "Document", new {Area = ""})},
				{"learnMore", Url.Action("GlobalDocuments", "Document", new {Area = ""})},
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
				ActivitybookComplete_PostProcess(model, companyId, IsGlobal);


			ExecuteCommand(new UpdateConnectionStringCommand());

			Response.Cache.AppendCacheExtension("no-store, must-revalidate");

			return View(model);
		}

		public void ActivitybookComplete_PostProcess(CheckListModel model, string companyId = null, bool isGlobal = false) {
			if (model.CoverPicture != null) {
				model.CoverPicture.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(model.CoverPicture.Id, 300, 300, companyId));
				model.CoverPicture.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(model.CoverPicture.Id, companyId));
			}
			model.ContentModels.ToList().ForEach(content => {
				content.Attachments.ToList().ForEach(attachment => {
					attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
					attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
					attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));

				});
				content.IsStandardUserAttachements = true;
				content.ContentToolsUploads.ToList().ForEach(attachment => {
					attachment.url = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.url, null, null, companyId));
				});
			});
			var userId = Thread.CurrentPrincipal.GetId().ToString();

			var assignedDocument = !isGlobal ? ExecuteQuery<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = userId, DocumentId = model.Id }) : null;

			if (assignedDocument != null) {

				var checkListUserResult = ExecuteQuery<CheckListUserResultQuery, CheckListUserResultViewModel>(new CheckListUserResultQuery { AssignedDocumentId = assignedDocument.Id });

				if (checkListUserResult != null) {
					model.Status = checkListUserResult.Status;
					model.SubmittedDate = checkListUserResult.SubmittedDate;
				}

				foreach (var item in model.ContentModels) {

					var checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = item.Id });

					item.StandardUserAttachments = checkListChapterUserUpload;
					item.StandardUserAttachments.ToList().ForEach(attachment => {
						attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
						attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
						attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
					});

					var checkListChapterUserResult = ExecuteQuery<CheckListChapterUserResultQuery, CheckListChapterUserResultViewModel>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = item.Id });
					if (checkListChapterUserResult != null) {
						item.IsChecked = checkListChapterUserResult.IsChecked;
						item.IssueDiscription = checkListChapterUserResult.IssueDiscription;
					} else
						item.IsChecked = false;
				}
			} else {
				var checkListUserResult = ExecuteQuery<CheckListUserResultQuery, CheckListUserResultViewModel>(new CheckListUserResultQuery { DocumentId = model.Id, IsGlobalAccessed = isGlobal, UserId = userId });

				if (checkListUserResult != null) {
					model.Status = checkListUserResult.Status;
					model.SubmittedDate = checkListUserResult.SubmittedDate;
				}
				foreach (var item in model.ContentModels) {

					var checkListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { DocumentId = model.Id, IsGlobalAccessed = isGlobal, CheckListChapterId = item.Id, UserId = userId });

					item.StandardUserAttachments = checkListChapterUserUpload;
					item.StandardUserAttachments.ToList().ForEach(attachment => {
						attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
						attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
						attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
					});

					var checkListChapterUserResult = ExecuteQuery<CheckListChapterUserResultQuery, CheckListChapterUserResultViewModel>(new CheckListChapterUserResultQuery { DocumentId = model.Id, IsGlobalAccessed = isGlobal, CheckListChapterId = item.Id, UserId = userId });
					if (checkListChapterUserResult != null) {
						item.IsChecked = checkListChapterUserResult.IsChecked;
						item.IssueDiscription = checkListChapterUserResult.IssueDiscription;
					} else
						item.IsChecked = false;
				}
			}

		}

		[System.Web.Http.HttpPost]
		[ValidateInput(false)]
		public JsonResult CompleteActivitybookChapters(CheckListModel ContentModel, bool status, bool IsGlobalAccessed = false) {
			var userId = Thread.CurrentPrincipal.GetId().ToString();

			//below code by neeraj
			
			var viewDate = DateTime.UtcNow;
			//viewDate = DateTime.SpecifyKind(DateTime.ParseExact(viewDate.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null), DateTimeKind.Utc); // remove milliseconds

			ExecuteCommand(new CreateOrUpdateDocumentUsageCommand
			{
				DocumentId = ContentModel.Id.ToString(),
				DocumentType = DocumentType.Checklist,
				UserId = userId,
				ViewDate = viewDate,
				IsGlobalAccessed = IsGlobalAccessed,
				Status = (status == true) ? DocumentUsageStatus.Complete : DocumentUsageStatus.UnderReview
			});

			//code end

			if (IsGlobalAccessed) {

				foreach (var item in ContentModel.ContentModels) {

					ExecuteCommand(new CreateOrUpdateCheckListChapterUserResultCommand { UserId = userId, DocumentId = ContentModel.Id, IsGlobalAccessed = IsGlobalAccessed, CheckListChapterId = item.Id, IsChecked = item.IsChecked, IssueDiscription = item.IssueDiscription });

					// ExecuteCommand(new DeleteCheckListUserUploadResultCommand { DocumentId = ContentModel.Id, IsGlobalAccessed = IsGlobalAccessed, CheckListChapterId = item.Id });

					foreach (var upload in item.StandardUserAttachments) {

						ExecuteCommand(new CreateOrUpdateCheckListChapterUserUploadResultsCommand { UserId = userId, IsGlobalAccessed = IsGlobalAccessed, DocumentId = ContentModel.Id, CheckListChapterId = item.Id, UploadId = upload.Id });
					}
				}

				ExecuteCommand(new CreateOrUpdateCheckListUserResultCommand { UserId = userId, AssignedDocumentId = null, DocumentId = ContentModel.Id, Status = status, IsGlobalAccessed = IsGlobalAccessed });

			} else {
				var model = ExecuteQuery<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = userId, DocumentId = ContentModel.Id });

				foreach (var item in ContentModel.ContentModels) {

					ExecuteCommand(new CreateOrUpdateCheckListChapterUserResultCommand { UserId = userId, AssignedDocumentId = model?.Id, CheckListChapterId = item.Id, IsChecked = item.IsChecked, IssueDiscription = item.IssueDiscription });

					// ExecuteCommand(new DeleteCheckListUserUploadResultCommand { AssignedDocumentId = model?.Id, CheckListChapterId = item.Id });

					foreach (var upload in item.StandardUserAttachments) {

						ExecuteCommand(new CreateOrUpdateCheckListChapterUserUploadResultsCommand { UserId = userId, AssignedDocumentId = model.Id, CheckListChapterId = item.Id, UploadId = upload.Id });
					}
				}

				ExecuteCommand(new CreateOrUpdateCheckListUserResultCommand { UserId = userId, AssignedDocumentId = model?.Id, DocumentId = ContentModel.Id, Status = status, IsGlobalAccessed = IsGlobalAccessed });

			}

			return new JsonResult { Data = "done", JsonRequestBehavior = JsonRequestBehavior.AllowGet };

		}

		public JsonResult IsActivitybookComplete(string documentId) {
			var result = ExecuteQuery<FetchByIdQuery, AssignedDocumentListModel>(new FetchByIdQuery { Id = documentId });

			return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		private DocumentType GetDocumentType() {
			var documentType = DocumentType.Unknown;
			switch (typeof(CheckList).Name) {
				case "TrainingManual":
					documentType = DocumentType.TrainingManual;
					break;
				case "Test":
					documentType = DocumentType.Test;
					break;
				case "Policy":
					documentType = DocumentType.Policy;
					break;
				case "Memo":
					documentType = DocumentType.Memo;
					break;
				case "CheckList":
					documentType = DocumentType.Checklist;
					break;
			}

			return documentType;
		}

		#region written by ashok to save file in zip and download zip
		[System.Web.Mvc.HttpGet]
		public ActionResult DownloadPrintExcel(string id, bool addOnrampBranding = false) {

			var query = new CheckListExportReportQuery {
				AddOnrampBranding = true,
				PortalContext = PortalContext.Current,
				ResultId = id,
				UserId = Thread.CurrentPrincipal.GetId(),
				IsDetail = false
			};

			var memoryStream = new MemoryStream();
			var zipStream = new ZipOutputStream(memoryStream);
			zipStream.SetLevel(3);

			var checkList = ExecuteQuery<FetchByIdQuery, CheckListModel>(new FetchByIdQuery { Id = query.ResultId });
			query.IsChecklistTracked = checkList.IsChecklistTracked;
			var model = ExecuteQuery<CheckListExportReportQuery, IExportModel>(query);
			model.Title = model.Title.RemoveSpecialCharacters() + ".xls";
			var stream = new MemoryStream();
			IReportDocumentWriter publisher = new ExcelReportPublisher();
			publisher.Write(model.Document, stream);
			stream.Position = 0;
			var t = stream.ToArray();

			SaveFile(zipStream, model.Title, t);

			var trainingManual = ExecuteQuery<FetchByIdQuery, TrainingManualModel>(new FetchByIdQuery { Id = id });
			var memo = ExecuteQuery<FetchByIdQuery, MemoModel>(new FetchByIdQuery { Id = id });
			var policy = ExecuteQuery<FetchByIdQuery, PolicyModel>(new FetchByIdQuery { Id = id });
			var test = ExecuteQuery<FetchByIdQuery, TestModel>(new FetchByIdQuery { Id = id });

			if (trainingManual != null && trainingManual.ContentModels.Count() > 0) {

				var chapters = trainingManual.ContentModels;

				foreach (var item in chapters) {
					if (item.Attachments.Any()) {
						foreach (var attachment in item.Attachments) {

							ZipFile(attachment.Id, zipStream, attachment.Name);

						}

					}
				}
			} else if (memo != null && memo.ContentModels.Count() > 0) {
				var chapters = memo.ContentModels;

				foreach (var item in chapters) {
					if (item.Attachments.Any()) {
						foreach (var attachment in item.Attachments) {

							ZipFile(attachment.Id, zipStream, attachment.Name);

						}

					}
				}
			} else if (policy != null && policy.ContentModels.Count() > 0) {
				var chapters = policy.ContentModels;

				foreach (var item in chapters) {
					if (item.Attachments.Any()) {
						foreach (var attachment in item.Attachments) {

							ZipFile(attachment.Id, zipStream, attachment.Name);

						}

					}
				}
			} else if (checkList != null && checkList.ContentModels.Count() > 0) {
				var chapters = checkList.ContentModels;

				foreach (var item in chapters) {
					if (item.Attachments.Any()) {
						foreach (var attachment in item.Attachments) {

							ZipFile(attachment.Id, zipStream, attachment.Name);

						}

					}
				}
			} else {

				var chapters = test?.ContentModels;

				foreach (var item in chapters) {
					if (item.Attachments.Any()) {
						foreach (var attachment in item.Attachments) {

							ZipFile(attachment.Id, zipStream, attachment.Name);

						}

					}
				}
			}

			zipStream.IsStreamOwner = false;
			zipStream.Close();
			memoryStream.Position = 0;
			Response.AddHeader("filename", $"{model.Title.Replace(".xls", "")}.zip");
			return new FileStreamResult(memoryStream, "application/octet-stream");
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

		[HttpPost]
		[ValidateInput(false)]
		public object DocumentWorkFlowMessageSave(CheckListModel model, string message, bool creator, bool? approver, bool? admin, string[] approvers, string action)
		{
			var userId = Thread.CurrentPrincipal.GetId();
			if (Thread.CurrentPrincipal.IsInGlobalAdminRole())
			{
				approver = false;
				creator = false;
				admin = true;
			}
			if (creator)
			{
				model.CreatedBy = userId.ToString();
			}

			if (admin != true && message != "")
			{
				var cs = new SaveOrUpdateDocumentWorkflowAuditMessagesCommand
				{
					DocumentId = model.Id == null ? null : model.Id,
					CreatorId = model.CreatedBy == "null" ? userId : Guid.Parse(model.CreatedBy),
					ApproverId = creator ? Guid.Empty : userId,
					Message = message
				};

				ExecuteCommand(cs);
			}
			if (approvers != null && message != "")
			{
				sendMail(model.CreatedBy, message, model.Id, approvers, creator, approver, admin, action, model);
			}
			else
			{
				string[] approverIds = model.Approver.Split(',');
				sendMail(model.CreatedBy, message, model.Id, approverIds, creator, approver, admin, action, model);
			}

			return null;
		}
		private void sendMail(string creatorId, string message, string documentId, string[] approverIds, bool? isCreator, bool? isApprover, bool? isAdmin, string action, CheckListModel model)
		{
			string subject = null;
			//if admin is delining or accepting document
			if (isAdmin == true)
			{
				//send notification to all approver and content creator
				message = "A document has been " + action + " by the global administrator";

				subject = "A document has been approved";
				addNotification(documentId, creatorId, "A document has been approved", DocumentNotificationType.Accepted.GetDescription());


				//send notification to all approvaers
				foreach (var a in approverIds)
				{
					mail(a, documentId, message, subject, model);

					addNotification(documentId, a, "A document has been approved", DocumentNotificationType.Accepted.GetDescription());
				}

				//send mail to creator
				mail(creatorId, documentId, message, subject, model);

			}
			//if creator submits document for approval
			else if (isCreator == true)
			{

				subject = "A document has been submitted for approval";
				//send notification to all approvers
				foreach (var a in approverIds)
				{
					mail(a, documentId, message, subject, model);
					addNotification(documentId, a, "A document has been submitted for approval", DocumentNotificationType.Assign.GetDescription());
				}

			}
			//if approver accepts or declines document
			else if (isApprover == true)
			{

				if (action == "decline")
				{
					subject = "Document Declined";
					addNotification(documentId, creatorId, "A document has been declined", DocumentNotificationType.Declined.GetDescription());
				}
				else if (action == "accept")
				{
					subject = "Document Approval";
					addNotification(documentId, creatorId, "A document has been approved", DocumentNotificationType.Accepted.GetDescription());
				}

				//send notification to all approvaers
				foreach (var a in approverIds)
				{
					mail(a, documentId, message, subject, model);
					if (action == "decline")
					{
						addNotification(documentId, a, "A document has been declined", DocumentNotificationType.Declined.GetDescription());
					}
					else if (action == "accept")
					{
						addNotification(documentId, a, "A document has been approved", DocumentNotificationType.Accepted.GetDescription());
					}
				}

				//send mail to creator
				mail(creatorId, documentId, message, subject, model);

			}
		}

		private void mail(string userId, string documentId, string message, string subject, CheckListListModel model)
		{
			var approver = new UserViewModel();
			var creator = new UserViewModel();
			var userQueryParameter = new UserQueryParameter
			{
				UserId = Guid.Parse(userId)
			};

			var user = ExecuteQuery<UserQueryParameter, UserViewModel>(userQueryParameter);
			var name = user.FirstName + " " + user.LastName;

			var author = ExecuteQuery<UserQueryParameter, UserViewModel>(new UserQueryParameter
			{
				UserId = Guid.Parse(model.CreatedBy)
			});

			var documentTitles = new List<DocumentTitlesAndTypeQuery>();
			documentTitles.Add(new DocumentTitlesAndTypeQuery
			{
				DocumentTitle = model.Title,
				AdditionalMsg = model.Description,
				DocumentType = model.DocumentType,
				DocumentId = model.Id,
				Author = author.FirstName + " " + author.LastName,
				Points = model.Points,
				Passmark = model.PassMarks
			});

			var eventPublisher = new EventPublisher();
			eventPublisher.Publish(new DocumentWorkflowEvent
			{
				UserViewModel = new UserViewModel() { FirstName = name, EmailAddress = user.EmailAddress },
				CompanyViewModel = PortalContext.Current.UserCompany,
				DocumentTitles = documentTitles,
				Subject = subject,
				Message = message
			});
		}

		private void addNotification(string DocumentId, string UserId, string AdditionalMsg, string notiType)
		{
			var documentNotifications = new List<DocumentNotificationViewModel>();


			var notificationModel = new DocumentNotificationViewModel
			{
				AssignedDate = DateTime.Now,
				IsViewed = false,
				DocId = DocumentId,
				UserId = UserId,
				NotificationType = notiType,
				Message = AdditionalMsg
			};


			documentNotifications.Add(notificationModel);

			ExecuteCommand(documentNotifications);
		}

		[HttpPost]
		public void DeleteAttachment(string CheckListChapterId, string UploadId)
		{
			
				ExecuteCommand(new DeleteCheckListUserUploadResultCommand { CheckListChapterId = CheckListChapterId, UploadId = UploadId });
			
		}
	}

}