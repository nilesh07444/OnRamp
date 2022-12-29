using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Command.AcrobatField;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler
{
	public class AcrobatFieldValuesCommandHandler : CommandHandlerBase<CreateOrUpdateAcrobatFieldValueCommand>
	{

		readonly ITransientRepository<StandardUserAdobeFieldValues> _repository;

		public AcrobatFieldValuesCommandHandler(ITransientRepository<StandardUserAdobeFieldValues> repository)
		{
			_repository = repository;
		}

		public override CommandResponse Execute(CreateOrUpdateAcrobatFieldValueCommand commandData)
		{
			var userId = (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
			var user_IDs = Guid.Parse(userId);
			foreach (var command in commandData.AcrobatFieldList)
			{

				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == command.DocumentId && x.AcrobatFieldChapterId == command.AcrobatFieldChapterId && x.User_ID== user_IDs && x.Field_Name==command.Field_Name);
				if (entity == null)
				{
					entity = new StandardUserAdobeFieldValues
					{
						Id = Guid.NewGuid().ToString(),
						AcrobatFieldChapterId = command.AcrobatFieldChapterId,
						CreatedOn = DateTime.UtcNow,
						Field_Name = command.Field_Name,
						Field_Value = command.Field_Value,
						DocumentId = command.DocumentId,
						User_ID = Guid.Parse(userId)
					};
					_repository.Add(entity);
				}
				else
				{
					entity.AcrobatFieldChapterId = command.AcrobatFieldChapterId;
					entity.CreatedOn = DateTime.UtcNow;
					entity.Field_Name = command.Field_Name;
					entity.Field_Value = command.Field_Value;
					entity.DocumentId = command.DocumentId;
					entity.User_ID = Guid.Parse(userId);
				}
				_repository.SaveChanges();
			}
			return null;
		}
	}
}
