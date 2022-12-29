using Common.Query;
using System;

namespace Ramp.Contracts.QueryParameter.User {
	public class UserQueryParameter  {
		public Guid CompanyId { get; set; }
		public string CompanyName { get; set; }
		public Guid UserId { get; set; }
		public Guid LoggedInUserId { get; set; }
	}
}
