using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter
{
    public class SaveOrUpdateAutoAssignWorkflowCommand : ICommand
    {
        public string Id { get; set; }
        public string WorkflowName { get; set; }
        public List<AutoWorkFlowDocs> DocumentListID { get; set; }
        public string[] GroupId { get; set; }
        public DateTime? DateCreated { get; set; }
        public bool IsDeleted { get; set; }

		public bool SendNotiEnabled { get; set; }

		public string AssignedBy { get; set; }
		public string ComapnyId { get; set; }
	}
}
