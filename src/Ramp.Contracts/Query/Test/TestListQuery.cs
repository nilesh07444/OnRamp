using Common.Query;
using Ramp.Contracts.Query.Document;

namespace Ramp.Contracts.Query.Test {
	public class TestListQuery : DocumentListQueryBase, IPagedQuery {
		public int Page { get ; set ; }
		public int? PageSize { get ; set ; }
		public bool? EnableChecklistDocument { get; set; }
		public bool IsRecycleBin { get; set; } = false;
	}
}
