using Common.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ramp.Contracts.ViewModel
{
    public class CategoryStatisticsReportViewModel
    {
        public IList<CompanyViewModel> Companies { get; set; }
        public Guid? SelectedCategoryId { get; set; }
        public IList<CategoryViewModel> Categories { get; set; }
        public Guid SelectedCompanyId { get; set; }
        public TrainingGuideSummaryData Summary { get; set; }
        public TrainingGuideDetail Detail { get; set; }

        public class TrainingGuideSummaryData
        {
            public IList<TrainingGuideSummary> Items { get; set; }

            public double Allocated
            {
                get { return Items.Sum(c => c.Allocated); }
            }

            public double Interacted
            {
                get { return Items.Sum(c => c.Interacted); }
            }

            public double NotInteracted
            {
                get { return Items.Sum(c => c.NotInteracted); }
            }

            public double Assigned
            {
                get { return Items.Sum(c => c.Assigned); }
            }

            public double Passed
            {
                get { return Items.Sum(c => c.Passed); }
            }

            public double TestTaken
            {
                get { return Items.Sum(c => c.TestTaken); }
            }

            public double PassPercentage
            {
                get { return Math.Round(Items.Average(c => c.PassPercentage), 2); }
            }

            public TrainingGuideSummaryData()
            {
                Items = new List<TrainingGuideSummary>();
            }
        }

        public class TrainingGuideSummary
        {
            public Guid GuideId { get; set; }

            public string Guide { get; set; }

            public double Allocated { get; set; }

            public double Interacted { get; set; }

            public double TestTaken { get; set; }

            public double NotInteracted
            {
                get { return Allocated - Interacted; }
            }

            public double Assigned { get; set; }
            public double Passed { get; set; }

            public double PassPercentage
            {
                get
                {
                    var percentage = Math.Round(((Passed / TestTaken) * 100), 2);
                    if (double.IsNaN(percentage))
                        percentage = 0;

                    return percentage;
                }
            }
        }

        public class UserDetail : IdentityModel<Guid>
        {
            public string Name { get; set; }
        }

        public class TrainingGuideDetail
        {
            public Guid GuideId { get; set; }

            public string Guide { get; set; }

            public IEnumerable<UserDetail> Allocated { get; set; }

            public IEnumerable<UserDetail> Interacted { get; set; }

            public IEnumerable<UserDetail> NotInteracted
            {
                get { return Assigned.Except(Interacted); }
            }

            public IEnumerable<UserDetail> Assigned { get; set; }

            public IEnumerable<UserDetail> Passed { get; set; }

            public TrainingGuideDetail()
            {
                Interacted = new List<UserDetail>();
                Assigned = new List<UserDetail>();
                Passed = new List<UserDetail>();
            }
        }

        public CategoryStatisticsReportViewModel()
        {
            Summary = new TrainingGuideSummaryData();
        }
    }
}