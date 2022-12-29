using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models.CheckLists;
using Ramp.Contracts.Command.CheckList;
using System;
using System.Linq;

namespace Ramp.Services.CommandHandler {
	public class CheckListChapterCommandHandler : CommandHandlerBase<CreateOrUpdateCheckListChapterCommand> {

		readonly ITransientRepository<CheckListChapter> _repository;

		public CheckListChapterCommandHandler(ITransientRepository<CheckListChapter> repository) {
			_repository = repository;
		}

		public override CommandResponse Execute(CreateOrUpdateCheckListChapterCommand command) 
		{
			var entity = _repository.List.AsQueryable().FirstOrDefault(x => x.Id == command.Id);
			if (entity == null) 
			{
				entity = new CheckListChapter 
				{
					Id = Guid.NewGuid().ToString(),
					CheckListId = command.CheckListId,
					Title = command.Title,
					Number = command.Number,
					Content = command.Content,
					AttachmentRequired = command.AttachmentRequired,
					IsChecked = command.IsChecked,
					Deleted = false
				};
				_repository.Add(entity);
			} 
			else 
			{				
				entity.IsChecked = command.IsChecked;
			}
			_repository.SaveChanges();
			return null;
		}
	}
}
