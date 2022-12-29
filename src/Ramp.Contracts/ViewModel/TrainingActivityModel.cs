using Common.Data;
using Domain.Customer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Report;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel
{
    public class TrainingActivityListModel  : IdentityModel<string>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TrainingActivityType? TrainingActivityType { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public decimal? CostImplication { get; set; }
        public decimal? CostRangeFrom { get; set; }
        public decimal? CostRangeTo { get; set; }
        public DateTime? LastEditDate { get; set; }
    }
    public class TrainingActivityModel : TrainingActivityListModel
    {
        public DateTime? Time { get; set; }
        public IList<UserModelShort> UsersTrained { get; set; } = new List<UserModelShort>();
        public int? RewardPoints { get; set; }
        public string Venue { get; set; }
        public string AdditionalInfo { get; set; }
        public string TrainingLabels { get; set; } = string.Empty;
        public string Trainers { get; set; } = string.Empty;
        public string Trainees { get; set; } = string.Empty;
        public string ExternalTrainingProviders { get; set; } = string.Empty;
        public IList<UploadResultViewModel> Documents { get; set; } = new List<UploadResultViewModel>();
        public UserModelShort EditedBy { get; set; }
        public DateTime? Created { get; set; }
        public UserModelShort CreatedBy { get; set; }
		public BursaryTrainingActivityDetailModel BursaryTrainingActivityDetail { get; set; } = new BursaryTrainingActivityDetailModel();
        public ToolboxTalkTrainingActivityDetailModel ToolboxTalkTrainingActivityDetail { get; set; }
        public InternalTrainingActivityDetailModel InternalTrainingActivityDetail { get; set; }
        public MentoringAndCoachingTrainingActivityDetailModel MentoringAndCoachingTrainingActivityDetail { get; set; }
		public ExternalTrainingActivityDetailModel ExternalTrainingActivityDetail { get; set; } = new ExternalTrainingActivityDetailModel();
        public decimal CostPerUser { get; set; }
		public SelectList TrainingActivityTypeList { get; set; }
		public SelectList TrainingLabelList { get; set; }
		public SelectList TrainersList { get; set; }
		public string LabelIds { get; set; }
		public string UploadIds { get; set; }
		public string ExternalInvoiceIds { get; set; }
		public string BursaryInvoiceIds { get; set; }
		public SelectList TraineesList { get; set; }
		public SelectList ExternalTrainingProviderList { get; set; }
	}
    public class BursaryTrainingActivityDetailModel
    {
        public string Id { get; set; }
        public IList<UploadResultViewModel> Invoices { get; set; } = new List<UploadResultViewModel>();
        public IList<UserModelShort> ConductedBy { get; set; } = new List<UserModelShort>();
        public string BursaryType { get; set; }
    }
    public class ToolboxTalkTrainingActivityDetailModel
    {
        public string Id { get; set; }
        public IList<UserModelShort> ConductedBy { get; set; } = new List<UserModelShort>();
    }

    public class InternalTrainingActivityDetailModel
    {
        public string Id { get; set; }
        public IList<UserModelShort> ConductedBy { get; set; } = new List<UserModelShort>();
    }
    public class MentoringAndCoachingTrainingActivityDetailModel
    {
        public string Id { get; set; }
        public IList<UserModelShort> ConductedBy { get; set; } = new List<UserModelShort>();
    }
    public class ExternalTrainingActivityDetailModel
    {
        public string Id { get; set; }
        public IList<UploadResultViewModel> Invoices { get; set; } = new List<UploadResultViewModel>();
        public IList<ExternalTrainingProviderListModel> ConductedBy { get; set; } = new List<ExternalTrainingProviderListModel>();

    }
    public class TrainingActivityReportModel : ContextReportModel<TrainingActivityModel>
    { 
    }
}
