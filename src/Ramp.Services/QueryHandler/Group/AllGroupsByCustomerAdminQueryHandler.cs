using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ramp.Services.QueryHandler.Group {
	public class AllGroupsByCustomerAdminQueryHandler :
		QueryHandlerBase<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>, IQueryHandler<CustomerGroup, IEnumerable<JSTreeViewModel>> {

		private readonly ITransientReadRepository<CustomerGroup> _repository;

		private readonly IRepository<CustomerGroup> _groupRepository;

		public AllGroupsByCustomerAdminQueryHandler(IRepository<CustomerGroup> groupRepository, ITransientReadRepository<CustomerGroup> repository) {
			_groupRepository = groupRepository;
			_repository = repository;
		}

		public override List<GroupViewModel> ExecuteQuery(AllGroupsByCustomerAdminQueryParameter queryParameters) {
			var groupList = _groupRepository.GetAll();
			var groups = new List<GroupViewModel>();
			foreach (var group in groupList) {
				var groupViewModel = new GroupViewModel {
					GroupId = group.Id,
					Title = group.Title,
					IsforSelfSignUpGroup=group.IsforSelfSignUpGroup,
					Description = group.Description
				};
				var company = new CompanyViewModel {
					Id = group.CompanyId
				};
				groupViewModel.Company = company;
				groups.Add(groupViewModel);
			}
			return groups;
		}

		public IEnumerable<JSTreeViewModel> ExecuteQuery(CustomerGroup query) {
			try {
				var groups = _repository.List.AsQueryable().OrderBy(x => x.Title).Select(Project.UserGroups_JSTreeViewModel).ToList();
				return groups;
			}
			catch (Exception e) {
				throw e;
			}
		}
	}
}

namespace Ramp.Services.Projection {
	public static partial class Project {
		public static readonly Expression<Func<CustomerGroup, JSTreeViewModel>> UserGroups_JSTreeViewModel =
			x => new JSTreeViewModel {
				id = x.Id.ToString(),
				parent = x.ParentId ?? "#",
				text = x.Title != null ? x.Title.Length > 100 ? x.Title.Substring(0, 100) : x.Title : null
			};
	}
}