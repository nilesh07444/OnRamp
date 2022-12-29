using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class GetChapterUploadQueryHandler : IQueryHandler<GetChapterUploadQueryParameter, ChapterUploadViewModel>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public GetChapterUploadQueryHandler(IRepository<TrainingGuide> trainingGuideRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
        }

        public ChapterUploadViewModel ExecuteQuery(GetChapterUploadQueryParameter query)
        {
            Expression<Func<ChapterUpload, ChapterUploadViewModel>> projection = chapterUpload => new ChapterUploadViewModel
            {
                ChapterUploadId = chapterUpload.Id,
                Content = query.IncludeContent ? chapterUpload.DocumentFileContent : null,
                DocumentName = chapterUpload.DocumentName,
                DocumentType = chapterUpload.DocumentType
            };

            var q = _trainingGuideRepository.List.AsQueryable();
            var q2 = q.SelectMany(c => c.ChapterList.AsQueryable());
            var q3 = q2.SelectMany(c => c.ChapterUploads.AsQueryable());
            return q3.Where(c => c.Id == query.Id).Select(projection).FirstOrDefault();
        }
    }
}