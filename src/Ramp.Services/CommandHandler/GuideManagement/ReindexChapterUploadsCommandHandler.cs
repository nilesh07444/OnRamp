using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using IKVM;
using Ramp.Contracts.CommandParameter.GuideManagement;
using Ramp.Services.CommandParameter.GuideManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TikaOnDotNet;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class ReindexChapterUploadsCommandHandler : CommandHandlerBase<ReindexChapterUploadsCommandParameter>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public ReindexChapterUploadsCommandHandler(IRepository<TrainingGuide> trainingGuideRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
        }

        public override CommandResponse Execute(ReindexChapterUploadsCommandParameter command)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 5, 0)))
            {
                var trainingGuideChapters = _trainingGuideRepository.List.SelectMany(a => a.ChapterList).ToList();

                foreach (var trainingGuideChapter in trainingGuideChapters)
                {
                    foreach (var upload in trainingGuideChapter.ChapterUploads.Where(w => w.Upload != null && w.Upload.Data != null))
                    {
                        upload.Content = ExtractText(upload.Upload.Data);
                    }
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