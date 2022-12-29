using Ramp.Contracts.ViewModel;
using System;


namespace Ramp.Contracts.QueryParameter.TrainingActivity {
	public class FilterTrainingActivityLogQuery : IContextQuery {
		public DateTime FromDate { get; set; }
		public DateTime ToDate { get; set; }
		public string Trainers { get; set; }
		public string Trainees { get; set; }
		public string TrainingLables { get; set; }
		public string ExternalTrainingProviders { get; set; }
		public int TrainingType { get; set; }
		public decimal CostRangeFrom { get; set; }
		public decimal CostRangeTo { get; set; }
		public string 	Recepients { get; set; }
		public PortalContextViewModel PortalContext { get; set; }
		public bool AddOnrampBranding { get; set; }
	}
}
