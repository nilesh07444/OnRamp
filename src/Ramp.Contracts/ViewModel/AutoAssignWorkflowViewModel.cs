using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel {
	
	public class AutoAssignWorkflowViewModel : IViewModel {
		public string Id { get; set; }
		public string WorkflowName { get; set; }
		public List<AutoWorkFlowDocs> DocumentList { get; set; }
		public string[] GroupIds { get; set; }
		public bool SendNotiEnabled { get; set; }
		public DateTime DateCreated { get; set; }
	}

}
	
