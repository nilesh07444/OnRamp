using System;
using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Customer.Models.Groups;
using Ramp.Contracts.Query.Bundle;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler {

	public class StandardUserGroupQueryHandler : IQueryHandler<StandardUserGroupByUserIdQuery, StandardUserGroupViewModel> {

		private readonly IRepository<StandardUserGroup> _standardUserGroupRepository;

		private readonly IRepository<CustomerGroup> _customerGroupRepository;

		public StandardUserGroupQueryHandler(
			IRepository<StandardUserGroup> standardUserGroupRepository, IRepository<CustomerGroup> customerGroupRepository) {
			_standardUserGroupRepository = standardUserGroupRepository;
			_customerGroupRepository = customerGroupRepository;
		}

		public StandardUserGroupViewModel ExecuteQuery(StandardUserGroupByUserIdQuery query) {
			
			var model = new StandardUserGroupViewModel();

			var groupList = _standardUserGroupRepository.List.Where(c => c.UserId == Guid.Parse(query.UserId)).ToList();

			var  cusomerGroupList = _customerGroupRepository.List.ToList();

			foreach (var x in groupList) {
				var groupViewModel = new StandardUserGroupModel();
				groupViewModel.GroupId = x.GroupId;

				foreach (var g in cusomerGroupList) {
					if(x.GroupId == g.Id)
					groupViewModel.Title = g.Title;
				}

				model.GroupList.Add(groupViewModel);
			};

			model.UserId = Guid.Parse(query.UserId);

			return model;
		}
	}
}