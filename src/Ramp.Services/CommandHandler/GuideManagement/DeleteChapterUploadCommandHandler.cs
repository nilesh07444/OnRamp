using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using Ramp.Contracts.CommandParameter.Login;
using System;
using System.IO;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class DeleteChapterUploadCommandHandler : CommandHandlerBase<DeleteChapterUploadCommand>
    {
        private readonly IRepository<ChapterUpload> _chapterUploadRepository;

        public DeleteChapterUploadCommandHandler(IRepository<ChapterUpload> chapterUploadRepository)
        {
            _chapterUploadRepository = chapterUploadRepository;
        }

        public override CommandResponse Execute(DeleteChapterUploadCommand command)
        {
            var chapterUploadModel = _chapterUploadRepository.Find(command.ChapterUploadId);
            if (chapterUploadModel != null)
            {
                var pathToDeleteUploadedFiles = command.PathToDeleteUploadedFiles;
                var companyDirectoryPath = Path.Combine(pathToDeleteUploadedFiles, command.CompanyName);
                var trainingGuidePath = Path.Combine(companyDirectoryPath, command.GuideTitle);
                var filePath = Path.Combine(trainingGuidePath, command.DocumentName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                _chapterUploadRepository.Delete(chapterUploadModel);
            }
            return null;
        }
    }
}