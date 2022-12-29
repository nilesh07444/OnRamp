using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel {
	public class TempData {
		public string Id { get; set; }
		public string UserId { get; set; }
		public string DocumentId { get; set; }
		public DocumentType DocumentType { get; set; }
		public string AssignedBy { get; set; }
		public string AdditionalMsg { get; set; }
		public DateTime AssignedDate { get; set; }
		public bool Deleted { get; set; }
		public bool IsRecurring { get; set; }
		public int OrderNumber { get; set; }
		public DateTime ViewDate { get; set; }
		public DocumentUsageStatus? Status { get; set; }

	}
}
