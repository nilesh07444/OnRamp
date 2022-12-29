using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class MigrateTestResultsExpansionCommandHandler : ICommandHandlerBase<MigrateTestResultsExpansionCommandParameter>
    {
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingTest> _testRepository;

        public MigrateTestResultsExpansionCommandHandler(
            IRepository<TestResult> testResultRepository,
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<TrainingTest> testRepository)
        {
            _testResultRepository = testResultRepository;
            _trainingGuideRepository = trainingGuideRepository;
            _testRepository = testRepository;
        }

        public CommandResponse Execute(MigrateTestResultsExpansionCommandParameter command)
        {
            foreach (var r in _testResultRepository.List)
            {
                if (r.TrainingGuideId.HasValue)
                {
                    var guide = _trainingGuideRepository.List.Where(x => x.Id.Equals(r.TrainingGuideId.Value)).Select(x => new { Categories = x.Categories, Title = x.Title }).ToList().SingleOrDefault();
                    if (guide != null)
                    {
                        var category = guide.Categories.FirstOrDefault();
                        if (category != null)
                        {
                            r.TrainingGuideCategory = category.CategoryTitle;
                            r.TrainingGuideCategoryId = category.Id;
                        }
                        r.TrainingGuideTitle = guide.Title;
                    }
                }
                if (r.TrainingTestId.HasValue)
                {
                    var test = _testRepository.List.Where(x => x.Id.Equals(r.TrainingTestId.Value)).Select(x => new { TestTitle = x.TestTitle, TrophyName = x.TrophyName, TestTrophy = x.TestTrophy, Version = x.Version }).ToList().SingleOrDefault();
                    if (test != null)
                    {
                        r.TestTitle = test.TestTitle;
                        r.TrophyName = test.TrophyName;
                        r.TrophyData = test.TestTrophy;
                        r.Version = test.Version ?? 0;
                    }
                }
            }
            _testResultRepository.SaveChanges();
            return null;
        }
    }
}