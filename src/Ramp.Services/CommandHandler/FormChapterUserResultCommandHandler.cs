using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models.Forms;
using Ramp.Contracts.Command.Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler
{
    public class FormChapterUserResultCommandHandler : CommandHandlerBase<CreateOrUpdateFormChapterUserResultCommand>
	{
        readonly ITransientRepository<FormChapterUserResult> _repository;

        public FormChapterUserResultCommandHandler(ITransientRepository<FormChapterUserResult> repository)
        {
            _repository = repository;
        }


		public override CommandResponse Execute(CreateOrUpdateFormChapterUserResultCommand command)
		{
			var userId = string.IsNullOrEmpty(command.UserId) ? (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value : command.UserId;

			var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == command.AssignedDocumentId && x.FormChapterId == command.FormChapterId);
			if (entity == null)
			{
				entity = new FormChapterUserResult
				{
					Id = Guid.NewGuid().ToString(),
					AssignedDocumentId = command.AssignedDocumentId,
					FormChapterId = command.FormChapterId,
					CreatedOn = DateTime.UtcNow,
					CompletedOn = DateTime.UtcNow,
					UserId = userId
				};
				_repository.Add(entity);
			}
			else
			{
				entity.CompletedOn = DateTime.UtcNow;
				entity.UserId = userId;
			}
			_repository.SaveChanges();

			return null;
		}
	}
}
