using Common;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Customer.Models.Feedback;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.Catagories;
using Ramp.Contracts.QueryParameter.Feedback;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class TrainingGuideQueryHandler :
        QueryHandlerBase<TrainingGuideQueryParameter, TrainingGuideViewModel>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;
        private readonly IRepository<Feedback> _feedbackRepository;

        public TrainingGuideQueryHandler(
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<Domain.Customer.Models.Categories> categoryRepository,
            IRepository<Feedback> feedbackRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
            _categoryRepository = categoryRepository;
            _feedbackRepository = feedbackRepository;
        }

        public override TrainingGuideViewModel ExecuteQuery(TrainingGuideQueryParameter queryParameters)
        {
            var trainingGuide = _trainingGuideRepository.Find(queryParameters.Id);
            if (trainingGuide == null)
                return null;
            else
            {
                var model = new TrainingGuideViewModel
                {
                    CreatedOn = trainingGuide.CreatedOn,
                    Description = trainingGuide.Description,
                    Title = trainingGuide.Title,
                    IsActive = trainingGuide.IsActive,
                    ReferenceId = trainingGuide.ReferenceId,
                    PlaybookPreviewMode = trainingGuide.PlaybookPreviewMode,
                    Printable = trainingGuide.Printable,
                    TrainingGuidId = trainingGuide.Id,
                    TrainingLabels = string.Join(",",trainingGuide.TrainingLabels.Select(x => x.Name))
                };
                foreach (var chapter in trainingGuide.ChapterList.OrderBy(c => c.ChapterNumber).ToList())
                {
                    var chapterModel = new TraningGuideChapterViewModel
                    {
                        ChapterContent = chapter.ChapterContent,
                        ChapterName = chapter.ChapterName,
                        ChapterNumber = chapter.ChapterNumber,
                        TraningGuideChapterId = chapter.Id
                    };
                    chapter.ChapterLinks.ForEach(l => chapterModel.Attachments.Add(new FileUploadResultViewModel
                    {
                        Id = l.Id,
                        Embeded = true,
                        Url = l.Url.Replace("watch?v=", "embed/"),
                        Number = l.ChapterUploadSequence,
                        Type = l.Type.ToString()
                    }));
                    chapter.ChapterUploads.Where(x => x.Upload != null).ToList().ForEach(u => chapterModel.Attachments.Add(new FileUploadResultViewModel
                    {
                        DeleteType = "DELETE",
                        Id = u.Upload.Id,
                        InProcess = false,
                        Name = u.Upload.Name,
                        Description = u.Upload.Description,
                        Number = u.ChapterUploadSequence,
                        Type = u.Upload.ContentType,
                        Size = u.Upload.Data.Length,
                        Progress = "100%",
                        Url = string.IsNullOrWhiteSpace(queryParameters.UploadUrlBase) ? string.Empty : queryParameters.UploadUrlBase.Replace(Guid.Empty.ToString(), $"{u.Upload.Id}"),
                        ThumbnailUrl = string.IsNullOrWhiteSpace(queryParameters.UploadThumbnailUrlBase) ? string.Empty : queryParameters.UploadThumbnailUrlBase.Replace(Guid.Empty.ToString(), $"{u.Upload.Id}"),
                        DeleteUrl = string.IsNullOrWhiteSpace(queryParameters.UploadDeleteUrlBase) ? string.Empty : queryParameters.UploadDeleteUrlBase.Replace(Guid.Empty.ToString(), $"{u.Upload.Id}"),
                        PreviewPath = string.IsNullOrWhiteSpace(queryParameters.UploadPreviewUrlBase) ? string.Empty : queryParameters.UploadPreviewUrlBase.Replace(Guid.Empty.ToString(), $"{ u.Upload.Id }")
                    }));
                    chapterModel.Attachments = chapterModel.Attachments.OrderBy(a => a.Number).ToList();
                    chapter.CKEUploads.Where(x => x.Upload != null).ToList().ForEach(x => chapterModel.CKEUploads.Add(new CKEditorUploadResultViewModel
                    {
                        Id = x.Upload.Id
                    }));
                   
                    model.TraningGuideChapters.Add(chapterModel);
                }
                if (trainingGuide.CoverPicture != null)
                {
                    model.CoverPictureVM = new FileUploadResultViewModel
                    {
                        DeleteType = "DELETE",
                        DeleteUrl = string.IsNullOrWhiteSpace(queryParameters.UploadDeleteUrlBase) ? string.Empty : queryParameters.UploadDeleteUrlBase.Replace(Guid.Empty.ToString(), $"{trainingGuide.CoverPicture.Id}"),
                        Id = trainingGuide.CoverPicture.Id,
                        InProcess = false,
                        Name = trainingGuide.CoverPicture.Name,
                        Description = trainingGuide.CoverPicture.Description,
                        Progress = "100%",
                        Size = trainingGuide.CoverPicture.Data.Length,
                        Type = trainingGuide.CoverPicture.ContentType,
                        Url = string.IsNullOrWhiteSpace(queryParameters.UploadUrlBase) ? string.Empty : queryParameters.UploadUrlBase.Replace(Guid.Empty.ToString(), $"{trainingGuide.CoverPicture.Id}"),
                        ThumbnailUrl = string.IsNullOrWhiteSpace(queryParameters.CoverPictureThumbnailUrlBase) ? string.Empty : queryParameters.CoverPictureThumbnailUrlBase.Replace(Guid.Empty.ToString(), $"{trainingGuide.CoverPicture.Id}")
                    };
                }
                trainingGuide.Collaborators.ForEach(c => model.Collaborators.Add(Project.UserViewModelFrom(c)));
                //if (trainingGuide.Category != null)
                //{
                //    model.SelectedCategoryId = trainingGuide.Category.Id;
                //    model.CategoryName = trainingGuide.Category.CategoryTitle;
                //}
                var allCategories = new QueryExecutor().Execute<AllCategoriesQueryParameter, List<CategoryViewModel>>(new AllCategoriesQueryParameter
                {
                    CompanyId = queryParameters.CompanyId
                });
                allCategories.ForEach(c => model.TrainingGuideCategoryDropDown.Add(new JSTreeViewModel
                {
                    text = c.CategorieTitle.Length > 30 ? c.CategorieTitle.Substring(0, 29) : c.CategorieTitle,
                    id = c.Id.ToString(),
                    parent = c.ParentCategoryId.HasValue ? c.ParentCategoryId.Value.ToString() : "#"
                }));
                var resultAll = new QueryExecutor().Execute<AllTrainingGuideQueryParameter, List<TrainingGuideViewModel>>(new AllTrainingGuideQueryParameter());
                model.TraningGuideDropDownForLinking = resultAll.Select(c => new SerializableSelectListItem
                {
                    Text = c.Title,
                    Value = c.TrainingGuidId.ToString()
                });

                var feedbacks = new QueryExecutor().Execute<GetFeedbackForPlaybookQueryParameter, List<FeedbackViewModel>>(new GetFeedbackForPlaybookQueryParameter()
                {
                    ReferenceId = trainingGuide.ReferenceId
                });

                model.Feedback = feedbacks;

                return model;
            }
        }
    }
}