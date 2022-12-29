using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.Group;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.CommandHandler.Group {
	public class DeleteGroupCommandHandler : CommandHandlerBase<DeleteGroupCommand> {
		private readonly IRepository<CustomerGroup> _groupRepository;

		public DeleteGroupCommandHandler(IRepository<CustomerGroup> groupRepository) {
			_groupRepository = groupRepository;
		}

		public override CommandResponse Execute(DeleteGroupCommand command) {
			var allgroups = _groupRepository.GetAll();
			var xc =new List<CustomerGroup>();
			foreach(var groups in allgroups) {
				xc.Add(groups);
			}
			var t = xc.OrderBy(c => c.DateCreated).ToList();

			var filteredGroups = new List<CustomerGroup>();
			if (allgroups != null) {
				foreach (var groups in t) {
					if ((groups.ParentId != null && Guid.Parse(groups.ParentId) == command.GroupId) || groups.Id == command.GroupId) {
						if (filteredGroups.Contains(groups) == false) {
							filteredGroups.Add(groups);
							command.GroupId = groups.Id;
						}
					}
				}
			}

			foreach (var groups in filteredGroups) {
				_groupRepository.Delete(groups);
				_groupRepository.SaveChanges();
			}

			return null;
		}
		
	}
}