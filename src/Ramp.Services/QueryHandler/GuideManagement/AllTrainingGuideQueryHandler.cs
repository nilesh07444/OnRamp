using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class AllTrainingGuideQueryHandler : QueryHandlerBase<AllTrainingGuideQueryParameter, List<TrainingGuideViewModel>>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public AllTrainingGuideQueryHandler(
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<TrainingTest> trainingTestRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
            _trainingTestRepository = trainingTestRepository;
        }

        public override List<TrainingGuideViewModel> ExecuteQuery(AllTrainingGuideQueryParameter queryParameters)
        {
            var trainingGuideViewModelList = new List<TrainingGuideViewModel>();
            var trainingGuideModelList = _trainingGuideRepository.List;

            if (queryParameters.OnlyActive)
                trainingGuideModelList = trainingGuideModelList.Where(c => c.IsActive);
            if (!queryParameters.CollaboratorId.Equals(Guid.Empty))
            {
                trainingGuideModelList =
                    trainingGuideModelList.Where(
                        t => t.Collaborators.Any(c => c.Id.Equals(queryParameters.CollaboratorId)));
            }
            foreach (TrainingGuide trainingGuide in trainingGuideModelList.ToList())
            {
                var testModel = trainingGuide.TestVersion?.CurrentVersion;

                var selectedCategoryIdList = new List<Guid>();
                foreach (var category in trainingGuide.Categories)
                {
                    selectedCategoryIdList.Add(category.Id);
                }

                var trainingGuideModel = new TrainingGuideViewModel
                {
                    TrainingGuidId = trainingGuide.Id,
                    Title = trainingGuide.Title,
                    IsActive = trainingGuide.IsActive,
                    Description = trainingGuide.Description,
                    SelectedCategories = selectedCategoryIdList,
                    ReferenceId = trainingGuide.ReferenceId,
                    CreatedOn = trainingGuide.CreatedOn,
                    LastEditDate = trainingGuide.LastEditDate,
                    Category =trainingGuide.Categories.Any()  ?  Project.CategoryViewModelFrom(trainingGuide.Categories.First()) : null
                };
                if (trainingGuide.Collaborators.Count > 0)
                {
                    trainingGuide.Collaborators.ForEach(
                        c => trainingGuideModel.Collaborators.Add(Project.UserViewModelFrom(c)));
                }
                if (testModel != null)
                {
                    trainingGuideModel.IsTestCreated = true;
                    trainingGuideModel.TrainingTestId = testModel.Id;
                }
                var tarningGuideChapterList = new List<TraningGuideChapterViewModel>();
                foreach (TraningGuideChapter traningGuideChapter in trainingGuide.ChapterList)
                {
                    var traningGuideChapterViewModel = new TraningGuideChapterViewModel
                    {
                        TraningGuideChapterId = traningGuideChapter.Id,
                        ChapterNumber = traningGuideChapter.ChapterNumber,
                        ChapterName = traningGuideChapter.ChapterName,
                        ChapterContent = traningGuideChapter.ChapterContent,
                        VimeoUrl = traningGuideChapter.ChapterLinks.Where(l => l.Type == ChapterLinkType.Vimeo).Select(x => x.Url).ToList(),
                        YouTubeUrl = traningGuideChapter.ChapterLinks.Where(l => l.Type == ChapterLinkType.Youtube).Select(x => x.Url).ToList(),
                    };
                    if (traningGuideChapterViewModel.YouTubeUrl != null)
                    {
                        for (int x = 0; x < traningGuideChapterViewModel.YouTubeUrl.Count; x++)
                        {
                            string thumbnailUrl = CommonHelper.GetYouTubeImage(traningGuideChapterViewModel.YouTubeUrl[x]);
                            //traningGuideChapterViewModel.YouTubeThumbnail.Add(thumbnailUrl);
                        }
                    }
                    string chapterNumberText;
                    NumberToTextConverterHelper.ConvertNumberToText(traningGuideChapter.ChapterNumber,
                        out chapterNumberText);
                    traningGuideChapterViewModel.ChapterNumberText = chapterNumberText.Trim();
                    foreach (ChapterUpload chapterUpload in traningGuideChapter.ChapterUploads)
                    {
                        var chapterUploadViewModel = new ChapterUploadViewModel
                        {
                            ChapterUploadId = chapterUpload.Id,
                            DocumentType = chapterUpload.DocumentType,
                            DocumentName = chapterUpload.DocumentName
                        };
                        traningGuideChapterViewModel.ChapterUpload.Add(chapterUploadViewModel);
                    }
                    tarningGuideChapterList.Add(traningGuideChapterViewModel);
                }
                foreach (TraningGuideChapterViewModel source in tarningGuideChapterList.OrderBy(m => m.ChapterNumber))
                {
                    trainingGuideModel.TraningGuideChapters.Add(source);
                }
                trainingGuideViewModelList.Add(trainingGuideModel);
            }
            return trainingGuideViewModelList;
        }
    }
}