using Domain.Customer;

namespace Ramp.Contracts.Query.Document {
	public	class DocAssignedToUserQuery {

		public string DocumentId { get; set; }
		public DocumentType DocumentType { get; set; }
		public string UserId { get; set; }
	}
}
