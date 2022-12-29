using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.TrophyCabinet;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.TrophyCabinet
{
    public class AllTrophiesByStandardUserQueryHandler : IQueryHandler<AllTrophiesByStandardUserQuery, List<TestResultViewModel>>
    {
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TestAssigned> _assignedTestRespository;

        public AllTrophiesByStandardUserQueryHandler(
            IRepository<TestResult> testResultRepository,
            IRepository<TestAssigned> assignedTestRespository)
        {
            _testResultRepository = testResultRepository;
            _assignedTestRespository = assignedTestRespository;
        }

        public List<TestResultViewModel> ExecuteQuery(AllTrophiesByStandardUserQuery query)
        {
            var testResultList = new List<TestResultViewModel>();
            var userAssignedTests = _assignedTestRespository.List.Where(t => t.UserId.HasValue && t.UserId.Value.Equals(query.UserId)).Select(t => t.Test.Id).ToList();
            var testresult =
                _testResultRepository.List.Where(c => c.TestResultStatus && c.TestTakenByUserId == query.UserId && c.TrainingTestId.HasValue && userAssignedTests.Contains(c.TrainingTestId.Value))
                    .OrderByDescending(c => c.TestDate)
                    .ToList();
            foreach (var test in testresult)
            {
                var result = Convert.ToInt32(test.TestScore);
                var totalMarks = test.Total;
                var userPercentage = Math.Round(((double)result / totalMarks) * 100, 2);

                var testResultModel = new TestResultViewModel
                {
                    TestTitle = test.TestTitle,
                    persentage = userPercentage,
                    MarkScored = test.TestScore,
                    MarksOutOff = totalMarks,
                    PassPoints = test.Points
                };
                if (test.TestResultStatus)
                {
                    testResultModel.Result = "Passed";
                }
                else
                {
                    testResultModel.Result = "Fail";
                }
                if (test.TrophyName == null)
                {
                    testResultModel.IsTrophyPic = false;
                }
                else
                {
                    var path = System.Web.Hosting.HostingEnvironment.MapPath(ConfigurationManager.AppSettings["TrophyPicDir"]);
                    var temp = Path.Combine(path, "Temp");
                    var old = Path.Combine(path, "Old");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    if (!Directory.Exists(temp))
                        Directory.CreateDirectory(temp);
                    if (!Directory.Exists(old))
                        Directory.CreateDirectory(old);
                    var subPath = Path.Combine(temp, $"{Path.GetFileNameWithoutExtension(test.TrophyName)}_{test.TestDate.Ticks}.png");
                    if (File.Exists(subPath))
                    {
                        testResultModel.IsTrophyPic = true;
                        testResultModel.TrophyPicName = Path.GetFileName(subPath);
                    }
                    else if (test.TrophyData != null)
                    {
                        using (var stream = new MemoryStream(test.TrophyData))
                        {
                            using (var image = Bitmap.FromStream(stream, true, true))
                            {
                                image.Save(subPath, System.Drawing.Imaging.ImageFormat.Png);
                                testResultModel.IsTrophyPic = true;
                                testResultModel.TrophyPicName = Path.GetFileName(subPath);
                            }
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(test.TrophyName))
                    {
                        var trophyPath = Path.Combine(old, Path.GetFileNameWithoutExtension(test.TrophyName) + ".png");
                        if (File.Exists(trophyPath))
                        {
                            using (var image = Bitmap.FromFile(trophyPath, true))
                            {
                                image.Save(subPath, System.Drawing.Imaging.ImageFormat.Png);
                                testResultModel.IsTrophyPic = true;
                                testResultModel.TrophyPicName = Path.GetFileName(subPath);
                            }
                            test.TrophyData = File.ReadAllBytes(trophyPath);
                        }
                    }
                }
                testResultList.Add(testResultModel);
            }
            _testResultRepository.SaveChanges();
            return testResultList;
        }
    }
}