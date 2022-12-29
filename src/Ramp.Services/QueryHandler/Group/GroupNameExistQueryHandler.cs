using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System.Linq;

namespace Ramp.Services.QueryHandler.Group
{
    public class GroupNameExistQueryHandler :
        QueryHandlerBase<GroupNameExistQueryParameter, RemoteValidationResponseViewModel>
    {
        private readonly IRepository<CustomerGroup> _groupRepository;

        public GroupNameExistQueryHandler(IRepository<CustomerGroup> groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public override RemoteValidationResponseViewModel ExecuteQuery(GroupNameExistQueryParameter queryParameters)
        {
            RemoteValidationResponseViewModel result = new RemoteValidationResponseViewModel();

            result.Response = _groupRepository.List.Any(u => (u.Title.TrimAllCastToLowerInvariant().Equals(queryParameters.GroupName.TrimAllCastToLowerInvariant())));

            return result;
            //return new RemoteValidationResponseViewModel()
            //{
            //    //here created by condition is added to check for qroup name and created by
            //    Response = _groupRepository.List.Any(u => u.Title.Equals(queryParameters.GroupName)&& u.CompanyId == queryParameters.CompanyId)
            //};
        }
    }
}