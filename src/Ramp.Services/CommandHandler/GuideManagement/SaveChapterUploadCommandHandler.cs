using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using IKVM;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.GuideManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TikaOnDotNet;
namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class SaveChapterUploadCommandHandler : CommandHandlerBase<SaveChapterUploadCommandParameter>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public SaveChapterUploadCommandHandler(IRepository<TrainingGuide> trainingGuideRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
        }

        public override CommandResponse Execute(SaveChapterUploadCommandParameter command)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                var trainingGuideChapter = _trainingGuideRepository.List.SelectMany(a => a.ChapterList.Where(w => w.Id == command.TrainingGuideChapterId)).FirstOrDefault();

                if (trainingGuideChapter != null)
                {
                    var chapterUpload = new ChapterUpload
                    {
                        Id = command.Id,
                        DocumentFileContent = command.DocumentFileContent,
                        DocumentName = command.DocumentName,
                        DocumentType = Helpers.CommonHelper.GetDocumentType(command.DocumentName),
                        ChapterUploadSequence = command.UploadSequence,
                        Content = ExtractText(command.DocumentFileContent)
                    };

                    trainingGuideChapter.ChapterUploads.Add(chapterUpload);
                }

                _trainingGuideRepository.SaveChanges();

                scope.Complete();
            }
            return null;
        }

        private string ExtractText(byte[] dataBytes)
        {
            //hack to load the correct DLLs
            var t = typeof(com.sun.codemodel.@internal.ClassType); // IKVM.OpenJDK.Tools
            t = typeof(com.sun.org.apache.xalan.@internal.xsltc.trax.TransformerFactoryImpl); // IKVM.OpenJDK.XML.Transform
            t = typeof(com.sun.org.glassfish.external.amx.AMX); // IKVM.OpenJDK.XML.WebServices
            TextExtractor extractor = new TextExtractor();
            TextExtractionResult result = extractor.Extract(dataBytes);

            return result.Text;
        }
    }
}