using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class MigrateTestsToKnockoutCommandHandler : ICommandHandlerBase<MigrateTestsToKnockoutCommand>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<FileUploads> _uploadRepository;

        public MigrateTestsToKnockoutCommandHandler(
            IRepository<TrainingTest> trainingTestRepository,
            IRepository<FileUploads> uploadRepository)
        {
            _trainingTestRepository = trainingTestRepository;
            _uploadRepository = uploadRepository;
        }

        public CommandResponse Execute(MigrateTestsToKnockoutCommand command)
        {
            foreach (var test in _trainingTestRepository.List.ToList())
            {
                foreach (var question in test.QuestionList)
                {
                    if (question.Image != null)
                    {
                        var upload = new FileUploads
                        {
                            ContentType = GetMimeType(question.Image.DocumentName),
                            Data = question.Image.DocumentFileContent,
                            Id = Guid.NewGuid(),
                            Name = question.Image.DocumentName,
                            Description = question.Image.DocumentName,
                            Type = question.Image.DocumentType.ToString()
                        };
                        _uploadRepository.Add(upload);
                        question.Image.DocumentFileContent = null;
                        question.Image.DocumentName = string.Empty;
                        question.Image.DocumentType = TrainingDocumentTypeEnum.Other;
                        question.Image.Upload = upload;
                        _trainingTestRepository.SaveChanges();
                    }
                    if (question.Video != null)
                    {
                        var upload = new FileUploads
                        {
                            ContentType = GetMimeType(question.Video.DocumentName),
                            Data = question.Video.DocumentFileContent,
                            Id = Guid.NewGuid(),
                            Name = question.Video.DocumentName,
                            Description = question.Video.DocumentName,
                            Type = question.Video.DocumentType.ToString()
                        };
                        _uploadRepository.Add(upload);
                        question.Video.DocumentFileContent = null;
                        question.Video.DocumentName = string.Empty;
                        question.Video.DocumentType = TrainingDocumentTypeEnum.Other;
                        question.Video.Upload = upload;
                        _trainingTestRepository.SaveChanges();
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