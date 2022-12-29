using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler.User
{
    public class AllRolesQueryHandler : QueryHandlerBase<EmptyQueryParameter, List<RoleViewModel>>
    {
        private readonly IRepository<Role> _roleRepository;

        public AllRolesQueryHandler(IRepository<Role> roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public override List<RoleViewModel> ExecuteQuery(EmptyQueryParameter queryParameters)
        {
            var roles = _roleRepository.List;

            return roles.Select(role => new RoleViewModel
            {
                RoleId = role.Id,
                RoleName = role.RoleName,
                Description = role.Description
            }).ToList();
        }
    }
}