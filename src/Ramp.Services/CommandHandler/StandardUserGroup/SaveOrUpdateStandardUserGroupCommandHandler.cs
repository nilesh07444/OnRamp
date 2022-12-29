using Common.Command;
using Common.Data;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Groups;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Command.User;
using Ramp.Contracts.CommandParameter.Group;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ramp.Services.CommandHandler.Group
{

	public class SaveOrUpdateStandardUserGroupCommandHandler : CommandHandlerBase<SaveOrUpdateStandardUserGroupCommand>,
		ICommandHandlerBase<StandardUserGroupUpdateParentCommand>
	//ICommandHandlerBase<UserUpdateGroupCommand>
	{
		private readonly IRepository<StandardUserGroup> _groupRepository;
		private readonly IRepository<StandardUser> _userRepository;
		private readonly IReadRepository<AutoAssignWorkflow> _autoAssignWorkFlowRepository;
		private readonly IReadRepository<AutoAssignGroups> _autoAssignWorkFlowGroupsRepository;
		private readonly IReadRepository<AutoAssignDocuments> _autoAssignWorkFlowDocsRepository;
		private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public SaveOrUpdateStandardUserGroupCommandHandler(IRepository<StandardUserGroup> groupRepository, IRepository<StandardUser> userRepository, IReadRepository<AutoAssignWorkflow> autoAssignWorkFlowRepository,
			IReadRepository<AutoAssignGroups> autoAssignWorkFlowGroupsRepository,
			IReadRepository<AutoAssignDocuments> autoAssignWorkFlowDocsRepository,
			IRepository<AssignedDocument> assignedDocumentRepository,
			ICommandDispatcher commandDispatcher)
		{
			_groupRepository = groupRepository;
			_userRepository = userRepository;
			_autoAssignWorkFlowRepository = autoAssignWorkFlowRepository;
			_autoAssignWorkFlowGroupsRepository = autoAssignWorkFlowGroupsRepository;
			_autoAssignWorkFlowDocsRepository = autoAssignWorkFlowDocsRepository;

			_assignedDocumentRepository = assignedDocumentRepository;

			_commandDispatcher = commandDispatcher;
		}

		public override CommandResponse Execute(SaveOrUpdateStandardUserGroupCommand command)
		{

			if (command.UserId != null && command.GroupId != null)
			{
				var entity = _groupRepository.List.Where(x => x.UserId == command.UserId).AsQueryable().ToList();

				if (entity == null)
				{
					command.GroupId.ForEach(x => {
						var data = new StandardUserGroup
						{
							Id = Guid.NewGuid(),
							UserId = command.UserId,
							GroupId = Guid.Parse(x),
							DateCreated = DateTime.Now

						};

						//#region autoassignworkflow document assignment
						//var docs = new List<AutoAssignDocuments>();

						////get all the workflow for a given group
						//var groups = _autoAssignWorkFlowGroupsRepository.List.Where(r => r.GroupId.ToString() == x.ToString()).ToList();

						//foreach (var group in groups)
						//{
						//	//get all the documents for a given workflow
						//	var d = _autoAssignWorkFlowDocsRepository.List.Where(r => r.WorkFlowId == group.WorkFlowId).ToList();

						//	docs.AddRange(d);
						//}

						//var data1 = new List<AssignmentViewModel>();

						//foreach (var doc in docs)
						//{
						//	var docs1 = new AssignmentViewModel()
						//	{
						//		AssignedDate = DateTime.Now,
						//		AdditionalMsg = "",
						//		DocumentId = doc.Id.ToString(),
						//		DocumentType = (DocumentType)doc.Type,
						//		MultipleAssignedDates = null,
						//		OrderNumber = doc.Order,
						//		UserId = command.UserId.ToString()
						//	};

						//	data1.Add(docs1);
						//}

						//var assignedDocuments = _assignedDocumentRepository.List.Where(xr => !xr.Deleted).ToList();

						//var existDocument = data1.Where(y => assignedDocuments.Any(z => z.DocumentId == y.DocumentId && z.UserId == y.UserId)).ToList();

						//var result = data1.Except(existDocument);

						//var response = _commandDispatcher.Dispatch(new AssignDocumentsToUsers
						//{
						//	AssignedBy = null,
						//	AssignmentViewModels = result,
						//	CompanyId = _userRepository.Find(command.UserId).CompanyId,
						//	AssignedDate = data1.FirstOrDefault().AssignedDate == null ? DateTime.UtcNow : data1.FirstOrDefault().AssignedDate.Value.ToLocalTime(),
						//	MultipleAssignedDates = data1.FirstOrDefault().MultipleAssignedDates,
						//});

						//var documentNotifications = new List<DocumentNotificationViewModel>();

						//foreach (var m in data1)
						//{
						//	//DocumentType type;
						//	//Enum.TryParse<DocumentType>(doc.Type, out type);
						//	var additionalMsg = "";
						//	var notificationModel = new DocumentNotificationViewModel
						//	{
						//		AssignedDate = DateTime.Now,
						//		IsViewed = false,
						//		DocId = m.DocumentId,
						//		UserId = m.UserId,
						//		NotificationType = m.DocumentType.ToString(),
						//	};

						//	if (m.AdditionalMsg != "" && m.AdditionalMsg != null)
						//	{
						//		var additionalMsgList = m.AdditionalMsg.Split('^').Select(f => new { Id = f.Split('~')[0], Msg = f.Split('~')[1] }).ToList();
						//		additionalMsg = additionalMsgList.FirstOrDefault(f => f.Id == m.DocumentId).Msg;
						//		if (additionalMsg != "")
						//		{
						//			notificationModel.Message = additionalMsg;
						//		}
						//	}
						//	documentNotifications.Add(notificationModel);
						//}
						//_commandDispatcher.Dispatch(documentNotifications);
						//#endregion

						_groupRepository.Add(data);
					});

					_groupRepository.SaveChanges();


				}
				else
				{
					entity.ForEach(x => {
						_groupRepository.Delete(x);
					});
					_groupRepository.SaveChanges();

					command.GroupId.ForEach(x => {
						var data = new StandardUserGroup
						{
							Id = Guid.NewGuid(),
							UserId = command.UserId,
							GroupId = Guid.Parse(x),
							DateCreated = DateTime.Now

						};
						_groupRepository.Add(data);
					});

					_groupRepository.SaveChanges();
				}
				//_groupRepository.SaveChanges();

			}
			return null;
		}

		public CommandResponse Execute(UserUpdateGroupCommand command)
		{
			//var group = _groupRepository.Find(Guid.Parse(command.GroupId));
			//var user = _userRepository.Find(Guid.Parse(command.Id));
			//user.Group = group;
			//_userRepository.SaveChanges();

			return null;
		}

		public CommandResponse Execute(StandardUserGroupUpdateParentCommand command)
		{
			try
			{
				//var category = _groupRepository.Find(Guid.Parse(command.Id));
				//category.ParentId = command.ParentId;
				//_groupRepository.SaveChanges();
			}
			catch (Exception e)
			{
				throw e;
			}
			return null;
		}

	}
}