using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TrainingActivity
{
    public class CreateOrUpdateTrainingActivityCommand : IdentityModel<string>
    {
        [Required(AllowEmptyStrings = false,ErrorMessage ="Please supply a Title")]
        public string Title { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please supply a Description")]
        public string Description { get; set; }
        [Required(ErrorMessage ="Type Is Required")]
        public TrainingActivityType? TrainingActivityType { get; set; }
        [Required(ErrorMessage = "From Is Required")]
        public DateTime? From { get; set; }
        [Required(ErrorMessage = "To Is Required")]
        public DateTime? To { get; set; }
        public decimal? CostImplication { get; set; }
        public DateTime? LastEditDate { get; set; }
        public DateTime? Time { get; set; }
        public IList<UserModelShort> UsersTrained { get; set; } = new List<UserModelShort>();
        public int? RewardPoints { get; set; }
        public string Venue { get; set; }
        public string AdditionalInfo { get; set; }
        public string TrainingLabels { get; set; } = string.Empty;
        public IList<UploadResultViewModel> Documents { get; set; } = new List<UploadResultViewModel>();
        public UserModelShort EditedBy { get; set; }
        public DateTime? Created { get; set; }
        public UserModelShort CreatedBy { get; set; }
		public BursaryTrainingActivityDetailModel BursaryTrainingActivityDetail { get; set; } = new BursaryTrainingActivityDetailModel();
		public ToolboxTalkTrainingActivityDetailModel ToolboxTalkTrainingActivityDetail { get; set; } = new ToolboxTalkTrainingActivityDetailModel();
		public InternalTrainingActivityDetailModel InternalTrainingActivityDetail { get; set; } = new InternalTrainingActivityDetailModel();
		public MentoringAndCoachingTrainingActivityDetailModel MentoringAndCoachingTrainingActivityDetail { get; set; } = new MentoringAndCoachingTrainingActivityDetailModel();
		public ExternalTrainingActivityDetailModel ExternalTrainingActivityDetail { get; set; } = new ExternalTrainingActivityDetailModel();
    }
}
