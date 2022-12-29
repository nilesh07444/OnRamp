using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.UserFeedbackRead
{
	public class UserFeedbackReadListQuery : IPagedQuery {
		public int Page { get ; set ; }
		public int? PageSize { get ; set ; }
		public bool? EnableChecklistDocument { get; set; }
	}
}
