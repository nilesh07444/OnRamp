using Domain.Models;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class UserKpiReportViewModel
    {
        public IList<CompanyViewModel> Companies = new List<CompanyViewModel>();
        public IList<UserViewModel> Users = new List<UserViewModel>();

        public IList<TrainingGuideStatistic> TrainingGuideStatistics = new List<TrainingGuideStatistic>();
        public IList<ActivityItem> Activity = new List<ActivityItem>();
        public IList<TestResultItem> TestResults = new List<TestResultItem>();

        public int LoginCount { get; set; }
        public double LoginFrequency { get; set; }

        public class TestResultItem
        {
            public DateTime TestDate { get; set; }
            public TimeSpan TimeTaken { get; set; }
            public string Guide { get; set; }
            public double Score { get; set; }
            public double MaxScore { get; set; }
            public bool Result { get; set; }
        }

        public class ActivityItem
        {
            public DateTime EventDate { get; set; }
            public string Type { get; set; }
        }

        public class TrainingGuideStatistic
        {
            public Guid GuideId { get; set; }
            public string Guide { get; set; }

            public bool LookedAt { get; set; }
            public bool Passed { get; set; }
        }
    }
}