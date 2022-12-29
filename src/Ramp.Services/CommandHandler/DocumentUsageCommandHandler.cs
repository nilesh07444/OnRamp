using System;
using System.Linq;
using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models.Document;
using Ramp.Contracts.Command.DocumentUsage;
using Ramp.Contracts.Events.Account;

namespace Ramp.Services.CommandHandler {
	public class DocumentUsageCommandHandler: ICommandHandlerBase<CreateOrUpdateDocumentUsageCommand>,IEventHandler<StandardUserDeletedEvent>
    {
        private readonly IRepository<DocumentUsage> _documentUsageRepository;

        public DocumentUsageCommandHandler(IRepository<DocumentUsage> documentUsageRepository)
        {
            _documentUsageRepository = documentUsageRepository;
        }
        public CommandResponse Execute(CreateOrUpdateDocumentUsageCommand command)
        {
            var documentUsage = _documentUsageRepository.List.FirstOrDefault(x =>
                x.DocumentType == command.DocumentType &&
                x.DocumentId == command.DocumentId &&
				x.IsGlobalAccessed == command.IsGlobalAccessed &&
                //x.UserId == command.UserId && 
                x.AssignedDocumentId == command.AssignedDocumentId
                );

			if (documentUsage == null)
            {
                if(command.Status != null)
                {
                    _documentUsageRepository.Add(new DocumentUsage
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = command.UserId,
                        DocumentId = command.DocumentId,
                        DocumentType = command.DocumentType,
                        ViewDate = command.ViewDate,
                        Duration = TimeSpan.FromSeconds(0),
                        IsGlobalAccessed = command.IsGlobalAccessed,
                        Status = command.Status,
                        AssignedDocumentId = command.AssignedDocumentId

                    });
                }                
            }
            else
            {
				if (command.Status != null)
                {                    
                    documentUsage.Status = command.Status;
                    documentUsage.Duration = command.Duration;
				}
            }

			//code end neeraj

            _documentUsageRepository.SaveChanges();

            return null;
        }

        public void Handle(StandardUserDeletedEvent @event)
        {
            if (!string.IsNullOrWhiteSpace(@event.Id))
            {
                _documentUsageRepository.List.AsQueryable().Where(x => x.UserId == @event.Id).ToList().ForEach(x => _documentUsageRepository.Delete(x));
                _documentUsageRepository.SaveChanges();
            }
        }
    }
}
