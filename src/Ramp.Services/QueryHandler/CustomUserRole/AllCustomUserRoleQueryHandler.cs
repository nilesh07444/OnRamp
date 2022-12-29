

using Common.Data;
using Common.Query;
using Domain.Customer.Models.CustomRole;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;
using System.Collections.Generic;
using System.Linq;


namespace Ramp.Services.QueryHandler {
	public class AllCustomUserRoleQueryHandler :
		QueryHandlerBase<FetchAllRecordsQuery, List<CustomUserRoles>> {

		private readonly IRepository<CustomUserRoles> _customUserRole;

		public AllCustomUserRoleQueryHandler(IRepository<CustomUserRoles> customUserRole)
		{
			_customUserRole = customUserRole;
		}

		public override List<CustomUserRoles> ExecuteQuery(FetchAllRecordsQuery queryParameters)
		{
			return _customUserRole.List.ToList();
		}
	}
}
