using Common.Command;
using Common.Data;
using Domain.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.CustomDocument;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ramp.Services.CommandHandler.CustomDocuments {
	public class DeleteCustomDocumentCommandHandler : CommandHandlerBase<DeleteCustomDoumentCommand>, ICommandHandlerAndValidator<DeleteByIdCommand<CustomDocument>>
	{

		private readonly IRepository<CustomDocument> _customDocumentRepository;

		public DeleteCustomDocumentCommandHandler(IRepository<CustomDocument> customDocumentRepository)
		{
			_customDocumentRepository = customDocumentRepository;
		}

		public override CommandResponse Execute(DeleteCustomDoumentCommand command)
		{
			var reponse = new CommandResponse()
			{
				Id = command.Id.ToString(),
				ErrorMessage = null,
				Validation = null
			};

			try
			{
				var document = _customDocumentRepository.Find(command.Id);

				if (document != null)
				{
					_customDocumentRepository.Delete(document);
					_customDocumentRepository.SaveChanges();
				}
				else
				{
					reponse.ErrorMessage = "No Record Exist";
				}
			}
			catch(Exception ex)
			{
				reponse.ErrorMessage = ex.Message;
			}

			return reponse;
		}

		public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<CustomDocument> argument)
		{
			if (_customDocumentRepository.Find(argument.Id) == null)
				yield return new ValidationResult("Id", $"Cannot find Custom Document with id : {argument.Id}");
		}

		public CommandResponse Execute(DeleteByIdCommand<CustomDocument> command)
		{
			var reponse = new CommandResponse()
			{
				Id = command.Id.ToString(),
				ErrorMessage = null,
				Validation = null
			};

			try
			{
				var document = _customDocumentRepository.Find(command.Id);

				if (document != null)
				{
					//	_customDocumentRepository.Delete(document);
					document.Deleted = true;
					_customDocumentRepository.SaveChanges();
				}
				else
				{
					reponse.ErrorMessage = "No Record Exist";
				}
			}
			catch (Exception ex)
			{
				reponse.ErrorMessage = ex.Message;
			}

			return reponse;
		}
	}
}
