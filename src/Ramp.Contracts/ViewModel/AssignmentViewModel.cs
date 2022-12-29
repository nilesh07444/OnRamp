using System;
using Domain.Customer;

namespace Ramp.Contracts.ViewModel {
	public class AssignmentViewModel
    {
        public string DocumentId { get; set; }
        public string MultipleAssignedDates { get; set; }
        public string AdditionalMsg { get; set; }
        public DocumentType DocumentType { get; set; }
        public string UserId { get; set; }
		public DateTime? AssignedDate { get; set; }
		public int OrderNumber { get; set; } // For reordering

	}
}
