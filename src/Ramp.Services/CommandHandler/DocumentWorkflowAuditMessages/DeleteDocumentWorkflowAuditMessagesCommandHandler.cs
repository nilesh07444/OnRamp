using Common.Command;
using Common.Data;
using Ramp.Contracts.CommandParameter;
using System.Linq;
using Domain.Customer.Models.CustomRole;
using System;
using Domain.Customer.Models;

namespace Ramp.Services.CommandHandler {
	public class DeleteDocumentWorkflowAuditMessagesCommandHandler :
		CommandHandlerBase<DeleteDocumentWorkflowAuditMessagesCommand> {

		private readonly IRepository<DocumentWorkflowAuditMessages> _audit;

		public DeleteDocumentWorkflowAuditMessagesCommandHandler(
			IRepository<DocumentWorkflowAuditMessages> audit)
		{
			_audit = audit;
		}

		public override CommandResponse Execute(DeleteDocumentWorkflowAuditMessagesCommand command)
		{
			var response = new CommandResponse();
			response.ErrorMessage = null;
			response.Id = command.Id.ToString();
			response.Validation = null;

			var entity = _audit.List.Where(x => x.Id == command.Id).FirstOrDefault();

			try
			{
				if (entity != null)
				{
					_audit.Delete(entity);
					_audit.SaveChanges();

					response.Id = command.Id.ToString();
					response.ErrorMessage = null;
					response.Validation = null;

				}
				else
				{
					response.Id = command.Id.ToString();
					response.ErrorMessage = "No record exist";
					response.Validation = null;
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
