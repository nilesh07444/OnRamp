using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Command.AcrobatField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Ramp.Services.CommandHandler
{
	public class AcrobatFieldChapterUserResultCommandHandler : CommandHandlerBase<CreateOrUpdateAcrobatFieldChapterUserResultCommand>,
									ICommandHandlerAndValidator<DeleteByIdCommand<AcrobatFieldChapterUserResult>>,
									ICommandHandlerAndValidator<DeleteByIdCommand<AcrobatFieldContentBox>>
	{

		readonly ITransientRepository<AcrobatFieldChapterUserResult> _repository;
		readonly ITransientRepository<AcrobatFieldContentBox> _AcrobatContextBoxrepository;

		public AcrobatFieldChapterUserResultCommandHandler(ITransientRepository<AcrobatFieldChapterUserResult> repository, ITransientRepository<AcrobatFieldContentBox> AcrobatContextBoxrepository)
		{
			_repository = repository;
			_AcrobatContextBoxrepository = AcrobatContextBoxrepository;
		}

		public override CommandResponse Execute(CreateOrUpdateAcrobatFieldChapterUserResultCommand command)
		{
			var userId = string.IsNullOrEmpty(command.UserId) ? (Thread.CurrentPrincipal as ClaimsPrincipal)?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value : command.UserId;
			if (command.IsGlobalAccessed)
			{
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.DocumentId == command.DocumentId && x.AcrobatFieldChapterId == command.AcrobatFieldChapterId);
				if (entity == null)
				{
					entity = new AcrobatFieldChapterUserResult
					{
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						AcrobatFieldChapterId = command.AcrobatFieldChapterId,
						IsChecked = command.IsChecked,
						CreatedDate = DateTime.UtcNow,
						IssueDiscription = command.IssueDiscription,
						DocumentId = command.DocumentId,
						DateCompleted = DateTime.UtcNow,
						IsGlobalAccessed = command.IsGlobalAccessed,
						UserId = userId

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
				var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.AssignedDocumentId == command.AssignedDocumentId && x.AcrobatFieldChapterId == command.AcrobatFieldChapterId);
				if (entity == null)
				{
					entity = new AcrobatFieldChapterUserResult
					{
						Id = Guid.NewGuid().ToString(),
						AssignedDocumentId = command.AssignedDocumentId,
						AcrobatFieldChapterId = command.AcrobatFieldChapterId,
						IsChecked = command.IsChecked,
						CreatedDate = DateTime.UtcNow,
						DateCompleted = DateTime.UtcNow,
						UserId = userId,
						IssueDiscription = command.IssueDiscription,
						DocumentId = command.DocumentId,
						IsGlobalAccessed = command.IsGlobalAccessed

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

		public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<AcrobatFieldChapterUserResult> argument)
		{
			if (_AcrobatContextBoxrepository.Find(argument.Id) == null)
				yield return new ValidationResult("Id", $"Cannot find Acrobat field Content Box with id : {argument.Id}");
		}

		public CommandResponse Execute(DeleteByIdCommand<AcrobatFieldChapterUserResult> command)
		{
			var reponse = new CommandResponse()
			{
				Id = command.Id.ToString(),
				ErrorMessage = null,
				Validation = null
			};

			try
			{
				var document = _AcrobatContextBoxrepository.Find(command.Id);

				if (document != null)
				{
					//	_contentRepository.Delete(document);
					document.Deleted = true;
					_AcrobatContextBoxrepository.SaveChanges();
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
		public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<AcrobatFieldContentBox> argument)
		{
			if (_AcrobatContextBoxrepository.Find(argument.Id) == null)
				yield return new ValidationResult("Id", $"Cannot find Acrobat field Content Box with id : {argument.Id}");
		}

		public CommandResponse Execute(DeleteByIdCommand<AcrobatFieldContentBox> command)
		{
			var reponse = new CommandResponse()
			{
				Id = command.Id.ToString(),
				ErrorMessage = null,
				Validation = null
			};

			try
			{
				var document = _AcrobatContextBoxrepository.Find(command.Id);

				if (document != null)
				{
					//	_contentRepository.Delete(document);
					document.Deleted = true;
					_AcrobatContextBoxrepository.SaveChanges();
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
