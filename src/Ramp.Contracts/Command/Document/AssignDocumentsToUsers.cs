using System;
using System.Collections.Generic;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Command.Document {
	public class AssignDocumentsToUsers
    {
        public string AssignedBy { get; set; }
		public string MultipleAssignedDates { get; set; }
        public IEnumerable<AssignmentViewModel> AssignmentViewModels { get; set; }
		public DateTime? AssignedDate { get; set; }
		public bool IsReassigned { get; set; } = false;
		public Guid CompanyId { get; set; }
		public DateTime? ExpiryDate { get; set; }

	}
}
