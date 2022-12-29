using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enums.GenderEnum;

namespace Ramp.Contracts.QueryParameter.TrainingActivity
{
    public class TrainingActivityListQuery : IContextQuery, IPagedQuery {
        public  Guid? Id { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public decimal? CostFloor { get; set; }
        public  decimal? CostCeiling { get; set; }
        public IEnumerable<TrainingActivityType?> TrainingActivityTypes { get; set; } = new List<TrainingActivityType?>();
        public IEnumerable<RaceCodeViewModel> RaceCodes { get; set; } = new List<RaceCodeViewModel>();
        public IEnumerable<Gender?> Genders { get; set; } = new List<Gender?>();
        public IEnumerable<UserModelShort> UsersTrained { get; set; } = new List<UserModelShort>();
        public IEnumerable<GroupViewModelShort> Groups { get; set; } = new List<GroupViewModelShort>();
        public string TrainingLabels { get; set; }
        public PortalContextViewModel PortalContext { get; set; }
        public bool AddOnrampBranding { get; set; }
        public bool IsChecklistEnable { get; set; }
		public int Page { get; set; }
		public int? PageSize { get; set; }
		public bool? EnableChecklistDocument { get; set; }
	}
}
