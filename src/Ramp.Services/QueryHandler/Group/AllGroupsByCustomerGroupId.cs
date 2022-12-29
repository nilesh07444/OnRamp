using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Common.Query;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler.Group
{
    public class AllGroupByCustomerGroupId : QueryHandlerBase<AllGroupsByCustomerGroupIdParameter, List<GroupViewModel>>
    {
        private readonly IRepository<Domain.Models.Group> _groupRepository;

        public AllGroupByCustomerGroupId(IRepository<Domain.Models.Group> groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public override List<GroupViewModel> ExecuteQuery(AllGroupsByCustomerGroupIdParameter queryParameters)
        {
            List<Domain.Models.Group> groups = _groupRepository.List.ToList();
            var groupList = new List<GroupViewModel>();
            foreach (var group in groups)
            {
                if(group.Id == queryParameters.GroupId)
                {
                    var groupModel = new GroupViewModel
                    {
                        GroupId = group.Id,
                        Title = group.Title,
                        Description = group.Description
                    };
                    if (group.Company != null)
                    {
                        var company = new CompanyViewModel
                        {
                            Id = group.Company.Id,
                            CompanyName = group.Company.CompanyName
                        };
                        groupModel.Company = company;
                    }
                    groupList.Add(groupModel);
                }
            }
            return groupList;
        }
    }
}