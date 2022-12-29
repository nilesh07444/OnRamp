using Common.Query;

using Ramp.Contracts.ViewModel;
using Web.UI.Code.Extensions;
using System.Collections.Generic;
using System.Linq;
using Ramp.Contracts.Query.Label;
using System.Web.Mvc;
using System.Threading;
using Domain.Customer;
using Ramp.Contracts.Command.DocumentUsage;
using System;


using Ramp.Contracts.Query.TrainingManual;
using Domain.Customer.Models.TrainingManual;
using Ramp.Contracts.Command.TrainingManual;

using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.CommandParameter;
using System.Configuration;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Security.Authorization;
using Ramp.Services.Helpers;
using Common.Events;
using Ramp.Contracts.Events.DocumentWorkflow;
using Web.UI.Helpers;
using Ramp.Contracts.Query.Document;

namespace Web.UI.Controllers {
	public class TrainingManualController : KnockoutDocumentController<TrainingManualListQuery, TrainingManualListModel, TrainingManual, TrainingManualModel, CreateOrUpdateTrainingManualCommand> {
		public override void Edit_PostProcess(TrainingManualModel model, string companyId = null, DocumentUsageStatus? status = null, string userid = null)
		{
			if (model.CoverPicture != null)
			{
				model.CoverPicture.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(model.CoverPicture.Id, 300, 300, companyId));
				model.CoverPicture.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(model.CoverPicture.Id, companyId));
			}
			model.ContentModels.ToList().ForEach(content =>
			{
				content.Attachments.ToList().ForEach(attachment =>
				{
					attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
					attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
					attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));


				});
				content.ContentToolsUploads.ToList().ForEach(attachment =>
				{
					attachment.url = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.url, null, null, companyId));
				});
			});
			var trainigLabels = new List<string>();

			foreach (var item in model.TrainingLabels.Split(','))
			{
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

		protected override void Preview_PostProcess(TrainingManualModel model, string companyId = null, string checkUser = null, bool isGlobal = false, DocumentUsageStatus? status = null)
		{

			ViewBag.IsGlobalAccessed = model.IsGlobalAccessed;
			if (model.Status.ToString() == "Viewed") { ViewBag.Show = false; }
			else { ViewBag.Show = true; }

			Edit_PostProcess(model, companyId);
		}

		[System.Web.Http.HttpPost]
		[ValidateInput(false)]
		public object CompleteTrainingMannual(TrainingManualModel model, DocumentUsageStatus? status = null, bool IsGlobalAccessed = false)
		{
			var userId = Thread.CurrentPrincipal.GetId().ToString();

			var viewDate = DateTime.UtcNow;
			//viewDate = DateTime.SpecifyKind(DateTime.ParseExact(viewDate.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null), DateTimeKind.Utc); // remove milliseconds

			ExecuteCommand(new CreateOrUpdateDocumentUsageCommand
			{
				DocumentId = model.Id.ToString(),
				DocumentType = DocumentType.TrainingManual,
				UserId = userId,
				ViewDate = viewDate,
				IsGlobalAccessed = IsGlobalAccessed,
				Status = status
			});

			return null;

		}

		[HttpPost]
		[ValidateInput(false)]
		public object DocumentWorkFlowMessageSave(TrainingManualModel model, string message, bool creator, bool? approver, bool? admin, string[] approvers, string action)
		{
			var userId = Thread.CurrentPrincipal.GetId();

			if (Thread.CurrentPrincipal.IsInGlobalAdminRole()){
				approver = false;
				creator = false;
				admin = true;
			}

			if (creator)
			{
				model.CreatedBy = userId.ToString() ;
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
		private void sendMail(string creatorId, string message, string documentId, string[] approverIds, bool? isCreator, bool? isApprover, bool? isAdmin, string action, TrainingManualModel model)
		{
			string subject = null;
			//if admin is delining or accepting document
			if (isAdmin == true)
			{
				//send notification to all approver and content creator
				message = "A document has been " + action + " by the global Administrator";
			
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
					mail(a, documentId, message, subject,model);
					addNotification(documentId, a, "A document has been submitted for approval", DocumentNotificationType.Assign.GetDescription());
				}

			}
			//if approver accepts or declines document
			else if (isApprover == true)
			{

				if (action == "decline")
				{
					subject = "A document has been declined";
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
					mail(a, documentId,message, subject, model);
					if (action == "decline")
					{
						addNotification(documentId, a, "A document has been declined", DocumentNotificationType.Declined.GetDescription());
					}
					else if(action == "accept")
					{
						addNotification(documentId, a, "A document has been approved", DocumentNotificationType.Accepted.GetDescription());
					}
				}

				//send mail to creator
				mail(creatorId, documentId, message, subject, model);

			}
		}

		private void mail(string userId, string documentId, string message, string subject, TrainingManualModel model)
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
			documentTitles.Add(new DocumentTitlesAndTypeQuery {
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
	}


}