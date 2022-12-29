using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class AllAssignedTestsForAUserQueryHandler :
        QueryHandlerBase<AllAssignedTestsForAUserQueryParameter, List<TrainingTestViewModel>>
    {
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TrainingTestUsageStats> _trainingTestUsageStatsRepository;
        private readonly IRepository<TestCertificate> _testCertificateRepository;
        private readonly IQueryExecutor _queryExecutor;

        public AllAssignedTestsForAUserQueryHandler(IRepository<StandardUser> userRepository,
            IRepository<TestAssigned> assignedTestRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<TrainingTestUsageStats> trainingTestUsageStatsRepository,
            IRepository<TestCertificate> testCertificateRepository,
            IQueryExecutor queryExecutor)
        {
            _userRepository = userRepository;
            _assignedTestRepository = assignedTestRepository;
            _testResultRepository = testResultRepository;
            _trainingTestUsageStatsRepository = trainingTestUsageStatsRepository;
            _testCertificateRepository = testCertificateRepository;
            _queryExecutor = queryExecutor;
        }

        public override List<TrainingTestViewModel> ExecuteQuery(AllAssignedTestsForAUserQueryParameter queryParameters)
        {
            var user = _userRepository.Find(queryParameters.UserId);
            var trainingTestViewModelList = new List<TrainingTestViewModel>();

            List<TestAssigned> list;
            if (user.Group == null)
            {
                list = _assignedTestRepository.GetAll()
                    .Where(tt => tt.UserId == queryParameters.UserId).Distinct()
                    .ToList();
            }
            else
            {
                list = _assignedTestRepository.GetAll()
                    .Where(tt => tt.UserId == queryParameters.UserId || tt.GroupId == user.Group.Id).Distinct()
                    .ToList();
            }
            list = list.Where(tt => (!tt.Test.Deleted.HasValue || (tt.Test.Deleted.HasValue && !tt.Test.Deleted.Value))).ToList();
            var userTestStats = _trainingTestUsageStatsRepository.List.Where(s => s.UserId.Equals(queryParameters.UserId)).ToList();

            foreach (TestAssigned test in list)
            {
                var model = new TrainingTestViewModel();
                model.ReferenceId = test.Test.ReferenceId;
                model.TrainingTestId = test.Test.Id;
                model.TestTitle = test.Test.TestTitle;
                model.CreateDate = test.Test.CreateDate;
                model.PassMarks = test.Test.PassMarks;
                model.TrainingGuideName = test.Test.TrainingGuide.Title;
                model.SelectedTrainingGuideId = test.Test.TrainingGuideId;
                model.ActiveStatus = test.Test.ActiveStatus;
                model.DraftStatus = test.Test.DraftStatus;
                model.ActivePublishDate = test.Test.ActivePublishDate;
                model.TestDuration = test.Test.TestDuration;
                model.DraftEditDate = test.Test.DraftEditDate;
                model.TestExpiryDate = test.Test.TestExpiryDate;
                if (test.AssignedDate.HasValue)
                    model.AssignedDate = test.AssignedDate.Value;

                if (userTestStats.Count > 0)
                {
                    var dateTaken = default(DateTime);
                    var testStat = userTestStats.Where(s => s.TrainingTestId.Equals(test.TestId) && !s.Unassigned).ToList();
                    if (testStat.Count > 0)
                    {
                        if (testStat.Count > 0)
                        {
                            if (testStat[0].DateTaken.HasValue)
                                dateTaken = testStat[0].DateTaken.Value;
                            if (testStat.Count > 1)
                            {
                                for (var i = 1; i < testStat.Count; i++)
                                {
                                    if (testStat[i].DateTaken.HasValue && testStat[i - 1].DateTaken.HasValue)
                                        if (testStat[i].DateTaken.Value > testStat[i - 1].DateTaken.Value)
                                            dateTaken = testStat[i].DateTaken.Value;
                                }
                            }
                        }
                        if (dateTaken != default(DateTime))
                            model.DateTaken = dateTaken;
                    }
                }

                var flag = _testResultRepository.List.Any(tt => tt.TrainingTestId.Equals(test.TestId)
                                                                && tt.TestTakenByUserId.Equals(queryParameters.UserId)
                                                                && tt.TestResultStatus && !tt.MaximumTestRewritesReached);
                model.IsUserEligibleToTakeTest = !flag;
                model.TestStatus = flag;
                if (model.TestStatus)
                {
                    if (queryParameters.InMyTests)
                    {
                        var cert = _queryExecutor.Execute<TestCertificateForResultQueryParameter, FileUploadResultViewModel>(new TestCertificateForResultQueryParameter
                        {
                            UserId = queryParameters.UserId,
                            DefaultCertPath = HostingEnvironment.MapPath("~/Content/images/Certificate.jpg"),
                            ResultId = _testResultRepository.List.FirstOrDefault(tt => tt.TrainingTestId.Equals(test.TestId)
                                                                && tt.TestTakenByUserId.Equals(queryParameters.UserId)
                                                                && tt.TestResultStatus && !tt.MaximumTestRewritesReached).Id
                        });
                        if (cert != null)
                            model.CertificateUrl = queryParameters.CertificateUrlbase.Replace($"{Guid.Empty}", $"{cert.Id}");
                    }
                }
                trainingTestViewModelList.Add(model);
            }
            return trainingTestViewModelList;
        }
    }
}