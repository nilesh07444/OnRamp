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
using System.IO;
using System.Linq;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class EditTrainingGuideQueryHandler :
        QueryHandlerBase<EditTrainingGuideQueryParameter, TrainingGuideViewModelLong>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public EditTrainingGuideQueryHandler(IRepository<Company> companyRepository, IRepository<TrainingGuide> trainingGuideRepository)
        {
            _companyRepository = companyRepository;
            _trainingGuideRepository = trainingGuideRepository;
        }

        public override TrainingGuideViewModelLong ExecuteQuery(EditTrainingGuideQueryParameter queryParameters)
        {
            //var list = new List<string, int>();

            var trainingGuideViewModelLong = new TrainingGuideViewModelLong();
            TrainingGuide trainingGuide = _trainingGuideRepository.Find(queryParameters.TrainigGuideId);

            if (trainingGuide != null)
            {
                var selectedCategoryIdList = new List<Guid>();
                //    selectedCategoryIdList.Add(trainingGuide.Category.Id);

                //var categoryEdit = trainingGuide.Category;

                string catName = "";
                Guid selectedCatId = Guid.Empty;
                //if (categoryEdit != null)
                //{
                //    catName = categoryEdit.CategoryTitle;
                //    selectedCatId = categoryEdit.Id;
                //}

                var trainingGuideModel = new TrainingGuideViewModel
                {
                    TrainingGuidId = trainingGuide.Id,
                    Title = trainingGuide.Title,
                    Description = trainingGuide.Description,
                    SelectedCategories = selectedCategoryIdList,
                    ReferenceId = trainingGuide.ReferenceId,
                    IsActive = trainingGuide.IsActive,
                    CreatedOn = trainingGuide.CreatedOn,
                    CategoryName = catName,
                    SelectedCategoryId = selectedCatId
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
                    };

                 

                    foreach (var link in traningGuideChapter.ChapterLinks.Where(c => c.Type == ChapterLinkType.Youtube))
                    {
                        if (link.Url.Contains("embed"))
                        {
                            traningGuideChapterViewModel.YouTubeUrl.Add(link.Url);
                            DocumentSequenceViewModel sequence = new DocumentSequenceViewModel();
                            sequence.DocumentName = link.Url;
                            sequence.sequenceId = link.ChapterUploadSequence;
                            sequence.Url = link.Url;
                            sequence.DocumentType = "YouTube";
                            sequence.ChapterUploadId = link.Id;
                            traningGuideChapterViewModel.DocumentSequenceList.Add(sequence);
                        }
                        else
                        {
                            string[] array = link.Url.Split('=');
                            if (array != null)
                            {
                                traningGuideChapterViewModel.YouTubeUrl.Add("https://www.youtube.com/embed/" + array[1]);
                            }
                        }
                    }

                    foreach (var link in traningGuideChapter.ChapterLinks.Where(c => c.Type == ChapterLinkType.Vimeo))
                    {
                        if (link.Url.Contains("player.vimeo.com"))
                        {
                            traningGuideChapterViewModel.VimeoUrl.Add(link.Url);

                            DocumentSequenceViewModel sequence = new DocumentSequenceViewModel();
                            sequence.DocumentName = link.Url;
                            sequence.sequenceId = link.ChapterUploadSequence;
                            sequence.Url = link.Url;
                            sequence.DocumentType = "Vimeo";
                            sequence.ChapterUploadId = link.Id;
                            traningGuideChapterViewModel.DocumentSequenceList.Add(sequence);
                        }
                        else
                        {
                            var code = link.Url.Substring(link.Url.LastIndexOf('/') + 1);
                            traningGuideChapterViewModel.VimeoUrl.Add("//player.vimeo.com/video/" + code);
                        }
                    }

                    string chapterNumberText;
                    NumberToTextConverterHelper.ConvertNumberToText(traningGuideChapter.ChapterNumber, out chapterNumberText);

                    traningGuideChapterViewModel.ChapterNumberText = chapterNumberText.Trim();
                    //.OrderBy(em => em.ChapterUploadSequence)

                    //foreach (ChapterUpload chapterUpload in traningGuideChapter.ChapterUploads)
                    foreach (ChapterUpload chapterUpload in traningGuideChapter.ChapterUploads.OrderBy(em => em.ChapterUploadSequence))
                    {
                        var chapterUploadViewModel = new ChapterUploadViewModel
                        {
                            ChapterUploadId = chapterUpload.Id,
                            DocumentType = chapterUpload.DocumentType,
                            DocumentName = chapterUpload.DocumentName,
                            ChapterUploadSequence = chapterUpload.ChapterUploadSequence
                        };

                        if (chapterUpload.DocumentType == TrainingDocumentTypeEnum.Excel)
                        {
                            DocumentSequenceViewModel sequence = new DocumentSequenceViewModel();
                            sequence.DocumentName = chapterUpload.DocumentName;
                            sequence.sequenceId = chapterUpload.ChapterUploadSequence;
                            sequence.Url = chapterUpload.DocumentName;
                            sequence.DocumentType = "Excel";
                            sequence.ChapterUploadId = chapterUpload.Id;
                            traningGuideChapterViewModel.DocumentSequenceList.Add(sequence);
                        }
                        if (chapterUpload.DocumentType == TrainingDocumentTypeEnum.Video)
                        {
                            DocumentSequenceViewModel sequence = new DocumentSequenceViewModel();
                            sequence.DocumentName = chapterUpload.DocumentName;
                            sequence.sequenceId = chapterUpload.ChapterUploadSequence;
                            sequence.Url = chapterUpload.DocumentName;
                            sequence.DocumentType = "Video";
                            sequence.ChapterUploadId = chapterUpload.Id;
                            traningGuideChapterViewModel.DocumentSequenceList.Add(sequence);
                        }
                        if (chapterUpload.DocumentType == TrainingDocumentTypeEnum.PowerPoint)
                        {
                            DocumentSequenceViewModel sequence = new DocumentSequenceViewModel();
                            sequence.DocumentName = chapterUpload.DocumentName;
                            sequence.sequenceId = chapterUpload.ChapterUploadSequence;
                            sequence.Url = chapterUpload.DocumentName;
                            sequence.DocumentType = "PowerPoint";
                            sequence.ChapterUploadId = chapterUpload.Id;
                            traningGuideChapterViewModel.DocumentSequenceList.Add(sequence);
                        }
                        if (chapterUpload.DocumentType == TrainingDocumentTypeEnum.Pdf)
                        {
                            DocumentSequenceViewModel sequence = new DocumentSequenceViewModel();
                            sequence.DocumentName = chapterUpload.DocumentName;
                            sequence.sequenceId = chapterUpload.ChapterUploadSequence;
                            sequence.Url = chapterUpload.DocumentName;
                            sequence.DocumentType = "Pdf";
                            sequence.ChapterUploadId = chapterUpload.Id;
                            traningGuideChapterViewModel.DocumentSequenceList.Add(sequence);
                        }
                        if (chapterUpload.DocumentType == TrainingDocumentTypeEnum.WordDocument)
                        {
                            DocumentSequenceViewModel sequence = new DocumentSequenceViewModel();
                            sequence.DocumentName = chapterUpload.DocumentName;
                            sequence.sequenceId = chapterUpload.ChapterUploadSequence;
                            sequence.Url = chapterUpload.DocumentName;
                            sequence.DocumentType = "WordDocument";
                            sequence.ChapterUploadId = chapterUpload.Id;
                            traningGuideChapterViewModel.DocumentSequenceList.Add(sequence);
                        }

                        //Code for bind DocumentUrl
                        var company = _companyRepository.List.FirstOrDefault(
                                    com => com.Id == queryParameters.CompanyId);
                        if (company != null)
                        {
                            string trainingGuidePath = Path.Combine(company.CompanyName,
                            trainingGuide.Id.ToString());

                            chapterUploadViewModel.DocumentUrl = Path.Combine(trainingGuidePath,
                                chapterUpload.DocumentName);

                            var url = Path.Combine(trainingGuidePath,
                                chapterUpload.DocumentName);
                            if (chapterUpload.DocumentType == TrainingDocumentTypeEnum.Image)
                            {
                                DocumentSequenceViewModel sequence = new DocumentSequenceViewModel();
                                sequence.DocumentName = chapterUpload.DocumentName;
                                sequence.sequenceId = chapterUpload.ChapterUploadSequence;
                                sequence.Url = url;
                                sequence.DocumentType = "Image";
                                sequence.ChapterUploadId = chapterUpload.Id;
                                traningGuideChapterViewModel.DocumentSequenceList.Add(sequence);
                            }
                        }

                        traningGuideChapterViewModel.ChapterUpload.Add(chapterUploadViewModel);
                    }
                    tarningGuideChapterList.Add(traningGuideChapterViewModel);
                }
                foreach (var source in tarningGuideChapterList.OrderBy(m => m.ChapterNumber))
                {
                    trainingGuideModel.TraningGuideChapters.Add(source);
                }
                trainingGuideViewModelLong.TrainingGuide = trainingGuideModel;
            }
            return trainingGuideViewModelLong;
        }
    }
}