using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Label
{
    public class LabelListQuery: IPagedQuery {
        public IEnumerable<string> Values = new List<string>();

		public int Page { get; set; }
		public int? PageSize { get; set; }
		public bool? EnableChecklistDocument { get; set; }
	}
}
