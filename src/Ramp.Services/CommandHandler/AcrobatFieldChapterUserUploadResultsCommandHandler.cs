using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Command.AcrobatField;
using Ramp.Contracts.CommandParameter.AcrobatField;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler
{
    public class AcrobatFieldChapterUserUploadResultsCommandHandler : CommandHandlerBase<CreateOrUpdateAcrobatFieldChapterUserUploadResultsCommand>,
         ICommandHandlerBase<DeleteAcrobatFieldUserUploadResultCommand>
    {

        readonly ITransientRepository<AcrobatFieldChapterUserUploadResult> _repository;

        public AcrobatFieldChapterUserUploadResultsCommandHandler(ITransientRepository<AcrobatFieldChapterUserUploadResult> repository)
        {
            _repository = repository;
        }

        public override CommandResponse Execute(CreateOrUpdateAcrobatFieldChapterUserUploadResultsCommand command)
        {
            var userId = string.IsNullOrEmpty(command.UserId) ? (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value : command.UserId;
            if (command.IsGlobalAccessed)
            {
                var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == command.DocumentId && x.AcrobatFieldChapterId == command.AcrobatFieldChapterId && x.UploadId == command.UploadId);
                if (entity == null)
                {
                    entity = new AcrobatFieldChapterUserUploadResult
                    {
                        Id = Guid.NewGuid().ToString(),
                        AssignedDocumentId = command.AssignedDocumentId,
                        AcrobatFieldChapterId = command.AcrobatFieldChapterId,
                        CreatedDate = DateTime.UtcNow,
                        UploadId = command.UploadId,
                        DocumentId = command.DocumentId,
                        IsGlobalAccessed = command.IsGlobalAccessed,
                        UserId = userId,
                        isSignOff = command.isSignOff

                    };
                    _repository.Add(entity);
                }
                else
                {

                    entity.UploadId = command.UploadId;
                }
            }
            else
            {
                var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == command.AssignedDocumentId && x.AcrobatFieldChapterId == command.AcrobatFieldChapterId && x.UploadId == command.UploadId);
                if (entity == null)
                {
                    entity = new AcrobatFieldChapterUserUploadResult
                    {
                        Id = Guid.NewGuid().ToString(),
                        AssignedDocumentId = command.AssignedDocumentId,
                        AcrobatFieldChapterId = command.AcrobatFieldChapterId,
                        CreatedDate = DateTime.UtcNow,
                        UploadId = command.UploadId,
                        DocumentId = command.DocumentId,
                        IsGlobalAccessed = command.IsGlobalAccessed,
                        UserId = userId,
                        isSignOff = command.isSignOff
                    };
                    _repository.Add(entity);
                }
                else
                {

                    entity.UploadId = command.UploadId;
                }
            }
            _repository.SaveChanges();
            return null;
        }
        public CommandResponse Execute(DeleteAcrobatFieldUserUploadResultCommand command)
        {
            if (command.IsGlobalAccessed)
            {
                var entity = _repository.List.AsQueryable().Where(x => x.DocumentId == command.DocumentId && x.AcrobatFieldChapterId == command.AcrobatFieldChapterId).ToList();
                if (entity != null)
                {
                    foreach (var item in entity)
                    {
                        _repository.Delete(item);
                    }
                }
                _repository.SaveChanges();
            }
            else
            {
                var entity = _repository.List.AsQueryable().Where(x => x.AcrobatFieldChapterId == command.AcrobatFieldChapterId && x.UploadId == command.UploadId).ToList();
                if (entity != null)
                {
                    foreach (var item in entity)
                    {
                        _repository.Delete(item);
                    }
                }
                _repository.SaveChanges();
            }
            return null;
        }

    }
}
