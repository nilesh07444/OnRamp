using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler {
	public class DeleteAutoAssignWorkflowCommandHandler :
		ICommandHandlerBase<DeleteAutoAssignWorkflowCommand> {

		private readonly IRepository<AutoAssignWorkflow> _autoRepository;

		public DeleteAutoAssignWorkflowCommandHandler(IRepository<AutoAssignWorkflow> autoRepository)
		{
			_autoRepository = autoRepository;
		}


		public CommandResponse Execute(DeleteAutoAssignWorkflowCommand command)
		{
			var response = new CommandResponse();

			try
			{
				var entry = _autoRepository.Find(Guid.Parse(command.Id));

				if (entry !=null)
				{
					//_autoRepository.Delete(entry);
					entry.IsDeleted = true;
					_autoRepository.SaveChanges();
				}
				else
				{
					response.ErrorMessage = "No data found";
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
