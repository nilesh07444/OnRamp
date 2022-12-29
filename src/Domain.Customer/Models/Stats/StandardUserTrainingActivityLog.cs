using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class StandardUserTrainingActivityLog : Base.CustomerDomainObject
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public DateTime Time { get; set; }
        public virtual IList<StandardUser> UsersTrained { get; set; } = new List<StandardUser>();
        public TrainingActivityType? TrainingActivityType { get; set; }
        public int? RewardPoints { get; set; }
        public string Venue { get; set; }
        public string AdditionalInfo { get; set; }
        public string TrainingLabels { get; set; }
        public virtual IList<Upload> Documents { get; set; } = new List<Upload>();
        public decimal? CostImplication { get; set; }
        public DateTime? LastEditDate { get; set; }
        public virtual StandardUser EditedBy { get; set; }
        public DateTime Created { get; set; }
        public virtual StandardUser CreatedBy { get; set; }
        public virtual BursaryTrainingActivityDetail BursaryTrainingActivityDetail { get; set; }
        public virtual ToolboxTalkTrainingActivityDetail ToolboxTalkTrainingActivityDetail { get; set; }
        public virtual InternalTrainingActivityDetail InternalTrainingActivityDetail { get; set; }
        public virtual MentoringAndCoachingTrainingActivityDetail MentoringAndCoachingTrainingActivityDetail { get; set; }
        public virtual ExternalTrainingActivityDetail ExternalTrainingActivityDetail { get; set; }
    }
}
