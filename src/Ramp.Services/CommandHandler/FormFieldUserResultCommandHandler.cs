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
	public class FormFieldUserResultCommandHandler : CommandHandlerBase<CreateOrUpdateFormFieldUserResultCommand>
	{
		readonly ITransientRepository<FormField> _repository;
		private readonly ITransientRepository<FormFiledUserResult> _formFiledUserResultRepository;

		public FormFieldUserResultCommandHandler(ITransientRepository<FormField> repository, ITransientRepository<FormFiledUserResult> formFiledUserResultRepository)
		{
			_repository = repository;
			_formFiledUserResultRepository = formFiledUserResultRepository;
		}

		public override CommandResponse Execute(CreateOrUpdateFormFieldUserResultCommand command)
		{
			var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;


			var formField = _formFiledUserResultRepository.List.AsQueryable().FirstOrDefault(x => x.FormChapterId == command.FormChapterId && x.FormFieldId == command.Id /*&& x.UserId == userId*/ && x.AssignedId == command.AssignedId);

			if (formField == null)
			{
				formField = new FormFiledUserResult
				{
					Id = Guid.NewGuid().ToString(),
					Value = command.FormFieldDescription,
					FormChapterId = command.FormChapterId,
					UserId = userId,
					FormFieldId = command.FormFiledId,
					//Number = command.Number,
					AssignedId = command.AssignedId
				};
				_formFiledUserResultRepository.Add(formField);
			}
			else
			{
				formField.Value = command.FormFieldDescription;
				//formField.UserId = userId;
				formField.FormFieldId = command.FormFiledId;
			}
			_formFiledUserResultRepository.SaveChanges();

			return null;
		}
	}
}
