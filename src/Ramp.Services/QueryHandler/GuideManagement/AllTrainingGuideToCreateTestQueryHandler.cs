using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class AllTrainingGuideToCreateTestQueryHandler :
        QueryHandlerBase<AllTrainingGuideToCreateTestQueryParameter, List<TrainingGuideViewModel>>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public AllTrainingGuideToCreateTestQueryHandler(IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<TrainingTest> trainingTestRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
            _trainingTestRepository = trainingTestRepository;
        }

        public override List<TrainingGuideViewModel> ExecuteQuery(
            AllTrainingGuideToCreateTestQueryParameter queryParameters)
        {
            var trainingGuideViewModelList = new List<TrainingGuideViewModel>();
            var alltests = _trainingTestRepository.List;

            IEnumerable<TrainingGuide> guidelist;
            TrainingGuide myGuide;
            if (queryParameters.ShowAllTrainingGuide)
            {
                //get all training guides
                guidelist = _trainingGuideRepository.List;
            }
            else if (queryParameters.ShowTrainingGuideForEdit)
            {
                //get only those training guides for which no tests have been created yet
                guidelist = _trainingGuideRepository.List.Where(u => u.TestVersion.Versions.Count == 0 || u.TestVersion.Versions.All(x => x.Deleted.HasValue && x.Deleted.Value));

                TrainingTest test = alltests.FirstOrDefault(u => u.Id == queryParameters.Id.Value);

                myGuide = test.TrainingGuide;

                var selectedCategoryIdList = new List<Guid>();
                foreach (var category in myGuide.Categories)
                {
                    selectedCategoryIdList.Add(category.Id);
                }
                var trainingGuideModel = new TrainingGuideViewModel
                {
                    TrainingGuidId = myGuide.Id,
                    Title = myGuide.Title,
                    IsActive = myGuide.IsActive,
                    Description = myGuide.Description,
                    SelectedCategories = selectedCategoryIdList,
                    ReferenceId = myGuide.ReferenceId,
                    CreatedOn = myGuide.CreatedOn
                };

                var tarningGuideChapterList = new List<TraningGuideChapterViewModel>();
                foreach (TraningGuideChapter traningGuideChapter in myGuide.ChapterList)
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
            else
            {
                //get only those training guides for which no tests have been created yet
                guidelist = _trainingGuideRepository.List.Where(u => u.TestVersion != null && (u.TestVersion.Versions.Count == 0 || u.TestVersion.Versions.All(x => x.Deleted.HasValue && x.Deleted.Value)));
            }
            if (!queryParameters.ColaboratorId.Equals(Guid.Empty))
                guidelist = guidelist.Where(g => g.Collaborators.Any(u => u.Id.Equals(queryParameters.ColaboratorId)));
            foreach (TrainingGuide trainingGuide in guidelist)
            {
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
                    CreatedOn = trainingGuide.CreatedOn
                };

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