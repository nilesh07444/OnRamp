using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class AllTrainingGuideForMasterAdminQueryHandler : QueryHandlerBase<AllTrainingGuideForMasterAdminQueryParameter, List<TrainingGuideViewModel>>,
                                                              IQueryHandler<AllTrainingGuideForMasterAdminQueryParameter,IEnumerable<TrainingGuideListModel>>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public AllTrainingGuideForMasterAdminQueryHandler(IRepository<TrainingGuide> trainingGuideRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
        }

        public override List<TrainingGuideViewModel> ExecuteQuery(
            AllTrainingGuideForMasterAdminQueryParameter queryParameters)
        {
            var trainingGuideViewModelList = new List<TrainingGuideViewModel>();

            var trainingGuideModelList = _trainingGuideRepository.GetAll();

            foreach (TrainingGuide trainingGuide in trainingGuideModelList)
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

        IEnumerable<TrainingGuideListModel> IQueryHandler<AllTrainingGuideForMasterAdminQueryParameter, IEnumerable<TrainingGuideListModel>>.ExecuteQuery(AllTrainingGuideForMasterAdminQueryParameter query)
        {
            return _trainingGuideRepository.List.AsQueryable().OrderBy(x => x.ReferenceId).ThenBy(x => x.Title).Select(Project.TrainingGuide_TrainingGuideListModel).ToList();
        }
    }
}
namespace Services
{
    public static partial class Project
    {
        public static readonly Expression<Func<TrainingGuide, TrainingGuideListModel>> TrainingGuide_TrainingGuideListModel =
            x => new TrainingGuideListModel
            {
                Assignable = x.IsActive,
                Description = x.Description,
                HasTest = x.TestVersion != null ? x.TestVersion.CurrentVersion != null : false,
                Id = x.Id,
                LastEditDate = x.LastEditDate,
                Published = x.IsActive,
                ReferenceId = x.ReferenceId,
                Title = x.Title
            };
    }
}