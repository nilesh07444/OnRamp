using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class MigrateTrainingGuidesToKnockoutCommandHandler : ICommandHandlerBase<MigrateTrainingGuidesToKnockoutCommand>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<FileUploads> _fileUploadsRepository;
        private readonly IRepository<ChapterUpload> _chapterUploadRepository;

        public MigrateTrainingGuidesToKnockoutCommandHandler(
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<FileUploads> fileUploadsRepository,
            IRepository<ChapterUpload> chapterUploadRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
            _fileUploadsRepository = fileUploadsRepository;
            _chapterUploadRepository = chapterUploadRepository;
        }

        public CommandResponse Execute(MigrateTrainingGuidesToKnockoutCommand command)
        {
            foreach (var guide in _trainingGuideRepository.List.ToList())
            {
                if (guide.CoverPicFileContent != null)
                {
                    var upload = new FileUploads
                    {
                        ContentType = "image/png",
                        Data = guide.CoverPicFileContent,
                        Id = Guid.NewGuid(),
                        Name = "CoverPicture.png",
                        Description = "CoverPicture",
                        Type = TrainingDocumentTypeEnum.Image.ToString()
                    };
                    _fileUploadsRepository.Add(upload);
                    var entity = _trainingGuideRepository.Find(guide.Id);
                    entity.CoverPicture = upload;
                    entity.CoverPicFileContent = null;
                    _trainingGuideRepository.SaveChanges();
                }
                foreach (var chapter in guide.ChapterList)
                {
                    if (chapter.ChapterUploads.Count > 0)
                    {
                        foreach (var chapterUpload in chapter.ChapterUploads)
                        {
                            if (chapterUpload.DocumentFileContent != null)
                            {
                                var upload = new FileUploads
                                {
                                    Type = chapterUpload.DocumentType.ToString(),
                                    Data = chapterUpload.DocumentFileContent,
                                    Id = Guid.NewGuid(),
                                    Name = chapterUpload.DocumentName,
                                    Description = chapterUpload.DocumentName,
                                    ContentType = GetMimeType(chapterUpload.DocumentName)
                                };
                                _fileUploadsRepository.Add(upload);
                                var entity = _chapterUploadRepository.Find(chapterUpload.Id);
                                entity.Upload = upload;
                                entity.Content = string.Empty;
                                entity.DocumentFileContent = null;
                                entity.DocumentName = string.Empty;
                                entity.DocumentType = TrainingDocumentTypeEnum.Other;
                                _chapterUploadRepository.SaveChanges();
                            }
                        }
                    }
                }
            }
            return null;
        }

        private string GetMimeType(string name)
        {
            #region MimeTypes

            var mimeMap = new Dictionary<string, string>()
            {
                {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
                {".pps", "application/vnd.ms-powerpoint"},
                {".pdf", "application/pdf"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".ppt", "application/vnd.ms-powerpoint"},
                {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
                {".doc", "application/msword"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".mp4", "video/mp4"},
                {".jpeg", "image/jpeg"},
                {".png", "image/png"},
                {".gif", "image/gif"},
                {".bmp", "image/bmp"},
                {".jpg", "image/jpeg"}
            };

            #endregion MimeTypes

            var extension = Path.GetExtension(name.ToLowerInvariant());
            if (mimeMap.ContainsKey(extension.ToLowerInvariant()))
                return mimeMap[extension.ToLowerInvariant()];
            return null;
        }
    }
}