using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel {
	public class AssignedDocumentInfo {
		public string DocumentId { get; set; }
		public string Title { get; set; }
		public bool IsDocumentAssign { get; set; }

		public AssignedDocumentInfo() {
			DocumentId = "";
			Title = "";
			IsDocumentAssign = false;
		}
	}
}
