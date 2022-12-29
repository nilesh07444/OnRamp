using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.Course;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler {
	public class DeleteCourseCommandHandler : CommandHandlerBase<DeleteCourseCommand> {

		private readonly IRepository<Course> _courseRepository;

		public DeleteCourseCommandHandler(IRepository<Course> courseRepository)
		{
			_courseRepository = courseRepository;
		}

		public override CommandResponse Execute(DeleteCourseCommand command)
		{
			var reponse = new CommandResponse()
			{
				Id = command.Id.ToString(),
				ErrorMessage = null,
				Validation = null
			};

			try
			{
				var document = _courseRepository.Find(command.Id);

				if (document != null)
				{
					_courseRepository.Delete(document);
					_courseRepository.SaveChanges();
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
