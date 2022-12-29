using Common.Command;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.CommandParameter.Group
{
    public class SaveOrUpdateStandardUserGroupCommand : ICommand
    {
		public Guid UserId { get; set; }
		public List<string> GroupId { get; set; }
		public DateTime? DateCreated { get; set; }
		//public string Title { get; set; }
		//public string Description { get; set; }
		//public bool IsforSelfSignUpGroup { get; set; }
		//public Guid CompanyId { get; set; }
		//public bool AttemptCreate { get; set; }
		//public string ParentId { get; set; }
	}
}