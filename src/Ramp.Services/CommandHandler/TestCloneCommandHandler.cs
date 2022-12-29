using Common.Command;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Test;
using LinqKit;
using Ramp.Contracts.Command.Test;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using Ramp.Services.Uploads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler
{
    public class TestCloneCommandHandler : ICommandHandlerAndValidator<CloneCommand<Test>>
    {
        private readonly IQueryExecutor _queryExecutor;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ITransientRepository<Test> _repository;
        private readonly ITransientRepository<DocumentCategory> _categoryRepository;
        private readonly ITransientRepository<Upload> _uploadRepository;
        private readonly ITransientRepository<Certificate> _certificateRepository;

        public TestCloneCommandHandler( IQueryExecutor queryExecutor,
                                        ICommandDispatcher commandDispatcher,
                                        ITransientRepository<Test> repository,
                                        ITransientRepository<DocumentCategory> categoryRepository,
                                        ITransientRepository<Upload> uploadRepository,
                                        ITransientRepository<Certificate> certificateRepository)
        {
            _queryExecutor = queryExecutor;
            _commandDispatcher = commandDispatcher;
            _repository = repository;
            _categoryRepository = categoryRepository;
            _uploadRepository = uploadRepository;
            _certificateRepository = certificateRepository;
        }
        public CommandResponse Execute(CloneCommand<Test> command)
        {
            if (!string.IsNullOrWhiteSpace(command.SourceCompanyId))
                _repository.SetCustomerCompany(command.SourceCompanyId);
            var e = _repository.Find(command.Id);
            var clonedUploads = e.Questions.Where(x => !x.Deleted).Select(x => new { x.Id, x.Uploads }).Select(x => x.Uploads.Clone(x.Id)).ToList();
            clonedUploads.AddRange(e.Questions.Where(x => !x.Deleted).Select(x => new { x.Id, x.Question, x.ContentToolsUploads }).Select(x => x.ContentToolsUploads.Clone(x.Id, command.SourceCompanyId, command.TargetCompanyId, x.Question)).ToList());
            if (e.CoverPicture != null)
                clonedUploads.Add(e.CoverPicture.Clone());
            var title = e.Title;
            if (command.SourceCompanyId == command.TargetCompanyId)
            {
                var matchText = e.Title.IndexOf("/") != -1 ? e.Title.Substring(0, e.Title.LastIndexOf("/")) : e.Title;
                var num = _queryExecutor.Execute<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery { MatchText = command.NewVersion ? matchText + "/V" : matchText + "/D" }).Count();
                e.Title = num == 0 && command.NewVersion ? $"{e.Title}/V{++num}" : e.Title;
                e.DocumentStatus = command.NewVersion ? Domain.Customer.DocumentStatus.Recalled : e.DocumentStatus;
                title = command.NewVersion ? $"{matchText}/V{++num}" : $"{matchText}/D{++num}";
            }
            _repository.SetCustomerCompany(command.TargetCompanyId);
            _certificateRepository.SetCustomerCompany(command.TargetCompanyId);
            clonedUploads.SelectMany(x => x.Uploads).ToList().ForEach(u => _uploadRepository.Add(u.Upload));
            _repository.SaveChanges();
            var cloneCommand = Clone(e, clonedUploads,title);
            if (command.SourceCompanyId == command.TargetCompanyId)
            {
                cloneCommand.Collaborators = e.Collaborators;
            }
            _commandDispatcher.Dispatch(cloneCommand);
            command.Id = cloneCommand.Id;
            _repository.SetCustomerCompany(command.SourceCompanyId);
            return null;
        }
        private CreateOrUpdateTestCommand Clone(Test e, IList<UploadContentCloneMapping> uploadCloneMappings, string title)
        {
            var command = new CreateOrUpdateTestCommand
            {
                Id = Guid.NewGuid().ToString(),
				DocumentStatus = Domain.Customer.DocumentStatus.Draft,
                Deleted = false,
                Description = string.IsNullOrEmpty(e.Description) ? "Enter description" : e.Description,
                Points = e.Points,
                Title = title,
                AssignMarksToQuestions = e.AssignMarksToQuestions,
                Duration = e.Duration,
                EmailSummary = e.EmailSummary,
                HighlightAnswersOnSummary = e.HighlightAnswersOnSummary,
                OpenTest = e.OpenTest,
                IntroductionContent = e.IntroductionContent,
                NotificationInteval  = e.NotificationInteval,
                NotificationIntevalDaysBeforeExpiry = e.NotificationIntevalDaysBeforeExpiry,
                PassMarks = e.PassMarks,
                MaximumAttempts = e.MaximumAttempts,
                TestExpiresNumberDaysFromAssignment = e.TestExpiresNumberDaysFromAssignment,
                RandomizeQuestions = e.RandomizeQuestions,
                TestReview = e.TestReview,
                TrainingLabels = e.TrainingLabels,
                EnableTimer = e.EnableTimer,
				IsGlobalAccessed=e.IsGlobalAccessed
            };
            var targetCategory = _categoryRepository.List.AsQueryable().First(x => x.Title == "Default");
            //var targetCategory = e.Category;
            command.Category = Project.Category_CategoryViewModelShort.Invoke(targetCategory);
            command.CoverPicture = e.CoverPicture != null ? Project.Upload_UploadResultViewModel.Invoke(uploadCloneMappings.Where(x => x.Type == UploadCloneMappingType.CoverPicture).FirstOrDefault(e.CoverPictureId)) : null;
			
			//var targetCertificate = e.Certificate == null ? null : _certificateRepository.List.AsQueryable().First(x => x.Upload != null && x.Upload.Description == "Default");
			var targetCertificate = e.Certificate == null ? null : _certificateRepository.List.AsQueryable().First(x => x.Id == e.Certificate.Id);
			command.Certificate = Project.Certificate_UploadResultViewModel.Invoke(targetCertificate);
            foreach (var c in e.Questions.Where(x => !x.Deleted).OrderBy(x => x.Number).ToArray())
            {
                var contentToolUploads = uploadCloneMappings.FirstOrDefault(x => x.Type == UploadCloneMappingType.Html && x.Id == c.Id);

                var question = new TestQuestionModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Marks = c.AnswerWeightage,
                    Question = contentToolUploads?.Content ?? c.Question,
                    Deleted = false,
                    Number = c.Number,
                    CorrectAnswerId = Guid.NewGuid().ToString()
                };
                (question.Answers as List<TestQuestionAnswerModel>).AddRange(c.Answers.AsQueryable().Where(x => !x.Deleted).Select(x => Project.TestQuestionAnswer_TestQuestionAnswerModel.Invoke(x)).OrderBy(x => x.Number));
                question.Answers.ToList().ForEach(a => a.Id = a.Id == c.CorrectAnswerId ? question.CorrectAnswerId : Guid.NewGuid().ToString());
                (question.Attachments as List<UploadResultViewModel>).AddRange(c.Uploads.AsQueryable()
                                                                                         .Where(x => !x.Deleted)
                                                                                         .Select(x => Project.Upload_UploadResultViewModel.Invoke(uploadCloneMappings.Where(u => u.Type == UploadCloneMappingType.Normal && u.Id == c.Id).FirstOrDefault(x.Id)))
                                                                                         .OrderBy(x => x.Number));
                if (contentToolUploads != null)
                    (question.ContentToolsUploads as List<UploadFromContentToolsResultModel>).AddRange(contentToolUploads.Uploads.Select(x => Project.Upload_UploadFromContentToolsResultModel.Invoke(x.Upload)).ToList());
                (command.ContentModels as IList<TestQuestionModel>).Add(question);
            }
            return command;
        }
        public IEnumerable<IValidationResult> Validate(CloneCommand<Test> command)
        {
            return Enumerable.Empty<IValidationResult>();
        }
    }
}
