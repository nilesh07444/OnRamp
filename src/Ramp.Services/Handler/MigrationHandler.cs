using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Customer.Models.Feedback;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Migration;
using Ramp.Contracts.CommandParameter.Portal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Handler
{
    public class MigrationHandler :
        ICommandHandlerBase<MigrateCompanyPlaybooksAndTestToVersionsCommand>,
        ICommandHandlerBase<MigrateTestFeedbackToTestIdCommand>,
        ICommandHandlerBase<MigrateCustomConfigurationDataCommand>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<TrainingGuide> _guideRepository;
        private readonly IRepository<TrainingTest> _testRepository;
        private readonly IRepository<TestVersion> _testVersionRepository;
        private readonly IRepository<Feedback> _feedbackRepository;
        private readonly IRepository<FeedbackRead> _readFeedbackRepository;
        private readonly IRepository<CustomConfiguration> _customConfigurationRepository;
        private readonly ICommandDispatcher _commandDispatcher;
        public MigrationHandler(IRepository<Company> companyRepository,
            IRepository<TrainingGuide> guideRepository,
            IRepository<TrainingTest> testRepository,
            IRepository<TestVersion> testVersionRepository,
            IRepository<Feedback> feedbackRepository,
            IRepository<FeedbackRead> readFeedbackRepository,
            IRepository<CustomConfiguration> customConfigurationRepository,
            ICommandDispatcher commandDispatcher)
        {
            _companyRepository = companyRepository;
            _guideRepository = guideRepository;
            _testRepository = testRepository;
            _testVersionRepository = testVersionRepository;
            _feedbackRepository = feedbackRepository;
            _readFeedbackRepository = readFeedbackRepository;
            _customConfigurationRepository = customConfigurationRepository;
            _commandDispatcher = commandDispatcher;
        }
      

        public CommandResponse Execute(MigrateCompanyPlaybooksAndTestToVersionsCommand command)
        {
            try
            {
                var playbooks = _guideRepository.List.ToList();
                foreach (var p in playbooks)
                {
                    var v = new TestVersion { Id = Guid.NewGuid(), TrainingGuide = p };
                    _testVersionRepository.Add(v);

                    var pe = _guideRepository.Find(p.Id);
                    pe.TestVersion = v;
                    _guideRepository.SaveChanges();

                    var testForPlaybook = _testRepository.List.Where(x => x.TrainingGuideId.Equals(p.Id)).OrderBy(x => x.Version).ToList();
                    foreach (var t in testForPlaybook)
                    {
                        pe.TestVersion.Versions.Add(t);
                    }
                    pe.TestVersion.LastPublishedVersion = pe.TestVersion.Versions.LastOrDefault(x => x.ActivePublishDate.HasValue && x.ParentTrainingTestId.Equals(Guid.Empty) && x.ActiveStatus);
                    if (pe.TestVersion.LastPublishedVersion != null)
                        pe.TestVersion.CurrentVersion = pe.TestVersion.Versions.LastOrDefault(x => !x.ActivePublishDate.HasValue && x.ParentTrainingTestId != Guid.Empty && x.DraftStatus && x.Version == pe.TestVersion.LastPublishedVersion.Version);

                    if (pe.TestVersion.LastPublishedVersion != null && pe.TestVersion.CurrentVersion == null)
                        pe.TestVersion.CurrentVersion = pe.TestVersion.LastPublishedVersion;
                    _testVersionRepository.SaveChanges();
                }
            }catch(Exception ex)
            {
                if (!ex.InnerException.Message.Contains("Invalid"))
                    throw ex;
            }
            return null;
        }

        public CommandResponse Execute(MigrateTestFeedbackToTestIdCommand command)
        {
            var feedback = _feedbackRepository.List.AsQueryable().Where(x => x.Subject != null && x.Subject.Contains("RF") && x.Subject.EndsWith("T")).ToList();
            var guideList = feedback.Select(x => x.Subject.Replace("T", string.Empty)).ToList();
            var guides = _guideRepository.List.AsQueryable().Where(x => guideList.Contains(x.ReferenceId)).ToList();
            foreach (var f in feedback)
            {
                var fe = _feedbackRepository.Find(f.Id);
                if (fe == null)
                    continue;
                var guide = guides.FirstOrDefault(x => x.ReferenceId.Equals(fe.Subject.Replace("T", string.Empty), StringComparison.InvariantCultureIgnoreCase));
                if (guide == null)
                    continue;
                fe.Subject = guide.TestVersion.LastPublishedVersion?.Id.ToString();
                _feedbackRepository.SaveChanges();
            }
            return null;
        }

        public CommandResponse Execute(MigrateCustomConfigurationDataCommand command)
        {
            _customConfigurationRepository.List.ToList().ForEach(delegate (CustomConfiguration config)
            {
                if (config.Certificate != null)
                {
                    var c = new CustomConfiguration
                    {
                        Id = Guid.NewGuid(),
                        Upload = config.Certificate,
                        Type = CustomType.Certificate,
                        Version = 0
                    };
                    _customConfigurationRepository.Add(c);
                }
                if (config.CSS != null)
                {
                    var css = new CustomConfiguration
                    {
                        Id = Guid.NewGuid(),
                        Type = CustomType.CSS,
                        Upload = config.CSS,
                        Version = 0
                    };
                    _customConfigurationRepository.Add(css);
                }
                _customConfigurationRepository.Delete(config);
            });
            return null;
        }
    }
}
