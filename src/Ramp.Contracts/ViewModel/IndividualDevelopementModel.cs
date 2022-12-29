using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class IndividualDevelopmentModel
    {
        public UserViewModel User { get; set; }
        public  IEnumerable<TrainingActivityModel> TrainingActivities { get; set; } = new List<TrainingActivityModel>();
        public OnRampTrainingModel OnRampTraining { get; set; }
        public string CompanyLogoId { get; set; }
    }
    public class OnRampTrainingModel
    {
        public IEnumerable<PlaybookSummaryModel> Playbooks { get; set; } = new List<PlaybookSummaryModel>();
    }
    public class PlaybookSummaryModel
    {
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public bool? Viewed { get; set; }
        public bool Assigned { get; set; }
        public DateTime? DateAssigned { get; set; }
        public TestSummaryModel Test { get; set; }
    }
    public class TestSummaryModel
    {
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public string PlaybookId { get; set; }
        public bool Assigned { get; set; }
        public DateTime? DateAssigned { get; set; }
        public int? Version { get; set; }
        public bool TakenTest { get; set; }
        public bool? Passed { get; set; }
        public decimal? Percentage { get; set; }
        public int MaximumScore { get; set; }
        public int PointsScored { get; set; }
    }
    public class IndividualDevelopmentReportModel : ContextReportModel<IndividualDevelopmentModel>
    {
    }
}


