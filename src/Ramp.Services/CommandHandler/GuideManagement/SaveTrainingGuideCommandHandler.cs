using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using Ramp.Contracts.CommandParameter.TrainingLabel;
using Ramp.Contracts.CommandParameter.Upload;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.QueryParameter.TrainingLabel;
using Ramp.Contracts.QueryParameter.Upload;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class SaveTrainingGuideCommandHandler : ICommandHandlerBase<SaveTrainingGuideCommand>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TraningGuideChapter> _trainingGuideChapterRepository;
        private readonly IRepository<ChapterUpload> _chapterUploadRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<FileUploads> _fileUploadsRepository;
        private readonly IRepository<ChapterLink> _chapterLinkRepository;
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;
        private readonly IRepository<CKEUpload> _ckeUploadRepository;
        private readonly IRepository<TestVersion> _testVersionRepository;
        private readonly ICommandDispatcher _dispatcher;
        private readonly IQueryExecutor _executor;

        public SaveTrainingGuideCommandHandler(
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<TraningGuideChapter> trainingGuideChapterRepository,
            IRepository<ChapterUpload> chapterUploadRepository,
            IRepository<StandardUser> standardUserRepository,
            IRepository<FileUploads> fileUploadsRepository,
            IRepository<ChapterLink> chapterLinkRepository,
            IRepository<Domain.Customer.Models.Categories> categoryRepository,
            IRepository<CKEUpload> ckeUploadRepository,
            IRepository<TestVersion> testVersionRepository,
            ICommandDispatcher dispatcher,
            IQueryExecutor executor
            )
        {
            _trainingGuideChapterRepository = trainingGuideChapterRepository;
            _trainingGuideRepository = trainingGuideRepository;
            _chapterUploadRepository = chapterUploadRepository;
            _standardUserRepository = standardUserRepository;
            _fileUploadsRepository = fileUploadsRepository;
            _chapterLinkRepository = chapterLinkRepository;
            _categoryRepository = categoryRepository;
            _ckeUploadRepository = ckeUploadRepository;
            _testVersionRepository = testVersionRepository;
            _dispatcher = dispatcher;
            _executor = executor;
        }
        private void SyncLabels(SaveTrainingGuideCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.TrainingGuide.TrainingLabels))
                return;
            var allExsistingLabels = _executor.Execute<TrainingLabelListQuery, IEnumerable<TrainingLabel>>(new TrainingLabelListQuery { Names = command.TrainingGuide.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries) });
            foreach (var label in command.TrainingGuide.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!allExsistingLabels.Any(x => x.Name == label))
                {
                    _dispatcher.Dispatch(new CreateOrUpdateTrainingLableCommand { Name = label, Description = label });
                }
            }
        }
        public CommandResponse Execute(SaveTrainingGuideCommand command)
        {
            if (command.TrainingGuide != null)
            {
                SyncLabels(command);
                if (command.TrainingGuide.TrainingGuidId.Equals(Guid.Empty) || command.NewTrainingGuideId.HasValue)
                {
                    var id = command.NewTrainingGuideId.HasValue ? command.NewTrainingGuideId.Value : Guid.NewGuid();

                    #region Create

                    var trainingGuide = new TrainingGuide
                    {
                        Id = id,
                        CreatedBy = command.CurrentlyLoggedInUserId,
                        CreatedOn = DateTime.Now,
                        Description = command.TrainingGuide.Description,
                        IsActive = false,
                        Title = command.TrainingGuide.Title,
                        PlaybookPreviewMode = command.TrainingGuide.PlaybookPreviewMode,
                        Printable = command.TrainingGuide.Printable
                    };
                    trainingGuide.ReferenceId = LastReferenceIdForGuide.GetGuideReference(new QueryExecutor().Execute<LastReferenceIdFromGuideQueryParameter, string>(new LastReferenceIdFromGuideQueryParameter()));
                    if (command.Collaborators != null)
                    {
                        command.Collaborators.ForEach(c => trainingGuide.Collaborators.Add(_standardUserRepository.Find(c)));
                    }
                    if (!trainingGuide.Collaborators.Any() || (trainingGuide.Collaborators.Any() && !trainingGuide.Collaborators.Any(c => c.Id.Equals(command.CurrentlyLoggedInUserId))))
                        trainingGuide.Collaborators.Add(_standardUserRepository.Find(command.CurrentlyLoggedInUserId));
                    if (command.TrainingGuide.CoverPictureVM != null)
                    {
                        trainingGuide.CoverPicture = _fileUploadsRepository.Find(command.TrainingGuide.CoverPictureVM.Id);
                    }
                    _trainingGuideRepository.Add(trainingGuide);
                    var testRef = new TestVersion { Id = Guid.NewGuid(), TrainingGuide = trainingGuide };
                    _testVersionRepository.Add(testRef);
                    trainingGuide.TestVersion = testRef;
                    trainingGuide.TrainingLabels = _executor.Execute<TrainingLabelListQuery, IEnumerable<TrainingLabel>>(new TrainingLabelListQuery { Names = string.IsNullOrWhiteSpace(command.TrainingGuide.TrainingLabels) ? new string[0] : command.TrainingGuide.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries) }).ToList();
                    _trainingGuideRepository.SaveChanges();

                    var index = 1;
                    foreach (var chapter in command.TrainingGuide.TraningGuideChapters)
                    {
                        var trainingGuideChapter = new TraningGuideChapter
                        {
                            ChapterContent = chapter.ChapterContent,
                            ChapterName = chapter.ChapterName,
                            ChapterNumber = index++,
                            LinkedTrainingGuide = chapter.SelectedTraningGuideId.HasValue ? _trainingGuideRepository.Find(chapter.SelectedTraningGuideId) : null,
                            TrainingGuide = trainingGuide,
                            Id = chapter.TraningGuideChapterId != Guid.Empty ? chapter.TraningGuideChapterId : Guid.NewGuid()
                        };
                        _trainingGuideChapterRepository.Add(trainingGuideChapter);
                        var uploadIndex = 1;
                        foreach (var attachment in chapter.Attachments)
                        {
                            if (attachment.Embeded)
                            {
                                var chapterLink = new ChapterLink
                                {
                                    Id = Guid.NewGuid(),
                                    TraningGuideChapter = trainingGuideChapter,
                                    ChapterUploadSequence = uploadIndex++,
                                    Url = attachment.Url,
                                    Type = attachment.Type.Equals("Youtube") ? ChapterLinkType.Youtube : ChapterLinkType.Vimeo
                                };
                                _chapterLinkRepository.Add(chapterLink);
                            }
                            else
                            {
                                var chapterUpload = new ChapterUpload
                                {
                                    ChapterUploadSequence = uploadIndex++,
                                    TraningGuideChapter = trainingGuideChapter,
                                    Id = Guid.NewGuid(),
                                    Upload = _fileUploadsRepository.Find(attachment.Id),
                                    Content = new QueryExecutor().Execute<GetUploadContentQueryParameter, string>(new GetUploadContentQueryParameter { Id = attachment.Id })
                                };
                                _chapterUploadRepository.Add(chapterUpload);
                                if(!string.IsNullOrWhiteSpace(attachment.Description) &&  chapter.Upload != null && attachment.Description != chapterUpload.Upload.Description)
                                {
                                    var u = _fileUploadsRepository.Find(attachment.Id);
                                    u.Description = attachment.Description;
                                    _fileUploadsRepository.SaveChanges();
                                }
                            }
                        }
                        var viewContentToolsUploads = chapter.ContentToolsUploads.Select(delegate (UploadFromContentToolsResultModel x)
                        {
                            var sId = x.url.Replace(x.url.Substring(0, x.url.IndexOf("/GetThumbnail/") + 13), string.Empty).Substring(1);
                            if (sId.IndexOf('?') > -1)
                                sId = sId.Substring(0, sId.IndexOf('?'));
                            Guid gId;
                            if (!Guid.TryParse(sId, out gId))
                                return Guid.Empty;
                            chapter.CKEUploads.Add(new CKEditorUploadResultViewModel { Id = gId });
                            return gId;
                        }).ToList(); 
                        foreach (var ckeUpload in chapter.CKEUploads)
                        {
                            var ckeUploadE = new CKEUpload
                            {
                                TrainingGuideChapter = trainingGuideChapter,
                                Id = Guid.NewGuid(),
                                Upload = _fileUploadsRepository.Find(ckeUpload.Id)
                            };
                            _ckeUploadRepository.Add(ckeUploadE);
                        }
                        _trainingGuideChapterRepository.SaveChanges();
                    }
                    _trainingGuideRepository.SaveChanges();
                    if (!command.TrainingGuide.SelectedCategoryId.Equals(Guid.Empty))
                    {
                        var category = _categoryRepository.Find(command.TrainingGuide.SelectedCategoryId);
                        if (category != null)
                        {
                            if(category.TrainingGuides == null)
                            {
                                category.TrainingGuides = new List<TrainingGuide>();
                            }
                            category.TrainingGuides.Add(trainingGuide);
                        }
                    }
                    _categoryRepository.SaveChanges();

                    #endregion Create
                }
                else
                {
                    #region Update

                    var vm = command.TrainingGuide;
                    var t = _trainingGuideRepository.Find(command.TrainingGuide.TrainingGuidId);
                    t.Title = vm.Title;
                    t.Description = vm.Description;
                    t.LastEditDate = DateTime.Now;
                    t.TrainingLabels.Clear();
                    t.TrainingLabels = _executor.Execute<TrainingLabelListQuery, IEnumerable<TrainingLabel>>(new TrainingLabelListQuery { Names = string.IsNullOrWhiteSpace(command.TrainingGuide.TrainingLabels) ? new string[0] : command.TrainingGuide.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries) }).ToList();
                    t.PlaybookPreviewMode = vm.PlaybookPreviewMode;
                    t.Printable = vm.Printable;
                    if (!vm.SelectedCategoryId.Equals(Guid.Empty))
                    {
                        if (!t.Categories.Any(c => c.Id.Equals(vm.SelectedCategoryId)))
                        {
                            var oldCategory = _categoryRepository.Find(t.Categories.First().Id);
                            oldCategory.TrainingGuides.Remove(t);
                            var newCategory = _categoryRepository.Find(vm.SelectedCategoryId);
                            if (newCategory != null)
                            {
                                newCategory.TrainingGuides.Add(t);
                            }
                        }
                    }
                    if (vm.CoverPictureVM != null)
                    {
                        if(t.CoverPicture != null)
                        {
                            if (!vm.CoverPictureVM.Id.Equals(t.CoverPicture.Id))
                            {
                                new CommandDispatcher().Dispatch(new DeleteUploadCommand
                                {
                                    Id = t.CoverPicture.Id.ToString()
                                });
                            }
                        }
                        t.CoverPicture = _fileUploadsRepository.Find(vm.CoverPictureVM.Id);
                    }
                    else if (vm.CoverPictureVM == null && t.CoverPicture != null)
                    {
                        new CommandDispatcher().Dispatch(new DeleteUploadCommand
                        {
                            Id = t.CoverPicture.Id.ToString()
                        });
                        t.CoverPicture = null;
                    }
                    
                    var oldChapters = t.ChapterList.Select(m => m.Id).ToList();
                    var allChapters = vm.TraningGuideChapters.Select(m => m.TraningGuideChapterId).ToList();
                    var newChapters = vm.TraningGuideChapters.Where(m => m.New).Select(x => x.TraningGuideChapterId).ToList();
                    var missingChapters = oldChapters.Where(x => !allChapters.Contains(x) && !newChapters.Contains(x)).ToList();
                    var updateableChapters = allChapters.Where(x => !newChapters.Contains(x) && !missingChapters.Contains(x)).ToList();
                    foreach (var chapter in t.ChapterList.Where(x => missingChapters.Contains(x.Id)).ToList())
                    {
                        _trainingGuideChapterRepository.Delete(chapter);
                    }
                    foreach (var chapter in vm.TraningGuideChapters.Where(x => updateableChapters.Contains(x.TraningGuideChapterId)).ToList())
                    {
                        #region update chapter

                        //update old chapters
                        var c = t.ChapterList.FirstOrDefault(s => s.Id.Equals(chapter.TraningGuideChapterId));
                        c.ChapterContent = chapter.ChapterContent;
                        c.ChapterName = chapter.ChapterName;
                        c.ChapterNumber = vm.TraningGuideChapters.IndexOf(chapter);
                        if (c.LinkedTrainingGuide != null)
                        {
                            if (chapter.SelectedTraningGuideId.HasValue)
                            {
                                if (!c.LinkedTrainingGuide.Id.Equals(chapter.SelectedTraningGuideId))
                                {
                                    c.LinkedTrainingGuide = _trainingGuideRepository.Find(chapter.SelectedTraningGuideId);
                                }
                            }
                            else
                            {
                                c.LinkedTrainingGuide = null;
                            }
                        }
                        var oldChapterUploadIds = c.ChapterUploads.Where(x => x.Upload != null).Select(m => m.Upload.Id).ToList();
                        var newChapterUploadIds = chapter.Attachments.Where(x => !x.Embeded && !x.InProcess).Select(m => m.Id).ToList();
                        var missingAttachments = oldChapterUploadIds.Where(x => !newChapterUploadIds.Contains(x)).ToList();
                        var addedAttachments = newChapterUploadIds.Where(x => !oldChapterUploadIds.Contains(x)).ToList();
                        foreach (var upload in c.ChapterUploads.Where(x => (x.Upload != null && missingAttachments.Contains(x.Upload.Id)) || x.Upload == null).ToList())
                        {
                            //delete chapterUploadWrapper of deleted upload
                            _chapterUploadRepository.Delete(upload);
                        }
                        foreach (var upload in chapter.Attachments.Where(x => addedAttachments.Contains(x.Id)).ToList())
                        {
                            //add new chapterUploadWrapper
                            var chapterUpload = new ChapterUpload
                            {
                                ChapterUploadSequence = chapter.Attachments.IndexOf(upload),
                                TraningGuideChapter = c,
                                Id = Guid.NewGuid(),
                                Upload = _fileUploadsRepository.Find(upload.Id),
                                Content = new QueryExecutor().Execute<GetUploadContentQueryParameter, string>(new GetUploadContentQueryParameter { Id = upload.Id })
                            };
                            _chapterUploadRepository.Add(chapterUpload);
                        }
                        var oldChapterLinks = c.ChapterLinks.Select(m => m.Url).ToList();
                        var newChapterLinks = chapter.Attachments.Where(x => x.Embeded).Select(m => m.Url).ToList();
                        var missingLinks = oldChapterLinks.Where(x => !newChapterLinks.Contains(x)).ToList();
                        var addedLinks = newChapterLinks.Where(x => !oldChapterLinks.Contains(x)).ToList();
                        foreach (var link in c.ChapterLinks.Where(x => missingLinks.Contains(x.Url)).ToList())
                        {
                            _chapterLinkRepository.Delete(link);
                        }
                        foreach (var link in chapter.Attachments.Where(x => addedLinks.Contains(x.Url)).ToList())
                        {
                            var chapterLink = new ChapterLink
                            {
                                Id = Guid.NewGuid(),
                                TraningGuideChapter = c,
                                ChapterUploadSequence = chapter.Attachments.IndexOf(link) + 1,
                                Url = link.Url,
                                Type = link.Type.Equals("Youtube") ? ChapterLinkType.Youtube : ChapterLinkType.Vimeo
                            };
                            _chapterLinkRepository.Add(chapterLink);
                        }
                        var oldCKEUploads = c.CKEUploads.Where(x => x.Upload!=null).Select(x => x.Upload.Id).ToList();
                        var viewCKEUploads = chapter.CKEUploads.Select(x => x.Id).ToList();
                        var viewContentToolsUploads = chapter.ContentToolsUploads.Select(delegate (UploadFromContentToolsResultModel x)
                        {
                            var id =  x.url.Replace(x.url.Substring(0,x.url.IndexOf("/GetThumbnail/") + 13), string.Empty).Substring(1);
                            if (id.IndexOf('?') > -1)
                                id = id.Substring(0, id.IndexOf('?'));
                            Guid gId;
                            if (!Guid.TryParse(id, out gId))
                                return Guid.Empty;
                            chapter.CKEUploads.Add(new CKEditorUploadResultViewModel { Id = gId });
                            return gId;
                        }).ToList();
                        viewCKEUploads.AddRange(viewContentToolsUploads);
                        var addedCKEUploads = viewCKEUploads.Where(x => !oldCKEUploads.Contains(x)).ToList();
                        var missingCKEUploads = oldCKEUploads.Where(x => !viewCKEUploads.Contains(x)).ToList();
                        foreach(var ckeUpload in c.CKEUploads.Where(x => missingAttachments.Contains(x.Id)).ToList())
                        {
                            _ckeUploadRepository.Delete(ckeUpload);
                        }
                        foreach (var ckeUpload in chapter.CKEUploads.Where(x=> addedCKEUploads.Contains(x.Id)).ToList())
                        {
                            var ckeUploadE = new CKEUpload
                            {
                                TrainingGuideChapter = c,
                                Id = Guid.NewGuid(),
                                Upload = _fileUploadsRepository.Find(ckeUpload.Id)
                            };
                            _ckeUploadRepository.Add(ckeUploadE);
                        }
                        //arrangeFiles
                        foreach (var upload in chapter.Attachments.Where(x => !x.InProcess).ToList())
                        {
                            if (!upload.Embeded)
                            {
                                var cu = c.ChapterUploads.FirstOrDefault(q => q.Upload.Id.Equals(upload.Id));
                                cu.Upload.Description = upload.Description;
                                cu.ChapterUploadSequence = chapter.Attachments.IndexOf(upload);
                                _chapterUploadRepository.SaveChanges();
                            }
                            else
                            {
                                var l = c.ChapterLinks.FirstOrDefault(q => q.Url.Equals(upload.Url));
                                l.ChapterUploadSequence = chapter.Attachments.IndexOf(upload);
                                _chapterLinkRepository.SaveChanges();
                            }
                        }
                        _chapterLinkRepository.SaveChanges();
                        _chapterUploadRepository.SaveChanges();

                        #endregion update chapter
                    }
                    foreach (var chapter in vm.TraningGuideChapters.Where(x => newChapters.Contains(x.TraningGuideChapterId)).ToList())
                    {
                        //add new chapters

                        #region add chapter

                        var trainingGuideChapter = new TraningGuideChapter
                        {
                            ChapterContent = chapter.ChapterContent,
                            ChapterName = chapter.ChapterName,
                            ChapterNumber = vm.TraningGuideChapters.IndexOf(chapter),
                            LinkedTrainingGuide = chapter.SelectedTraningGuideId.HasValue ? _trainingGuideRepository.Find(chapter.SelectedTraningGuideId) : null,
                            TrainingGuide = t,
                            Id = chapter.TraningGuideChapterId != Guid.Empty ? chapter.TraningGuideChapterId :  Guid.NewGuid()
                        };
                        _trainingGuideChapterRepository.Add(trainingGuideChapter);
                        var uploadIndex = 0;
                        foreach (var attachment in chapter.Attachments)
                        {
                            if (attachment.Embeded)
                            {
                                var chapterLink = new ChapterLink
                                {
                                    Id = Guid.NewGuid(),
                                    TraningGuideChapter = trainingGuideChapter,
                                    ChapterUploadSequence = uploadIndex++,
                                    Url = attachment.Url,
                                    Type = attachment.Type.Equals("Youtube") ? ChapterLinkType.Youtube : ChapterLinkType.Vimeo
                                };
                                _chapterLinkRepository.Add(chapterLink);
                            }
                            else
                            {
                                var chapterUpload = new ChapterUpload
                                {
                                    ChapterUploadSequence = uploadIndex++,
                                    TraningGuideChapter = trainingGuideChapter,
                                    Id = Guid.NewGuid(),
                                    Upload = _fileUploadsRepository.Find(attachment.Id),
                                    Content = new QueryExecutor().Execute<GetUploadContentQueryParameter, string>(new GetUploadContentQueryParameter { Id = attachment.Id })
                                };
                                _chapterUploadRepository.Add(chapterUpload);
                            }
                        }
                        var viewContentToolsUploads = chapter.ContentToolsUploads.Select(delegate (UploadFromContentToolsResultModel x)
                        {
                            var id = x.url.Replace(x.url.Substring(0, x.url.IndexOf("/GetThumbnail/") + 13), string.Empty).Substring(1);
                            if (id.IndexOf('?') > -1)
                                id = id.Substring(0, id.IndexOf('?'));
                            Guid gId;
                            if (!Guid.TryParse(id, out gId))
                                return Guid.Empty;
                            chapter.CKEUploads.Add(new CKEditorUploadResultViewModel { Id = gId });
                            return gId;
                        }).ToList();
                        foreach (var ckeUpload in chapter.CKEUploads)
                        {
                            var ckeUploadE = new CKEUpload
                            {
                                TrainingGuideChapter = trainingGuideChapter,
                                Id = Guid.NewGuid(),
                                Upload = _fileUploadsRepository.Find(ckeUpload.Id)
                            };
                            _ckeUploadRepository.Add(ckeUploadE);
                        }
                        _trainingGuideChapterRepository.SaveChanges();

                        #endregion add chapter
                    }
                    _trainingGuideRepository.SaveChanges();

                    var index = 0;
                    foreach (var c in t.ChapterList.OrderBy(g => g.ChapterNumber).ToList())
                    {
                        c.ChapterNumber = index++;
                    }
                    _trainingGuideRepository.SaveChanges();
                    _trainingGuideChapterRepository.SaveChanges();

                    #endregion Update
                }
            }
            return null;
        }
    }
}