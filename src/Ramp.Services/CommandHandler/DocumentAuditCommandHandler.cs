using Common;
using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.DocumentTrack;
using Ramp.Contracts.Command.DocumentAudit;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler {
	public class DocumentAuditCommandHandler : CommandHandlerBase<CreateOrUpdateDocumentAuditCommand> {
		readonly ITransientRepository<DocumentAuditTrack> _repository;
		private readonly IQueryExecutor _queryExecutor;
		private readonly ITransientRepository<StandardUser> _standardUserRepository;

		public DocumentAuditCommandHandler(IQueryExecutor queryExecutor, ITransientRepository<DocumentAuditTrack> repository, ITransientRepository<StandardUser> standardUserRepository) {
			_repository = repository;
			_queryExecutor = queryExecutor;
			_standardUserRepository = standardUserRepository;
		}

		public override CommandResponse Execute(CreateOrUpdateDocumentAuditCommand command) {
			var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
			var standardUser = _standardUserRepository.Find(userId.ConvertToGuid());
			var id = Guid.Parse(command.Id);
			var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.Id == id);

			if (entity == null) {
				entity = new DocumentAuditTrack {
					Id = Guid.Parse(command.Id),
					LastEditedBy = userId,
					LastEditDate = DateTime.UtcNow,
					DocumentId=command.DocumentId,
					DocumentStatus=command.DocumentStatus,
					User=standardUser,
					UserName=standardUser.LastName+" "+standardUser.FirstName 
									
				};
				_repository.Add(entity);
			} else {

				entity.LastEditedBy = userId;
				entity.LastEditDate = DateTime.UtcNow;
				entity.DocumentId = command.DocumentId;
				entity.DocumentStatus = command.DocumentStatus;
				entity.User = standardUser;
				entity.UserName = standardUser.LastName + " " + standardUser.FirstName;

			}
			_repository.SaveChanges();
			return null;
		}
	}
}
