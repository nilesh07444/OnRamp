using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Query;
using Domain.Customer;

namespace Ramp.Contracts.Query.UserFeedback
{
    public class UserFeedbackListQuery: IPagedQuery {
        public string DocumentId { get; set; }
        public DocumentType DocumentType { get; set; }
		public int Page { get ; set ; }
		public int? PageSize { get ; set ; }
		public bool? EnableChecklistDocument { get; set; }
	}
}
