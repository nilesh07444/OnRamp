using Common.Command;
using Common.Data;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Document;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.CommandParameter;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler {
	public class AddOrUpdateAutoAssignWorkFlowCommandHandler :
		ICommandHandlerBase<SaveOrUpdateAutoAssignWorkflowCommand> {

		private readonly IRepository<AutoAssignWorkflow> _autoRepository;
		private readonly IRepository<AutoAssignDocuments> _docsRepository;
		private readonly IRepository<AutoAssignGroups> _grpRepository;
		private readonly IRepository<Domain.Customer.Models.Groups.StandardUserGroup> _standardUserGroupRepository;
		private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public AddOrUpdateAutoAssignWorkFlowCommandHandler(
			IRepository<AutoAssignWorkflow> autoRepository,
			IRepository<AutoAssignDocuments> docsRepository,
			IRepository<AutoAssignGroups> grpRepository,
			IRepository<Domain.Customer.Models.Groups.StandardUserGroup> standardUserGroupRepository, IRepository<AssignedDocument> assignedDocumentRepository,
			ICommandDispatcher commandDispatcher
			)
		{
			_autoRepository = autoRepository;
			_docsRepository = docsRepository;
			_grpRepository = grpRepository;
			_standardUserGroupRepository = standardUserGroupRepository;
			_assignedDocumentRepository = assignedDocumentRepository;
			
			_commandDispatcher = commandDispatcher;
		}

		public CommandResponse Execute(SaveOrUpdateAutoAssignWorkflowCommand command)
		{
			var temp = command;
			var response = new CommandResponse();

			try
			{
				if (command.Id == null)
				{
					//add
					var entry = new AutoAssignWorkflow();

					entry.Id = Guid.NewGuid();
					entry.IsDeleted = false;
					entry.WorkflowName = command.WorkflowName;
					entry.DateCreated = DateTime.Now;
					entry.SendNotiEnabled = command.SendNotiEnabled;
					_autoRepository.Add(entry);

					AddDocs(entry.Id, command.DocumentListID, false);
					AddGroups(entry.Id, command.GroupId, false);
				}
				else if (command.Id != null)
				{
					//update
					var entry = _autoRepository.Find(Guid.Parse(command.Id));

					entry.IsDeleted = false;
					entry.WorkflowName = command.WorkflowName;
					entry.SendNotiEnabled = command.SendNotiEnabled;
					AddDocs(entry.Id, command.DocumentListID, true);
					AddGroups(entry.Id, command.GroupId, true);
				}
				
				_autoRepository.SaveChanges();

				//AssignDocsToUser(temp);

			}
			catch (Exception ex)
			{
				response.ErrorMessage = ex.Message;
			}

			return response;
		}

		private void AddGroups(Guid autoId, string[] groups, bool check)
		{
			try
			{
				if (check)
				{
					//delete
					var eg = _grpRepository.List.Where(x => x.WorkFlowId == autoId).ToList();
					foreach (var e in eg)
					{
						_grpRepository.Delete(e);
					}
					_grpRepository.SaveChanges();
				}
				//add
				foreach (var g in groups)
				{
					var entry = new AutoAssignGroups();

					entry.Id = Guid.NewGuid();
					entry.WorkFlowId = autoId;
					entry.GroupId = Guid.Parse(g);

					_grpRepository.Add(entry);
				}
				_grpRepository.SaveChanges();
			}
			catch (Exception ex)
			{

				throw;
			}
			
		}
	

		private void AddDocs(Guid autoId, List<AutoWorkFlowDocs> docs, bool check)
		{
			try
			{
				if (check)
				{
					//delete
					var eg = _docsRepository.List.Where(x => x.WorkFlowId == autoId).ToList();
					foreach (var e in eg)
					{
						_docsRepository.Delete(e);
					}
					_docsRepository.SaveChanges();
				}
				//add
				foreach (var d in docs)
				{
					var entry = new AutoAssignDocuments();

					var type = Enum.TryParse(d.Type, out DocumentType t);

					entry.Id = Guid.NewGuid();
					entry.WorkFlowId = autoId;
					entry.DocumentId = Guid.Parse(d.Id);
					entry.Type = (int)t;
					entry.Order = d.Order;

					_docsRepository.Add(entry);
				}
				_docsRepository.SaveChanges();
			}
			catch (Exception ex)
			{

				throw;
			}
			
		}

		private void  AssignDocsToUser(SaveOrUpdateAutoAssignWorkflowCommand model)
		{
			var allgroups = _standardUserGroupRepository.List.ToList();
			var data = new List<AssignmentViewModel>();

			try
			{
				#region assign docs
				foreach (var doc in model.DocumentListID)
				{
					foreach (var group in model.GroupId)
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

				var response = _commandDispatcher.Dispatch(new AssignDocumentsToUsers
				{
					AssignedBy = model.AssignedBy,
					AssignmentViewModels = result,
					CompanyId = Guid.Parse(model.ComapnyId),
					AssignedDate = data.FirstOrDefault().AssignedDate == null ? DateTime.UtcNow : data.FirstOrDefault().AssignedDate.Value.ToLocalTime(),
					MultipleAssignedDates = data.FirstOrDefault().MultipleAssignedDates,
				});

				var documentNotifications = new List<DocumentNotificationViewModel>();

				foreach (var m in data)
				{
					//DocumentType type;
					//Enum.TryParse<DocumentType>(doc.Type, out type);
					var additionalMsg = "";
					var notificationModel = new DocumentNotificationViewModel
					{
						AssignedDate = DateTime.Now,
						IsViewed = false,
						DocId = m.DocumentId,
						UserId = m.UserId,
						NotificationType = m.DocumentType.ToString(),
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
				_commandDispatcher.Dispatch(documentNotifications);


				#endregion
			}
			catch (Exception e)
			{

				throw;
			}
			

		}
	}
}
