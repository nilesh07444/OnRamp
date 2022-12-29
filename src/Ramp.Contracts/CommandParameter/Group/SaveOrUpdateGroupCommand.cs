using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.Group
{
    public class SaveOrUpdateGroupCommand : ICommand
    {
        public Guid GroupId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsforSelfSignUpGroup { get; set; }
        public Guid CompanyId { get; set; }
        public bool AttemptCreate { get; set; }
		public string ParentId { get; set; }
	}
}