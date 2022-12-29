using Common;
using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.VirtualClassRooms;
using Ramp.Contracts.Command.VirtualClassRoom;
using Ramp.Contracts.CommandParameter.VirtualClassroom;
using Ramp.Contracts.Query.Document;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler {
	public class VirtualClassRoomCommandHandler : CommandHandlerBase<CreateOrUpdateVirtualClassRoomCommand>,
		 ICommandHandlerBase<DeleteVirtualClassroomCommand> {
		readonly ITransientRepository<VirtualClassRoom> _repository;
		private readonly IQueryExecutor _queryExecutor;
		private readonly ITransientRepository<StandardUser> _standardUserRepository;
		public VirtualClassRoomCommandHandler(IQueryExecutor queryExecutor, ITransientRepository<VirtualClassRoom> repository, ITransientRepository<StandardUser> standardUserRepository) {
			_repository = repository;
			_queryExecutor = queryExecutor;
			 _standardUserRepository = standardUserRepository;
		}
		public override CommandResponse Execute(CreateOrUpdateVirtualClassRoomCommand command) {
			var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
			var standardUser = _standardUserRepository.Find(userId.ConvertToGuid());
			var id = Guid.Parse(command.Id);
			var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.Id == id);

			if (entity==null) {
				entity = new VirtualClassRoom {
					Id = Guid.Parse(command.Id),
					VirtualClassRoomName = command.VirtualClassRoomName,
					Description = command.Description,
					IsPasswordProtection = command.IsPasswordProtection,
					Password = command.Password,
					StartDate = command.StartDate,
					EndDate = command.EndDate,
					IsPublicAccess=command.IsPublicAccess,
					CreatedOn = DateTime.UtcNow,
					JitsiServerName=command.JitsiServerName,
					ReferenceId =
						_queryExecutor.Execute<DocumentReferenceIdQuery, string>(new DocumentReferenceIdQuery()),
					CreatedBy =userId,
					Deleted = false
				};
				_repository.Add(entity);
			} else {
				
				entity.VirtualClassRoomName = command.VirtualClassRoomName;
				entity.Description = command.Description;
				entity.IsPasswordProtection = command.IsPasswordProtection;
				entity.Password = command.Password;
				entity.StartDate = command.StartDate;
				entity.EndDate = command.EndDate;
				entity.IsPublicAccess = command.IsPublicAccess;
				entity.LastEditDate = DateTime.UtcNow;
				entity.LastEditedBy = userId;
				entity.JitsiServerName = command.JitsiServerName;
				
			}
			_repository.SaveChanges();
			return null;
		}
		public CommandResponse Execute(DeleteVirtualClassroomCommand command) {
			var id = Guid.Parse(command.DocumentId);
			var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
			var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.Id == id);
			entity.Deleted = true;
			entity.LastEditDate = DateTime.UtcNow;
			entity.LastEditedBy = userId;
			_repository.SaveChanges();
			return null;
		}
	}
}
