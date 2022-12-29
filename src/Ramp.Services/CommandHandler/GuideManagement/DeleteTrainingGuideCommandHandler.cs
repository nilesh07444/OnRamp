using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using Ramp.Contracts.QueryParameter.Feedback;
using Ramp.Contracts.ViewModel;
using System.Linq;
using System.Collections.Generic;
using Domain.Customer.Models.Feedback;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class DeleteTrainingGuideCommandHandler : CommandHandlerBase<DeleteTrainingGuideCommandParameter>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IQueryExecutor _executor;
        private readonly IRepository<Feedback> _feedbackRepository;
        private readonly IRepository<FeedbackRead> _feedbackReadRepository;
        public DeleteTrainingGuideCommandHandler(IRepository<TrainingGuide> trainingGuideRepository, IRepository<TrainingTest> trainingTestRepository,IQueryExecutor executor,IRepository<Feedback> feedbackRepository, IRepository<FeedbackRead> feedbackReadRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
            _trainingTestRepository = trainingTestRepository;
            _executor = executor;
            _feedbackReadRepository = feedbackReadRepository;
            _feedbackRepository = feedbackRepository;
        }

        public override CommandResponse Execute(DeleteTrainingGuideCommandParameter command)
        {
            var trainingGuide = _trainingGuideRepository.Find(command.TrainingGuidId);

            var allTrainingGuidesWhoRefresToTrainingGuideToBeDeleted =
                _trainingGuideRepository.List.Where(tg => tg.ChapterList.Any(c => c.LinkedTrainingGuide != null && c.LinkedTrainingGuide.Id == trainingGuide.Id));

            foreach (var guide in allTrainingGuidesWhoRefresToTrainingGuideToBeDeleted)
            {
                guide.ChapterList.Where(
                    c => c.LinkedTrainingGuide != null && c.LinkedTrainingGuide.Id == trainingGuide.Id)
                    .ToList()
                    .ForEach(delegate (TraningGuideChapter chapter)
                    {
                        chapter.LinkedTrainingGuide = null;
                    });
            }
            if (!command.DoNotRemoveTest && trainingGuide.TestVersion != null)
            {
                trainingGuide.TestVersion.CurrentVersion = null;
                trainingGuide.TestVersion.LastPublishedVersion = null;
                foreach(var t in trainingGuide.TestVersion.Versions)
                {
                    t.Deleted = true;
                    _trainingTestRepository.SaveChanges();
                }
            }
            trainingGuide.TestVersion = null;
            var feedback = _executor.Execute<GetFeedbackForPlaybookQueryParameter, List<FeedbackViewModel>>(new GetFeedbackForPlaybookQueryParameter { ReferenceId = trainingGuide.ReferenceId });
            feedback.ForEach(delegate (FeedbackViewModel x)
            {
                var allRead = _feedbackReadRepository.List.AsQueryable().Where(y => y.Feedback.Id == x.Id).ToList();
                allRead.ForEach(y => _feedbackReadRepository.Delete(y));
                _feedbackRepository.Delete(_feedbackRepository.Find(x.Id));
            });
            _feedbackReadRepository.SaveChanges();
            _feedbackRepository.SaveChanges();
            trainingGuide.TrainingLabels.Clear();
            _trainingGuideRepository.Delete(trainingGuide);
            _trainingGuideRepository.SaveChanges();
            return null;
        }
    }
}