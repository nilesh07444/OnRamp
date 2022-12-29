using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.Command.User;
using Ramp.Contracts.CommandParameter.Group;
using System;
using System.Linq;

namespace Ramp.Services.CommandHandler.Group {
	public class SaveOrUpdateGroupCommandHandler : CommandHandlerBase<SaveOrUpdateGroupCommand>,
		ICommandHandlerBase<GroupUpdateParentCommand>,
		ICommandHandlerBase<UserUpdateGroupCommand> {
		private readonly IRepository<CustomerGroup> _groupRepository;
		private readonly IRepository<StandardUser> _userRepository;

		public SaveOrUpdateGroupCommandHandler(IRepository<CustomerGroup> groupRepository, IRepository<StandardUser> userRepository) {
			_groupRepository = groupRepository;
			_userRepository = userRepository;
		}

		public override CommandResponse Execute(SaveOrUpdateGroupCommand command) {
			if (command.IsforSelfSignUpGroup && _groupRepository.List.AsQueryable().Any(x => x.IsforSelfSignUpGroup)) {
				_groupRepository.List.AsQueryable().Where(x => x.IsforSelfSignUpGroup).ToList().ForEach(delegate (CustomerGroup group) {
					group.IsforSelfSignUpGroup = false;

				});
				_groupRepository.SaveChanges();
			}
			var groupModel = _groupRepository.List.AsQueryable().FirstOrDefault(g => g.Id == command.GroupId || g.Title == command.Title);
			if (groupModel == null) {
				groupModel = new CustomerGroup {
					Id = command.GroupId, //Guid.NewGuid(),
					CompanyId = command.CompanyId
				};
				_groupRepository.Add(groupModel);
			}
			groupModel.Title = command.Title;
			groupModel.Description = command.Description;
			groupModel.IsforSelfSignUpGroup = command.IsforSelfSignUpGroup;
			groupModel.ParentId = command.ParentId;
			groupModel.DateCreated = DateTime.Now;
			_groupRepository.SaveChanges();

			return null;
		}

		public  CommandResponse Execute(UserUpdateGroupCommand command) {
			var group = _groupRepository.Find(Guid.Parse(command.GroupId));
			var user = _userRepository.Find(Guid.Parse(command.Id));
			user.Group = group;
			_userRepository.SaveChanges();

			return null;
		}

		public CommandResponse Execute(GroupUpdateParentCommand command) {
			try {
				var category = _groupRepository.Find(Guid.Parse(command.Id));
				category.ParentId = command.ParentId;
				_groupRepository.SaveChanges();
			}
			catch (Exception e) {
				throw e;
			}
			return null;
		}

	}
}