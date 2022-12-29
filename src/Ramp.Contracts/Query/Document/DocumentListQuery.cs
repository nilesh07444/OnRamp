using Common.Data;
using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Document
{
    public class DocumentListQuery : DocumentListQueryBase,IPagedQuery
    {
        public int Page { get; set; }
        public int? PageSize { get; set; }
        public IEnumerable<string> DocumentFilters { get; set; } = new List<string>();
        public bool TemplatePortal { get; set; }
		public bool? EnableChecklistDocument { get; set; }
		
	}
    public class DocumentListQueryBase
    {
        public string CategoryId { get; set; }
        public IEnumerable<string> DocumentStatus { get; set; } = new List<string>();
        public string MatchText { get; set; } 
        public string SortingOrder { get; set; }
    }
}
