using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Policy;
using Ramp.Contracts.Command.Policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler
{
	public class PolicyContentBoxUserResultCommandHandler : CommandHandlerBase<CreateOrUpdatePolicyContentBoxUserResultCommand>,
		ICommandHandlerAndValidator<DeleteByIdCommand<PolicyContentBox>>
	{
		readonly ITransientRepository<PolicyContentBoxUserResult> _repository;
		readonly ITransientRepository<PolicyContentBox> _policycontentboxrepository;

		public PolicyContentBoxUserResultCommandHandler(ITransientRepository<PolicyContentBoxUserResult> repository, ITransientRepository<PolicyContentBox> policycontentboxrepository)
		{
			_repository = repository;
			_policycontentboxrepository = policycontentboxrepository;
		}


		public override CommandResponse Execute(CreateOrUpdatePolicyContentBoxUserResultCommand command)
		{
			var userId = string.IsNullOrEmpty(command.UserId) ? (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value : command.UserId;

			if (command.IsGlobalAccessed)
			{
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == command.DocumentId && x.PolicyContentBoxId == command.PolicyContentBoxId);
				if (entity == null)
				{
					entity = new PolicyContentBoxUserResult
					{
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						PolicyContentBoxId = command.PolicyContentBoxId,
						IsChecked = command.IsChecked,
						CreatedDate = DateTime.UtcNow,
						IssueDiscription = command.IssueDiscription,
						DocumentId = command.DocumentId,
						DateCompleted = DateTime.UtcNow,
						IsGlobalAccessed = command.IsGlobalAccessed,
						UserId = userId,

						//added by softdue
						IsActionNeeded = command.IsActionNeeded

				};
					if (entity.IsChecked)
					{
						entity.ChapterTrackedDate = DateTime.UtcNow;
					}
					else
					{
						entity.ChapterTrackedDate = null;
					}
					_repository.Add(entity);
				}
				else
				{
					entity.DateCompleted = DateTime.UtcNow;
					entity.IsChecked = command.IsChecked;
					entity.UserId = userId;
					entity.IssueDiscription = command.IssueDiscription;

					//added by softdue
					entity.IsActionNeeded = command.IsActionNeeded;

					if (entity.IsChecked && entity.ChapterTrackedDate == null)
					{
						entity.ChapterTrackedDate = DateTime.UtcNow;
					}
					else if (!entity.IsChecked)
					{
						entity.ChapterTrackedDate = null;
					}
				}
				_repository.SaveChanges();
			}
			else
			{
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == command.AssignedDocumentId && x.PolicyContentBoxId == command.PolicyContentBoxId);
				if (entity == null)
				{
					entity = new PolicyContentBoxUserResult
					{
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						PolicyContentBoxId = command.PolicyContentBoxId,
						IsChecked = command.IsChecked,
						CreatedDate = DateTime.UtcNow,
						DateCompleted = DateTime.UtcNow,
						UserId = userId,
						IssueDiscription = command.IssueDiscription,
						DocumentId = command.DocumentId,
						IsGlobalAccessed = command.IsGlobalAccessed,

						//added by softdue
						IsActionNeeded = command.IsActionNeeded

					};
					if (entity.IsChecked)
					{
						entity.ChapterTrackedDate = DateTime.UtcNow;
					}
					else
					{
						entity.ChapterTrackedDate = null;
					}

					_repository.Add(entity);
				}
				else
				{
					entity.DateCompleted = DateTime.UtcNow;
					entity.UserId = userId;
					entity.IsChecked = command.IsChecked;
					entity.IssueDiscription = command.IssueDiscription;

					//added by softdue
					entity.IsActionNeeded = command.IsActionNeeded;

					if (entity.IsChecked && entity.ChapterTrackedDate == null)
					{
						entity.ChapterTrackedDate = DateTime.UtcNow;
					}
					else if (!entity.IsChecked)
					{
						entity.ChapterTrackedDate = null;
					}
				}
				_repository.SaveChanges();
			}
			return null;
		}


		public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<PolicyContentBox> argument)
		{
			if (_policycontentboxrepository.Find(argument.Id) == null)
				yield return new ValidationResult("Id", $"Cannot find Acrobat field Content Box with id : {argument.Id}");
		}


		public CommandResponse Execute(DeleteByIdCommand<PolicyContentBox> command)
		{
			var reponse = new CommandResponse()
			{
				Id = command.Id.ToString(),
				ErrorMessage = null,
				Validation = null
			};

			try
			{
				var document = _policycontentboxrepository.Find(command.Id);

				if (document != null)
				{
					document.Deleted = true;
					_policycontentboxrepository.SaveChanges();
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
