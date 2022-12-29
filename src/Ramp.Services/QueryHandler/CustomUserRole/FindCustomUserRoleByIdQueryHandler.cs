using Common.Data;
using Common.Query;
using Domain.Customer.Models.CustomRole;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.CustomUserRole {
	

	public class FindCustomUserRoleByIdQueryHandler : QueryHandlerBase<FetchByIdQuery, CustomUserRoles> {

		private readonly IRepository<CustomUserRoles> _customUserRole;

		public FindCustomUserRoleByIdQueryHandler(IRepository<CustomUserRoles> customUserRole)
		{
			_customUserRole = customUserRole;
		}

		public override CustomUserRoles ExecuteQuery(FetchByIdQuery queryParameters)
		{

			return _customUserRole.Find(queryParameters.Id);
			
		}
	}
}
