using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter;
using System;
using System.Threading;

namespace Ramp.Services.CommandHandler {
	public class AddOrUpdateCourseCommandHandler : CommandHandlerBase<AddOrUpdateCourseCommand> {

		private readonly IRepository<Course> _course;

		public AddOrUpdateCourseCommandHandler(IRepository<Course> course)
		{
			_course = course;
		}

		public override CommandResponse Execute(AddOrUpdateCourseCommand command)
		{
			var response = new CommandResponse();
			response.Id = command.Id.ToString();

			try
			{
				if (command.Id == null || command.Id == Guid.Empty)
				{
					//add
					var entry = new Course()
					{
						Id = Guid.NewGuid(),
						Title = command.Title,
						Description = command.Description,
						CreatedBy = command.CreatedBy,
						CreatedOn = DateTime.Now,
						Status = command.Status,
						Points = command.Points,
						GlobalAccessEnabled = command.GlobalAccessEnabled,
						ExpiryEnabled = command.ExpiryEnabled,
						ExpiryInDays = command.ExpiryInDays,
						IsActive = true

					};

					_course.Add(entry);
					_course.SaveChanges();

				}
				else if (command.Id != null)
				{
					//update
					var entry = _course.Find(command.Id);

					if (entry != null)
					{
						entry.Id = Guid.NewGuid();
						entry.Title = command.Title;
						entry.Description = command.Description;
						entry.CreatedBy = command.CreatedBy;
						entry.CreatedOn = DateTime.Now;
						entry.Status = command.Status;
						entry.Points = command.Points;
						entry.GlobalAccessEnabled = command.GlobalAccessEnabled;
						entry.ExpiryEnabled = command.ExpiryEnabled;
						entry.ExpiryInDays = command.ExpiryInDays;
						entry.IsActive = true;

						_course.SaveChanges();

					}
					else if (entry == null)
					{
						response.ErrorMessage = "Record does not exist";
					}
					
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
