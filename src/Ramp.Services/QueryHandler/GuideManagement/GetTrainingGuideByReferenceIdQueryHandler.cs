using Common;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class GetTrainingGuideByReferenceIdQueryHandler :
        IQueryHandler<GetTrainingGuideByReferenceIdQueryParameter, TrainingGuideViewModel>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IQueryExecutor _executor;
        private readonly IRepository<TestResult> _testResultRepository;
        public GetTrainingGuideByReferenceIdQueryHandler(
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<TestAssigned> assignedTestRepository,
            IRepository<TestResult> testResultRepository,
            IQueryExecutor executor)
        {
            _trainingGuideRepository = trainingGuideRepository;
            _executor = executor;
            _assignedTestRepository = assignedTestRepository;
            _testResultRepository = testResultRepository;
        }

        public TrainingGuideViewModel ExecuteQuery(GetTrainingGuideByReferenceIdQueryParameter query)
        {
            var trainingGuide = _trainingGuideRepository.List.SingleOrDefault(t => t.ReferenceId.Equals(query.RefrenceId));
            if (trainingGuide == null)
                return null;

            var model = _executor.Execute<TrainingGuideQueryParameter, TrainingGuideViewModel>(new TrainingGuideQueryParameter
            {
                CompanyId = query.CompanyId,
                Id = trainingGuide.Id,
                UploadDeleteUrlBase = query.UploadDeleteUrlBase,
                CoverPictureThumbnailUrlBase = query.CoverPictureThumbnailUrlBase,
                UploadThumbnailUrlBase = query.UploadThumbnailUrlBase,
                UploadUrlBase = query.UploadUrlBase,
                UploadPreviewUrlBase = query.UploadPreviewUrlBase,
            });
            model.TrainingTestId = UserCanTakeTest(trainingGuide.TestVersion.LastPublishedVersion, query);
            return model;
        }
        private Guid UserCanTakeTest(TrainingTest test,GetTrainingGuideByReferenceIdQueryParameter query)
        {
            if (test == null)
                return Guid.Empty;
            var id = Guid.Empty;
            if (query.CurrentlyLoggedInUserId.HasValue
                && _assignedTestRepository.List.AsQueryable().Any(x
                => x.UserId.HasValue && x.UserId.Value == query.CurrentlyLoggedInUserId.Value && x.TestId == test.Id))
            {
                id = _executor.Execute<CheckUserHasAlreadyAppearedForTestQueryParameter, CheckUserHasAlreadyAppearedForTestViewModel>(
                    new CheckUserHasAlreadyAppearedForTestQueryParameter
                    {
                        CurrentlyLoggedInUserId = query.CurrentlyLoggedInUserId.Value,
                        TrainingTestId = test.Id
                    }).IsUserEligibleToTakeTest ? test.Id : Guid.Empty;
            }
            if (id != Guid.Empty)
            {
                if (test.TestExpiryDate.HasValue && test.TestExpiryDate.Value < DateTime.Now)
                    id = Guid.Empty;
            }
            return id;
        }
    }
}