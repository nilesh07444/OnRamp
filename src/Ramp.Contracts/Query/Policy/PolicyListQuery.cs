using Common.Query;
using Ramp.Contracts.Query.Document;

namespace Ramp.Contracts.Query.Policy {
	public class PolicyListQuery : DocumentListQueryBase, IPagedQuery {
		public int Page { get ; set ; }
		public bool IsRecycleBin { get; set; } = false;
		public int? PageSize { get ; set ; }
		public bool? EnableChecklistDocument { get; set; }
	}
}
