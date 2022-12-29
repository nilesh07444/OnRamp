using System;

namespace Domain.Customer.Models.Restore {
	public	class RestoreTrack : Base.CustomerDomainObject {
		public DateTime? UpdatedDate { get; set; }
		public string LastEditedBy { get; set; }
		public string DocumentName { get; set; }
		public string DocumentId { get; set; }
		public string UserName { get; set; }
		public string DocumentType { get; set; }
		public string DocumentStatus { get; set; }
	}
}
