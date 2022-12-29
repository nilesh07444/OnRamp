using Common.Command;
using Common.Data;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.ScheduleReport;
using Ramp.Contracts.CommandParameter.ScheduleReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.ScheduleReport {
	public class DeleteScheduleReportCommandhandler :
		CommandHandlerBase<DeleteScheduleReportCommand> {
		private readonly IRepository<ScheduleReportModel> _autoRepository;
		//private readonly ICommandDispatcher _commandDispatcher;

		public DeleteScheduleReportCommandhandler(IRepository<ScheduleReportModel> autoRepository)
		{
			_autoRepository = autoRepository;
		}

		public override CommandResponse Execute(DeleteScheduleReportCommand command)
		{
			var reponse = new CommandResponse()
			{
				Id = command.Id.ToString(),
				ErrorMessage = null,
				Validation = null
			};

			try
			{
				var document = _autoRepository.Find(command.Id);

				if (document != null)
				{
					_autoRepository.Delete(document);
					_autoRepository.SaveChanges();
				}
				else
				{
					reponse.ErrorMessage = "No Record Exist";
				}
			}
			catch (Exception ex)
			{
				reponse.ErrorMessage = ex.Message;
			}

			return reponse;
		}
	}
}
