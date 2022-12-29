using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Customer.Models.Groups;
using Ramp.Contracts.CommandParameter.Group;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.CommandHandler.Group {
	public class DeleteStandardUserGroupCommandHandler : CommandHandlerBase<DeleteStandardUserGroupCommand> {
		private readonly IRepository<StandardUserGroup> _groupRepository;

		public DeleteStandardUserGroupCommandHandler(IRepository<StandardUserGroup> groupRepository) {
			_groupRepository = groupRepository;
		}

		public override CommandResponse Execute(DeleteStandardUserGroupCommand command) {

			var entity = _groupRepository.List.Where(x => x.UserId == command.UserId).AsQueryable().ToList();

			entity.ForEach(x => {
				_groupRepository.Delete(x);
			});

			_groupRepository.SaveChanges();

			return null;
		}
		
	}
}