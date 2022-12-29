using System.Collections.Generic;

namespace Ramp.Contracts.Query.User
{
    public class StandardUsersQuery
    {
        public IEnumerable<string> Ids { get; set; } = new List<string>();
        public IEnumerable<string> GroupIds { get; set; } = new List<string>();
		public IEnumerable<string> TagNames { get; set; } = new List<string>();
	}
}