using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Ramp.Contracts.Query.Group;

namespace Ramp.Services.QueryHandler.Group
{
    public class GroupQueryHandler :
        QueryHandlerBase<GroupQueryParameter, GroupViewModelLong>,
        IQueryHandler<GroupsWithUsersQuery, IEnumerable<GroupViewModelShort>>,
		 IQueryHandler<FetchByIdQuery, CustomerGroup> {
        private readonly IRepository<CustomerGroup> _groupRepository;
        private readonly IRepository<Domain.Models.Company> _companyRepository;
        private readonly IRepository<StandardUser> _userRepository;

        public GroupQueryHandler(IRepository<CustomerGroup> groupRepository, IRepository<Company> companyRepository, IRepository<StandardUser> userRepository)
        {
            _groupRepository = groupRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
        }

        public override GroupViewModelLong ExecuteQuery(GroupQueryParameter queryParameters)
        {
            var groupViewModelLong = new GroupViewModelLong();

            if (queryParameters.GroupId != null && queryParameters.GroupId != Guid.Empty)
            {
                var groupModel = _groupRepository.Find(queryParameters.GroupId);
                if (groupModel != null)
                {
                    groupViewModelLong.GroupViewModel = new GroupViewModel
                    {
                        GroupId = groupModel.Id,
                        Description = groupModel.Description,
                        Title = groupModel.Title,
                        IsforSelfSignUpGroup = groupModel.IsforSelfSignUpGroup
                    };
                    var customerCompany = _companyRepository.Find(groupModel.CompanyId);

                    if (customerCompany != null)
                    {
                        groupViewModelLong.GroupViewModel.SelectedCustomerCompanyId = customerCompany.Id;

						if(customerCompany.ProvisionalAccountLink!=null && customerCompany.ProvisionalAccountLink != Guid.Empty) {
							var provisionalCompany = _companyRepository.Find(customerCompany.ProvisionalAccountLink);

							groupViewModelLong.GroupViewModel.SelectedProvisionalCompanyId = provisionalCompany.Id;
						}
                    }
                }
            }
            var groupList = _groupRepository.List.ToList();

            foreach (var group in groupList)
            {
                var groupViewModel = new GroupViewModel
                {
                    GroupId = group.Id,
                    Title = group.Title,
                    Description = group.Description
                };

                var customerCompany = _companyRepository.Find(queryParameters.CompanyId);

                var companyViewModel = new CompanyViewModel
                {
                    CompanyName = customerCompany.CompanyName,
                    Id = customerCompany.Id
                };

                groupViewModel.Company = companyViewModel;

                groupViewModelLong.GroupList.Add(groupViewModel);
            }
            return groupViewModelLong;
        }

       
		public IEnumerable<GroupViewModelShort> ExecuteQuery(GroupsWithUsersQuery query) {
			var groups = _userRepository.List.Where(u => !u.IsUserExpire && u.IsActive && u.Group != null).Select(u => u.Group).Distinct().Select(x => new GroupViewModelShort {
				Id = x.Id,
				Name = x.Title
			}).OrderBy(x => x.Name).ToList();
			return groups;
		}

		public CustomerGroup ExecuteQuery(FetchByIdQuery query) {
			CustomerGroup customerGroup = new CustomerGroup();
			customerGroup = _groupRepository.Find(query.Id);
			return customerGroup;
		}
	}
}