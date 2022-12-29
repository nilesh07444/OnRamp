using Common;
using Common.Command;
using Common.Data;
using Common.Events;
using Common.Query;
using Domain.Customer.Models;
using Domain.Customer.Models.Feedback;
using LinqKit;
using Ramp.Contracts.CommandParameter.Feedback;
using Ramp.Contracts.Events.Feedback;
using Ramp.Contracts.QueryParameter.Feedback;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace Ramp.Services.Handler
{
    public class FeedbackHandler :
        ICommandHandlerBase<SaveFeedbackCommand>,
        ICommandHandlerBase<ReadPlaybookFeedbackCommandParameter>,
        ICommandHandlerBase<ReadTestFeedbackCommandParameter>,
        IQueryHandler<GetFeedbackQueryParameter, FeedbackViewModel>,
        IQueryHandler<GetBulkFeedbackQueryParameter, IList<FeedbackViewModel>>,
        IQueryHandler<GetFeedbackForPlaybookQueryParameter, List<FeedbackViewModel>>,
        IQueryHandler<GetFeedbackForTestQueryParameter, List<FeedbackViewModel>>

    {
        private readonly IRepository<Feedback> _repository;
        private readonly IRepository<FeedbackRead> _readRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingTest> _testRepository;

        public FeedbackHandler(IRepository<Feedback> repository,
                                IRepository<FeedbackRead> readRepository,
                                IRepository<StandardUser> userRepository,
                                IRepository<TrainingGuide> trainingGuideRepository,
                                IRepository<TrainingTest> testRepository)
        {
            _repository = repository;
            _readRepository = readRepository;
            _userRepository = userRepository;
            _trainingGuideRepository = trainingGuideRepository;
            _testRepository = testRepository;
        }

        public CommandResponse Execute(SaveFeedbackCommand command)
        {
            var guide = _trainingGuideRepository.List.AsQueryable().Where(x => x.ReferenceId.Equals(command.Subject.Replace("T", string.Empty))).FirstOrDefault();
            if (guide == null)
                throw new Exception($"No Test found with reference :{command.Subject}");
            var feedback = new Domain.Customer.Models.Feedback.Feedback()
            {
                FeedbackDate = DateTime.Now,
                Id = Guid.NewGuid(),
                Message = command.Message,
                Option = command.Option,
                Subject = command.Type == FeedbackType.Playbook? guide.Title :  guide.TestVersion.LastPublishedVersion?.Id.ToString(),
                Type = command.Type,
                UserId = command.UserId,
                Reads = new List<FeedbackRead>()
            };

            _repository.Add(feedback);

            if (command.Type == FeedbackType.Playbook)
            {
                new EventPublisher().Publish(new FeedbackCreatedEvent()
                {
                    Id = feedback.Id,
                    FeedbackViewModel = PostProcess(Project
                                                        .FeedbackViewModelFrom(feedback)),
                    TrainingGuideViewModel = Project.TrainingGuideViewModelFrom(guide)
                });
            }
            else if (command.Type == FeedbackType.Test)
            {

                new EventPublisher().Publish(new FeedbackCreatedEvent()
                {
                    Id = feedback.Id,
                    FeedbackViewModel = PostProcess(Project
                                                        .FeedbackViewModelFrom(feedback)),
                    TrainingTestViewModel = Project.TrainingTestViewModelFrom(guide.TestVersion.LastPublishedVersion),
                    TrainingGuideViewModel = Project.TrainingGuideViewModelFrom(guide)
                });
            }

            return null;
        }

        public FeedbackViewModel ExecuteQuery(GetFeedbackQueryParameter query)
        {
            var feedback = _repository
                                .List
                                .FirstOrDefault(f => f.Id == query.Id);
            
            return PostProcess(Project
                                .FeedbackViewModelFrom(feedback));
        }

        private FeedbackViewModel PostProcess(FeedbackViewModel model)
        {
            var user = _userRepository.List.FirstOrDefault(f => f.Id == model.UserId);
            model.User = Project.UserViewModelFrom(user ?? new StandardUser { Id = Guid.NewGuid(), FirstName = "Deleted" });
            Guid id;
            if (Guid.TryParse(model.Subject, out id))
            {
                if (model.Type == FeedbackType.Test)
                {
                    var test = _testRepository.Find(id);
                    if (test == null)
                        return model;
                    model.TestVersion = test.Version;
                    model.Subject = test.TestTitle;
                }
                else if(model.Type == FeedbackType.Playbook)
                {
                    string title = null;
                    title = _trainingGuideRepository.Find(id)?.Title;
                    if (string.IsNullOrWhiteSpace(title))
                        title = _testRepository.Find(id)?.TrainingGuide?.Title;
                    model.Subject = title;
                }
            }
            else if (model.Subject.Contains("RF"))
            {
                var guide = _trainingGuideRepository.List.FirstOrDefault(x => x.ReferenceId.Equals(model.Subject));
                if (guide != null)
                    model.Subject = guide.Title;
            }

            return model;
        }
        private List<Feedback> GetFeebackByReferenceId(string referenceId)
        {
            var playbook = _trainingGuideRepository.List.AsQueryable().Where(x => x.ReferenceId.Equals(referenceId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (playbook == null)
                return new List<Feedback>();
            var allSubjectsUsedForPlaybooks = new List<string>();
            allSubjectsUsedForPlaybooks.Add(referenceId);
            allSubjectsUsedForPlaybooks.Add(playbook.Id.ToString());
            allSubjectsUsedForPlaybooks.Add(playbook.Title);
            if (playbook.TestVersion != null && playbook.TestVersion.LastPublishedVersion != null)
                allSubjectsUsedForPlaybooks.Add(playbook.TestVersion.LastPublishedVersion.Id.ToString());

            return _repository.List.AsQueryable()
                .Where(w => w.Type == FeedbackType.Playbook && allSubjectsUsedForPlaybooks.Any(x => x.Equals(w.Subject, StringComparison.InvariantCultureIgnoreCase)))
                .OrderByDescending(o => o.FeedbackDate).ToList();
        }
        public List<FeedbackViewModel> ExecuteQuery(GetFeedbackForPlaybookQueryParameter query)
        {
            var feedback = GetFeebackByReferenceId(query.ReferenceId);
            return feedback.ToList().Select(s => PostProcess(Project.FeedbackViewModelFrom(s))).ToList();
        }

        public CommandResponse Execute(ReadPlaybookFeedbackCommandParameter command)
        {
            var feedbacks = GetFeebackByReferenceId(command.ReferenceId);

            foreach (var feedback in feedbacks.ToList())
            {
                if (!feedback.Reads.Any(a => a.UserId == command.UserId))
                {
                    _readRepository.Add(new FeedbackRead()
                    {
                        Feedback = feedback,
                        ReadOn = DateTime.Now,
                        UserId = command.UserId
                    });

                }
            }

            return null;
        }
        private IEnumerable<string> GetTestSiblingsIds(string testId)
        {
            try
            {
                Guid temp;
                Guid.TryParse(testId ?? string.Empty, out temp);
                var t = _testRepository.Find(temp);
                var guide = _trainingGuideRepository.Find(t?.TrainingGuideId);
                if (guide == null)
                    throw new Exception();
                return guide.TestVersion.Versions.Where(x => !x.Deleted.HasValue || (x.Deleted.HasValue && !x.Deleted.Value)).Select(x => x.Id.ToString());
            }
            catch (Exception) { }
            return Enumerable.Empty<string>();
            
        }
        public List<FeedbackViewModel> ExecuteQuery(GetFeedbackForTestQueryParameter query)
        {
            var subjects = GetTestSiblingsIds(query.ReferenceId).Where(x => !string.IsNullOrEmpty(x));

            var feedback = _repository.List
                                    .Where(w => w.Type == FeedbackType.Test &&
                                                subjects.Contains(w.Subject))
                                    .OrderByDescending(o => o.FeedbackDate);
            
            return feedback
                        .Select(s =>
                                PostProcess(Project.FeedbackViewModelFrom(s)))
                        .ToList();

        }

        public CommandResponse Execute(ReadTestFeedbackCommandParameter command)
        {
            var subjects = GetTestSiblingsIds(command.ReferenceId);
            var feedbacks = _repository.List
                                    .Where(w => w.Type == FeedbackType.Test &&
                                                subjects.Contains(w.Subject))
                                    .OrderByDescending(o => o.FeedbackDate);
            foreach (var feedback in feedbacks)
            {
                if (!feedback.Reads.Any(a => a.UserId == command.UserId))
                {
                    _readRepository.Add(new FeedbackRead()
                    {
                        Feedback = feedback,
                        ReadOn = DateTime.Now,
                        UserId = command.UserId
                    });
                }
            }
            return null;
        }
        public IList<FeedbackViewModel> ExecuteQuery(GetBulkFeedbackQueryParameter query)
        {
            var feedback = _repository.List.Where(x => x.Subject != null);

            if (query.Type == 0)
            {
                query.Type = FeedbackType.Playbook;
            }

            feedback = feedback.Where(w => w.Type == query.Type);

            if (!string.IsNullOrEmpty(query.Text))
            { 
                feedback = feedback
                                    .Where(w => w.Subject.ToLower().Contains(query.Text.ToLower()));
            }

            if (query.StartDate.HasValue)
            {
                query.StartDate = query.StartDate.AtBeginningOfDay();
                feedback = feedback.Where(w => w.FeedbackDate >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                query.EndDate = query.EndDate.AtEndOfDay();
                feedback = feedback.Where(w => w.FeedbackDate <= query.EndDate.Value);
            }

            feedback = feedback.OrderByDescending(o => o.FeedbackDate);

            return feedback
                        .Select(s =>
                                PostProcess(Project.FeedbackViewModelFrom(s)))
                        .ToList();
        }
    }
}
