using Common;
using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.VirtualClassRooms;
using Ramp.Contracts.CommandParameter.ExternalMeetingUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler {
	public class ExternalMeetingUserCommandHandler : CommandHandlerBase<CreateExternalMeetingUserCommand> {
		readonly ITransientRepository<ExternalMeetingUser> _repository;
		private readonly ITransientRepository<StandardUser> _standardUserRepository;
		public ExternalMeetingUserCommandHandler(ITransientRepository<ExternalMeetingUser> repository, ITransientRepository<StandardUser> standardUserRepository) {
			_repository = repository;
			_standardUserRepository = standardUserRepository;
		}
		public override CommandResponse Execute(CreateExternalMeetingUserCommand command) {

			var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.Id == command.Id);

			if (entity == null) {
				entity = new ExternalMeetingUser {
					Id = Guid.NewGuid(),
					CreatedOn = DateTime.UtcNow,
					UserId= command.UserId,
					MeetingId=command.MeetingId,
					EmailAddress=command.EmailAddress
				};
				_repository.Add(entity);
			} 
			_repository.SaveChanges();
			return null;
		}
	}
}
