using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using Ramp.Contracts.CommandParameter.Login;
using System.IO;
using System.Linq;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class DeleteChapterCommandHandler : CommandHandlerBase<DeleteChapterCommand>
    {
        private readonly IRepository<TraningGuideChapter> _trainingGuideChapterRepository;

        public DeleteChapterCommandHandler(IRepository<TraningGuideChapter> trainingGuideChapterRepository)
        {
            _trainingGuideChapterRepository = trainingGuideChapterRepository;
        }

        public override CommandResponse Execute(DeleteChapterCommand command)
        {
            var trainingGuide = _trainingGuideChapterRepository.List.AsQueryable().First(f => f.Id == command.TraningGuideChapterId);

            if (trainingGuide != null)
            {
                var trainingGuideChapterList =
                    _trainingGuideChapterRepository.List.Where(c => c.TraningGuidId.Equals(trainingGuide.TraningGuidId)).ToList();

                if (trainingGuideChapterList.Count > 2)
                {
                    foreach (var chapterUpload in trainingGuide.ChapterUploads)
                    {
                        string pathToDeleteUploadedFiles = command.PathToDeleteUploadedFiles;
                        string companyDirectoryPath = Path.Combine(pathToDeleteUploadedFiles, command.CompanyName);
                        string trainingGuidePath = Path.Combine(companyDirectoryPath, command.GuideName);
                        string filePath = Path.Combine(trainingGuidePath, chapterUpload.DocumentName);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                    _trainingGuideChapterRepository.Delete(trainingGuide);
                    _trainingGuideChapterRepository.SaveChanges();
                }
            }
            return null;
        }
    }
}