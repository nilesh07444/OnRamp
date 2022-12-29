using System;
using Common.Data;

namespace Domain.Customer.Models.Document
{
    public class AssignedDocument : IdentityModel<string>
    {
        public string UserId { get; set; }
        public string DocumentId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string AssignedBy { get; set; }
        public string AdditionalMsg { get; set; }
        public DateTime AssignedDate { get; set; }
        public bool Deleted { get; set; }
		public bool IsRecurring { get; set; }
		public int OrderNumber { get; set; }
	}
}
