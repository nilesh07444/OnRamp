using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Customer.Models.CustomRole;
using Ramp.Contracts.CommandParameter;
using Ramp.Contracts.CommandParameter.CustomUserRole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler {
	public class SaveOrUpdateDocumentWorkflowAuditMessagesCommandHandler : CommandHandlerBase<SaveOrUpdateDocumentWorkflowAuditMessagesCommand> {

		private readonly IRepository<DocumentWorkflowAuditMessages> _audit;

		public SaveOrUpdateDocumentWorkflowAuditMessagesCommandHandler(
			IRepository<DocumentWorkflowAuditMessages> audit)
		{
			_audit = audit;
		}

		public override CommandResponse Execute(SaveOrUpdateDocumentWorkflowAuditMessagesCommand command)
		{
			var response = new CommandResponse();
			response.ErrorMessage = null;
			response.Id = command.Id.ToString();
			response.Validation = null;

			var data = new DocumentWorkflowAuditMessages();
			try
			{
				if (command.Id == Guid.Empty)
				{
					//code to create role

					data.Id = Guid.NewGuid();
					data.DocumentId = command.DocumentId;
					data.CreatorId = command.CreatorId;
					data.ApproverId = command.ApproverId;
					data.Message = command.Message;

					data.DateCreated = DateTime.Now;
					
					_audit.Add(data);
					_audit.SaveChanges();

					response.Id = data.Id.ToString();
				}
				else if (command.Id != Guid.Empty)
				{
					var entity = _audit.Find(command.Id);

					data.Id = Guid.NewGuid();
					data.DocumentId = command.DocumentId;
					data.CreatorId = command.CreatorId;
					data.ApproverId = command.ApproverId;
					data.Message = command.Message;

					entity.DateEdited = DateTime.Now;

					_audit.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				response.ErrorMessage = ex.Message;
			}

			return response;
		}
	}
}
