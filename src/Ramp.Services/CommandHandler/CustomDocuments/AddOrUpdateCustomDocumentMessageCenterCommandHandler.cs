using System;
using System.Linq;
using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models;
using Domain.Customer.Models.Document;
using Ramp.Contracts.Command.CustomDocument;
using Ramp.Contracts.Events.Account;

namespace Ramp.Services.CommandHandler {
	public class CustomDocumentMessageCenterCommandHandler: ICommandHandlerBase<CreateOrUpdateCustomDocumentMessageCenterCommand>
    {
        private readonly IRepository<CustomDocumentMessageCenter> _CustomDocumentMessageCenterRepository;
        private readonly IRepository<AssignedDocument> _AssignedDocumentRepository;

        public CustomDocumentMessageCenterCommandHandler(IRepository<CustomDocumentMessageCenter> CustomDocumentMessageCenterRepository, IRepository<AssignedDocument> AssignedDocumentRepository)
        {
            _CustomDocumentMessageCenterRepository = CustomDocumentMessageCenterRepository;
            _AssignedDocumentRepository = AssignedDocumentRepository;
        }
        public CommandResponse Execute(CreateOrUpdateCustomDocumentMessageCenterCommand command)
        {

            var CustomDocumentMessageCenter = _CustomDocumentMessageCenterRepository.List.FirstOrDefault(x =>
                x.UserId == command.UserId && x.DocumentType == command.DocumentType &&
                x.DocumentId == command.DocumentId &&
				x.IsGlobalAccessed == command.IsGlobalAccessed
				);

			if (CustomDocumentMessageCenter == null)
            {
                _CustomDocumentMessageCenterRepository.Add(new CustomDocumentMessageCenter
                {
                    Id = Guid.NewGuid(),
                    UserId = command.UserId,
                    DocumentId = command.DocumentId,
                    DocumentType = command.DocumentType,
                    CreatedOn = DateTime.Now,                    
					IsGlobalAccessed = command.IsGlobalAccessed,
					Status = command.Status,
                    Messages=command.Messages,
                    AssignedDocumentId = command.AssignedDocumentId
                });;
            }
            else
            {
				if (command.Status != null) {
					//below line added by neeraj
					//_CustomDocumentMessageCenterRepository.Delete(CustomDocumentMessageCenter);

					_CustomDocumentMessageCenterRepository.Add(new CustomDocumentMessageCenter
					{
						Id = Guid.NewGuid(),
						UserId = command.UserId,
						DocumentId = command.DocumentId,
						DocumentType = command.DocumentType,						
						IsGlobalAccessed = command.IsGlobalAccessed,
						Status = command.Status,
                        Messages=command.Messages,
                        CreatedOn=DateTime.Now,
                        AssignedDocumentId = command.AssignedDocumentId
                    });

					
				}
            }

			//code end neeraj

            _CustomDocumentMessageCenterRepository.SaveChanges();

            return null;
        }

        //public void Handle(StandardUserDeletedEvent @event)
        //{
        //    if (!string.IsNullOrWhiteSpace(@event.Id))
        //    {
        //        _CustomDocumentMessageCenterRepository.List.AsQueryable().Where(x => x.UserId == @event.Id).ToList().ForEach(x => _CustomDocumentMessageCenterRepository.Delete(x));
        //        _CustomDocumentMessageCenterRepository.SaveChanges();
        //    }
        //}
    }
}
