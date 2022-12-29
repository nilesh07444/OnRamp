using Common.Query;

namespace Ramp.Contracts.QueryParameter.Certificates
{
	public class CertificateListQuery :IPagedQuery {
		public int Page { get; set; }
		public int? PageSize { get; set; }
		public bool? EnableChecklistDocument { get; set; }
		public string MatchText { get; set; }
		public string SortingOrder { get; set; }
	}
}
