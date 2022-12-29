using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Common.Query;
using Ramp.Services.Helpers;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.CommandParameter.Categories;
using Ramp.Contracts.CommandParameter.Upload;
using Ramp.Services.Projection;
using Ramp.Contracts.CommandParameter.TestManagement;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class TrainingGuideCloningCommandHandler : ICommandHandlerBase<CloneTrainingGuideCommand>,
                                                      IValidator<CloneTrainingGuideCommand>,
                                                      ICommandHandlerBase<CloneTrainingGuideChapterCommand>,
                                                      IValidator<CloneTrainingGuideChapterCommand>,
                                                      ICommandHandlerBase<CloneChapterLinkCommand>,
                                                      IValidator<CloneChapterLinkCommand>,
                                                      ICommandHandlerBase<CloneChapterUploadCommand>,
                                                      IValidator<CloneChapterUploadCommand>,
                                                      ICommandHandlerBase<CloneUploadCommand>,
                                                      IValidator<CloneUploadCommand>,
                                                      ICommandHandlerBase<CloneCKEUploadCommand>,
                                                      IValidator<CloneCKEUploadCommand>
    {
        private readonly ICommandDispatcher _dispatcher;
        private readonly IRepository<TrainingGuide> _guideRepository;
        private readonly IRepository<TraningGuideChapter> _chapterRepository;
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;
        private readonly IRepository<ChapterLink> _chapterLinkReposiotry;
        private readonly IRepository<ChapterUpload> _chapterUploadRepository;
        private readonly IRepository<FileUploads> _uploadRepository;
        private readonly IRepository<CKEUpload> _ckeUploadRepository;
        private readonly IQueryExecutor _executor;
        public TrainingGuideCloningCommandHandler(IRepository<TrainingGuide> guideRepository,
                                                  IRepository<TraningGuideChapter> chapterRepository,
                                                  IRepository<Domain.Customer.Models.Categories> categoryRepository,
                                                  IRepository<ChapterLink> chapterLinkReposiotry,
                                                  IRepository<ChapterUpload> chapterUploadRepository,
                                                  IRepository<FileUploads> uploadRepository,
                                                  IRepository<CKEUpload> ckeUploadRepository,
                                                  ICommandDispatcher dispatcher,
                                                  IQueryExecutor executor)
        {
            _dispatcher = dispatcher;
            _guideRepository = guideRepository;
            _chapterRepository = chapterRepository;
            _categoryRepository = categoryRepository;
            _chapterLinkReposiotry = chapterLinkReposiotry;
            _chapterUploadRepository = chapterUploadRepository;
            _uploadRepository = uploadRepository;
            _ckeUploadRepository = ckeUploadRepository;
            _executor = executor;
        }


        #region TrainingGuide
        public CommandResponse Execute(CloneTrainingGuideCommand command)
        {
            var guide = _guideRepository.Find(command.Id);
            var clone = new TrainingGuide
            {
                Title = $"{guide.Title}-{DateTime.Now}",
                IsActive = false,
                Id = command.CloneId,
                Description = guide.Description,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = command.CurrentlyLoggedInUserId,
                PlaybookPreviewMode = guide.PlaybookPreviewMode,
                Printable = guide.Printable
            };
            clone.ReferenceId = LastReferenceIdForGuide.GetGuideReference(new QueryExecutor().Execute<LastReferenceIdFromGuideQueryParameter, string>(new LastReferenceIdFromGuideQueryParameter()));
            _guideRepository.Add(clone);
            clone.Collaborators = guide.Collaborators;
            if(guide.CoverPicture != null)
            {
                var cloneUploadCommand = new CloneUploadCommand { CloneId = Guid.NewGuid(), Id = guide.CoverPicture.Id };
                var result = _dispatcher.Dispatch(cloneUploadCommand);
                if (result.Validation.Any())
                    throw new Exception(string.Join(";", result.Validation.Select(x => x.ToString())));
                clone.CoverPicture = _uploadRepository.Find(cloneUploadCommand.CloneId);
            }
            guide.ChapterList.ForEach(delegate (TraningGuideChapter chapter)
            {
                var cChapterCommand = new CloneTrainingGuideChapterCommand { Id = chapter.Id, CloneId = Guid.NewGuid(), CurrentlyLoggedInUserId = command.CurrentlyLoggedInUserId, TrainingGuideId = command.CloneId };
                var result = _dispatcher.Dispatch(cChapterCommand);
                if (result.Validation.Any())
                    throw new Exception(string.Join(";", result.Validation.Select(x => x.ToString())));
                //clone.ChapterList.Add(_chapterRepository.Find(cChapterCommand.CloneId));
            });
            guide.Categories.ForEach(x => clone.Categories.Add(x));
            clone.TestVersion = new TestVersion { TrainingGuide = clone, Id = Guid.NewGuid() };
            _guideRepository.SaveChanges();
            if (guide.TestVersion.LastPublishedVersion != null)
            {
                var trainingTest = Project.TrainingTestViewModelFrom(guide.TestVersion.LastPublishedVersion);
                trainingTest.TrainingTestId = Guid.NewGuid();
                foreach (var q in trainingTest.QuestionsList.OrderBy(x => x.TestQuestionNumber).ToList())
                {
                    q.TrainingTestId = trainingTest.TrainingTestId;
                    q.TrainingTestQuestionId = Guid.NewGuid();
                    foreach (var a in q.TestAnswerList.OrderBy(x => x.Position).ToList())
                    {
                        bool correct = false;
                        if (a.TestAnswerId.Equals(q.CorrectAnswerId))
                            correct = true;
                        a.TestAnswerId = Guid.NewGuid();
                        a.TrainingQuestionId = q.TrainingTestQuestionId;
                        if (correct)
                            q.CorrectAnswerId = a.TestAnswerId;
                    }
                }
                trainingTest.SelectedTrainingGuideId = clone.Id;
                trainingTest.Version = 1;
                trainingTest.TestTitle = $"{trainingTest.TestTitle}-{DateTime.Now}";
                trainingTest.ParentTrainingTestId = Guid.Empty;
                new CommandDispatcher().Dispatch(new SaveTrainingTestCommand { TrainingTestViewModel = trainingTest });
            }
            return null;
        }
        public IEnumerable<IValidationResult> Validate(CloneTrainingGuideCommand argument)
        {
            if (_guideRepository.Find(argument.Id) == null)
                yield return new ValidationResult("Id", $"No Training Guide with id {argument.Id} found");
            if(argument.CurrentlyLoggedInUserId == Guid.Empty)
                yield return new ValidationResult("CurrentlyLoggedInUserId", $"Invalid User");
            if (argument.CloneId == Guid.Empty)
                yield return new ValidationResult("CloneId", $"Invalid Id For Cloned Training Guide");

        }
        #endregion

        #region Chapter
        public CommandResponse Execute(CloneTrainingGuideChapterCommand command)
        {
            var chapter = _chapterRepository.Find(command.Id);
            var clone = new TraningGuideChapter
            {
                ChapterContent = chapter.ChapterContent,
                ChapterName = chapter.ChapterName,
                ChapterNumber = chapter.ChapterNumber,
                Id = command.CloneId,
                TrainingGuide = _guideRepository.Find(command.TrainingGuideId)
            };
            _chapterRepository.Add(clone);
            chapter.ChapterLinks.ForEach(delegate (ChapterLink cl)
            {
                var cloneChapterLinkCommand = new CloneChapterLinkCommand { CloneId = Guid.NewGuid(), Id = cl.Id,TrainingGuideChapterId = command.CloneId };
                var result = _dispatcher.Dispatch(cloneChapterLinkCommand);
                if(result.Validation.Any())
                    throw new Exception(string.Join(";", result.Validation.Select(x => x.ToString())));
            });
            chapter.ChapterUploads.ForEach(delegate (ChapterUpload cu)
            {
                var cloneChapterUploadCommand = new CloneChapterUploadCommand { CloneId = Guid.NewGuid(), Id = cu.Id, TrainingGuideChapterId = command.CloneId };
                var result = _dispatcher.Dispatch(cloneChapterUploadCommand);
                if (result.Validation.Any())
                    throw new Exception(string.Join(";", result.Validation.Select(x => x.ToString())));
            });
            chapter.CKEUploads.ForEach(delegate (CKEUpload ckeu)
            {
                var cloneChapterCKEUploadCommand = new CloneCKEUploadCommand { CloneId = Guid.NewGuid(), Id = ckeu.Id, TrainingGuideChapterId = command.CloneId };
                var result = _dispatcher.Dispatch(cloneChapterCKEUploadCommand);
                if (result.Validation.Any())
                    throw new Exception(string.Join(";", result.Validation.Select(x => x.ToString())));
            });
            _chapterRepository.SaveChanges();
            return null;
        }
        public IEnumerable<IValidationResult> Validate(CloneTrainingGuideChapterCommand argument)
        {
            if (_chapterRepository.Find(argument.Id) == null)
                yield return new ValidationResult("Id", $"No Training Guide Chapter with id {argument.Id} found");
            if (argument.CurrentlyLoggedInUserId == Guid.Empty)
                yield return new ValidationResult("CurrentlyLoggedInUserId", $"Invalid User");
            if (argument.CloneId == Guid.Empty)
                yield return new ValidationResult("CloneId", $"Invalid Id For Cloned Training Guide Chapter");
            if (_guideRepository.Find(argument.TrainingGuideId) == null)
                yield return new ValidationResult("Id", $"No Training Guide with id {argument.Id} found");
        }
        #endregion

        #region ChapterLink
        public CommandResponse Execute(CloneChapterLinkCommand command)
        {
            var cl = _chapterLinkReposiotry.Find(command.Id);
            var clone = new ChapterLink
            {
                ChapterUploadSequence = cl.ChapterUploadSequence,
                Id = command.CloneId,
                Type = cl.Type,
                Url = cl.Url,
                TraningGuideChapter = _chapterRepository.Find(command.TrainingGuideChapterId)
            };
            _chapterLinkReposiotry.Add(clone);
            _chapterLinkReposiotry.SaveChanges();
            return null;
        }
        public IEnumerable<IValidationResult> Validate(CloneChapterLinkCommand argument)
        {
            if (_chapterLinkReposiotry.Find(argument.Id) == null)
                yield return new ValidationResult("Id", $"No Training Guide Chapter Link with id {argument.Id} found");
            if (argument.CloneId == Guid.Empty)
                yield return new ValidationResult("CloneId", $"Invalid Id For Cloned Training Guide Chapter Link");
            if (_chapterRepository.Find(argument.TrainingGuideChapterId) == null)
                yield return new ValidationResult("Id", $"No Training Guide Chapter with id {argument.Id} found");
        }
        #endregion

        #region ChapterUpload
        public CommandResponse Execute(CloneChapterUploadCommand command)
        {
            var cu = _chapterUploadRepository.Find(command.Id);
            var clone = new ChapterUpload
            {
                ChapterUploadSequence = cu.ChapterUploadSequence,
                Content = cu.Content,
                Id = command.CloneId,
                TraningGuideChapter = _chapterRepository.Find(command.TrainingGuideChapterId)
            };
            if(cu.Upload != null)
            {
                var cloneUploadCommand = new CloneUploadCommand { CloneId = Guid.NewGuid(), Id = cu.Upload.Id };
                var result = _dispatcher.Dispatch(cloneUploadCommand);
                if (result.Validation.Any())
                    throw new Exception(string.Join(";", result.Validation.Select(x => x.ToString())));
                clone.Upload = _uploadRepository.Find(cloneUploadCommand.CloneId);
            }
            _chapterUploadRepository.Add(clone);
            return null;
        }
        public IEnumerable<IValidationResult> Validate(CloneChapterUploadCommand argument)
        {
            if (_chapterUploadRepository.Find(argument.Id) == null)
                yield return new ValidationResult("Id", $"No Training Guide Chapter Upload with id {argument.Id} found");
            if (argument.CloneId == Guid.Empty)
                yield return new ValidationResult("CloneId", $"Invalid Id For Cloned Training Guide Chapter Upload");
            if (_chapterRepository.Find(argument.TrainingGuideChapterId) == null)
                yield return new ValidationResult("Id", $"No Training Guide Chapter with id {argument.Id} found");
        }
        #endregion

        #region Upload
        public CommandResponse Execute(CloneUploadCommand command)
        {
            var upload = _uploadRepository.Find(command.Id);
            var clone = new FileUploads
            {
                ContentType = upload.ContentType,
                Data = upload.Data,
                Description = upload.Description,
                Name = upload.Name,
                Type = upload.Type,
                Id = command.CloneId
            };
            _uploadRepository.Add(clone);
            return null;
        }

        public IEnumerable<IValidationResult> Validate(CloneUploadCommand argument)
        {
            if (_uploadRepository.Find(argument.Id) == null)
                yield return new ValidationResult("Id", $"No Training Guide Chapter Upload -> Upload with id {argument.Id} found");
            if (argument.CloneId == Guid.Empty)
                yield return new ValidationResult("CloneId", $"Invalid Id For Cloned Training Guide Chapter Upload -> Upload");
        }
        #endregion

        #region CKEUpload
        public CommandResponse Execute(CloneCKEUploadCommand command)
        {
            var ckeu = _ckeUploadRepository.Find(command.Id);
            var chapter = _chapterRepository.Find(command.TrainingGuideChapterId);

            var clone = new CKEUpload
            {
                Id = command.CloneId,
                TrainingGuideChapter = chapter,
                Type = ckeu.Type
            };
            if (ckeu.Upload != null)
            {
                var cloneUploadCommand = new CloneUploadCommand { CloneId = Guid.NewGuid(), Id = ckeu.Upload.Id };
                var result = _dispatcher.Dispatch(cloneUploadCommand);
                if (result.Validation.Any())
                    throw new Exception(string.Join(";", result.Validation.Select(x => x.ToString())));
                clone.Upload = _uploadRepository.Find(cloneUploadCommand.CloneId);
                if (!string.IsNullOrWhiteSpace(chapter.ChapterContent))
                    chapter.ChapterContent = chapter.ChapterContent.Replace($"/Upload/Get/{ckeu.Upload.Id}", $"/Upload/Get/{cloneUploadCommand.CloneId}").Replace($"/Upload/GetThumbnail/{ckeu.Upload.Id}", $"/Upload/GetThumbnail/{cloneUploadCommand.CloneId}");
            }
            _ckeUploadRepository.Add(clone);
            _ckeUploadRepository.SaveChanges();
           
            return null;
        }

        public IEnumerable<IValidationResult> Validate(CloneCKEUploadCommand argument)
        {
            if (_ckeUploadRepository.Find(argument.Id) == null)
                yield return new ValidationResult("Id", $"No Training Guide Chapter CKE Upload with id {argument.Id} found");
            if (argument.CloneId == Guid.Empty)
                yield return new ValidationResult("CloneId", $"Invalid Id For Cloned Training Guide Chapter CKE Upload");
            if (_chapterRepository.Find(argument.TrainingGuideChapterId) == null)
                yield return new ValidationResult("Id", $"No Training Guide Chapter with id {argument.Id} found");
        }
        #endregion
    }
}
