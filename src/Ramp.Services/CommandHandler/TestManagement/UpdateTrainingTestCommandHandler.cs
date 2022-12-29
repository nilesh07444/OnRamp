using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.CommandParameter.TestManagement.Question;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class UpdateTrainingTestCommandHandler : ICommandHandlerBase<UpdateTrainingTestCommand>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TrainingGuide> _guideRepository;
        private readonly ICommandDispatcher _dispatcher;
        public UpdateTrainingTestCommandHandler(IRepository<TrainingTest> trainingTestRepository,
            IRepository<TrainingGuide>guideRepository,
            ICommandDispatcher dispatcher)
        {
            _trainingTestRepository = trainingTestRepository;
            _guideRepository = guideRepository;
            _dispatcher = dispatcher;
        }
        public CommandResponse Execute(UpdateTrainingTestCommand command)
        {
            var tEdit = command.model;
            var tDomain = _trainingTestRepository.Find(command.model.TrainingTestId);
            if(tDomain.TrainingGuideId != tEdit.SelectedTrainingGuideId)
            {
                var guide = _guideRepository.Find(tDomain.TrainingGuideId);
                var test = guide.TestVersion.Versions.Remove(tDomain);
                if (guide.TestVersion.CurrentVersion != null && guide.TestVersion.CurrentVersion.Id.Equals(tDomain.Id))
                    guide.TestVersion.CurrentVersion = null;
                if (guide.TestVersion.LastPublishedVersion != null && guide.TestVersion.LastPublishedVersion.Id.Equals(tDomain.Id))
                    guide.TestVersion.LastPublishedVersion = null;
                _guideRepository.SaveChanges();
                var newGuide = _guideRepository.Find(tEdit.SelectedTrainingGuideId);
                newGuide.TestVersion.Versions.Add(tDomain);
                newGuide.TestVersion.CurrentVersion = tDomain;
                _guideRepository.SaveChanges();
                tDomain.ReferenceId = newGuide.ReferenceId + "T";
            }

            tDomain.DraftEditDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(tEdit.TrophyDataBase64Encoded))
                tEdit.TestTrophy = Convert.FromBase64String(tEdit.TrophyDataBase64Encoded);
            else if (!string.IsNullOrWhiteSpace(tEdit.TrophyName))
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
                var trophyPath = Path.Combine(old, Path.GetFileNameWithoutExtension(tEdit.TrophyName) + ".png");
                if (File.Exists(trophyPath))
                    tEdit.TestTrophy = File.ReadAllBytes(trophyPath);
            }
            tDomain.AssignMarksToQuestions = tEdit.AssignMarksToQuestions;
            tDomain.IntroductionContent = tEdit.IntroductionContent;
            tDomain.PassMarks = tEdit.PassMarks;
            tDomain.PassPoints = tEdit.PassPoints;
            tDomain.TestDuration = tEdit.TestDuration;
            tDomain.TestExpiryDate = tEdit.IsTestExpiryDate ? null : tEdit.TestExpiryDate;
            tDomain.TestReview = tEdit.TestReview;
            tDomain.TestTitle = tEdit.TestTitle;
            tDomain.TrophyName = tEdit.TrophyName;
            tDomain.TestTrophy = tEdit.TestTrophy;
            tDomain.DraftStatus = tEdit.DraftStatus;
            tDomain.ActiveStatus = tEdit.ActiveStatus;
            tDomain.TrainingGuideId = tEdit.SelectedTrainingGuideId;
            tDomain.MaximumNumberOfRewites = tEdit.EnableMaximumTestRewriteFunction ? tEdit.MaximumRewrites : null;
            tDomain.DisableQuestionRandomization = tEdit.DisableQuestionRandomization;
            tDomain.EmailSummaryOnCompletion = tEdit.EmailSummaryOnCompletion;
            tDomain.HighlightAnswersOnSummary = tEdit.HighlightAnswersOnSummary;
            
            var index = 1;
            foreach(var q in tEdit.QuestionsList)
            {
                q.TestQuestionNumber = index++;
                var answers = q.TestAnswerList.Where(x => !string.IsNullOrWhiteSpace(x.Option) && x.Option != "[Option]").ToList();
                answers.ForEach(x => x.Position = answers.IndexOf(x));
            }

            var currentQuestions = tDomain.QuestionList.Select(x => x.Id).ToList();
            var tEditQuestions = tEdit.QuestionsList.Select(x => x.TrainingTestQuestionId).ToList();
            var updateableQuestions = tEdit.QuestionsList.Where(x => currentQuestions.Contains(x.TrainingTestQuestionId)).ToList();
            var newQuestions = tEdit.QuestionsList.Where(x => !currentQuestions.Contains(x.TrainingTestQuestionId)).ToList();
            var removedQuestions = tDomain.QuestionList.Where(x => !tEditQuestions.Contains(x.Id)).ToList();

            //updateCurrentQuestions
            foreach (var q in updateableQuestions)
            {
                //get ids
                var result = _dispatcher.Dispatch(new UpdateTrainingQuestionCommand { Model = q });
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            //delete removed questions
            foreach (var q in removedQuestions)
            {
                var result = _dispatcher.Dispatch(new DeleteTrainingQuestionCommand { Model = Project.TrainingTestQuestionViewModelFrom(q), TrainingTestId = tDomain.Id });
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            //add new questions
            foreach (var q in newQuestions)
            {
                var result = _dispatcher.Dispatch(new SaveTrainingQuestionCommand { Model = q, TestId = tDomain.Id });
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            _trainingTestRepository.SaveChanges();
            return null;
        }
    }
}
