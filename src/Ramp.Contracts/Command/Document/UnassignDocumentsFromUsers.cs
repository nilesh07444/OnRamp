using System;
using System.Collections.Generic;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Command.Document {
	public class UnassignDocumentsFromUsers
    {
        public IEnumerable<AssignmentViewModel> AssignmentViewModels { get; set; }
		public Guid CompanyId { get; set; }
	}
}
