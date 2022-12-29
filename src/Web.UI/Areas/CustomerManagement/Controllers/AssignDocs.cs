using Common.Data;
using Domain.Customer;
using Domain.Customer.Models.Document;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Web.UI.Controllers;
using Web.UI.Helpers;

namespace Web.UI.Areas.CustomerManagement.Controllers {
	public class AssignDocs: RampController {

		private readonly IRepository<Domain.Customer.Models.Groups.StandardUserGroup> _standardUserGroupRepository;
		private readonly IRepository<AssignedDocument> _assignedDocumentRepository;

		public AssignDocs(IRepository<Domain.Customer.Models.Groups.StandardUserGroup> standardUserGroupRepository, IRepository<AssignedDocument> assignedDocumentRepository)
		{
			_standardUserGroupRepository = standardUserGroupRepository;
			_assignedDocumentRepository = assignedDocumentRepository;
		}
		
		public bool AssignDocsToUser(AddAutoAssignWorkflowViewModel model)
		{
			var allgroups = _standardUserGroupRepository.List.ToList();
			var data = new List<AssignmentViewModel>();

			#region assign docs
			foreach (var doc in model.DocumentList)
			{
				foreach (var group in model.GroupIds)
				{
					//get user list based on group ids
					var users = allgroups.Where(x => x.GroupId.ToString() == group);

					foreach (var user in users)
					{
						DocumentType type;
						Enum.TryParse<DocumentType>(doc.Type, out type);

						var docs = new AssignmentViewModel()
						{
							AssignedDate = DateTime.Now,
							AdditionalMsg = "",
							DocumentId = doc.Id,
							DocumentType = type,
							MultipleAssignedDates = null,
							OrderNumber = doc.Order,
							UserId = user.Id.ToString()
						};

						data.Add(docs);
					}
				}

			}

			var assignedDocuments = _assignedDocumentRepository.List.Where(x => !x.Deleted).ToList();

			var existDocument = data.Where(y => assignedDocuments.Any(z => z.DocumentId == y.DocumentId && z.UserId == y.UserId)).ToList();

			var result = data.Except(existDocument);

			var response = ExecuteCommand(new AssignDocumentsToUsers
			{
				AssignedBy = SessionManager.GetCurrentlyLoggedInUserId().ToString(),
				AssignmentViewModels = result,
				CompanyId = PortalContext.Current.UserCompany.Id,
				AssignedDate = data.FirstOrDefault().AssignedDate == null ? DateTime.UtcNow : data.FirstOrDefault().AssignedDate.Value.ToLocalTime(),
				MultipleAssignedDates = data.FirstOrDefault().MultipleAssignedDates,
			});

			var documentNotifications = new List<DocumentNotificationViewModel>();

			foreach (var m in data)
			{
				var additionalMsg = "";
				var notificationModel = new DocumentNotificationViewModel
				{
					AssignedDate = DateTime.Now,
					IsViewed = false,
					DocId = m.DocumentId,
					UserId = m.UserId,
					NotificationType = DocumentNotificationType.Assign.GetDescription(),
				};

				if (m.AdditionalMsg != "" && m.AdditionalMsg != null)
				{
					var additionalMsgList = m.AdditionalMsg.Split('^').Select(f => new { Id = f.Split('~')[0], Msg = f.Split('~')[1] }).ToList();
					additionalMsg = additionalMsgList.FirstOrDefault(f => f.Id == m.DocumentId).Msg;
					if (additionalMsg != "")
					{
						notificationModel.Message = additionalMsg;
					}
				}
				documentNotifications.Add(notificationModel);
			}
			ExecuteCommand(documentNotifications);


			#endregion

return true;
		}

	}
}