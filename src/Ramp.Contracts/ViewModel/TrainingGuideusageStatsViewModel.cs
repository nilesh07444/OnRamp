using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class TrainingGuideusageStatsViewModel : IViewModel
    {
        public TrainingGuideusageStatsViewModel()
        {
            TrainingGuidUsageList = new List<TrainingGuideusageStatsViewModelShort>();
        }

        public IList<TrainingGuideusageStatsViewModelShort> TrainingGuidUsageList { get; set; }

        public Guid Id { get; set; }
        public Guid TrainingGuidId { get; set; }
        public Guid UserId { get; set; }
        public DateTime ViewDate { get; set; }
        public int TotalAssignedCount { get; set; }
    }

    public class TrainingGuideusageStatsViewModelShort
    {
        //Code For Training Guide Usages Reoprt
        public string TraigingGuideName { get; set; }
        public int TotalView { get; set; }
        public decimal ViewPerWeek { get; set; }

        //Code For Training Guide Usages Threshold Reoprt
        //public string UserName { get; set; }
        public string CompanyName { get; set; }
        public int AllowGuide { get; set; }
        public int CreatedGuide { get; set; }
    }
}
