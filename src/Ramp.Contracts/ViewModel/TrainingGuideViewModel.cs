using Domain.Customer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Ramp.Contracts.ViewModel
{
    public class TrainingGuideViewModel : IViewModel
    {
        public TrainingGuideViewModel()
        {
            TraningGuideChapters = new List<TraningGuideChapterViewModel>();
            TraningGuideCategories = new List<CategoryViewModel>();
            SelectedCategories = new List<Guid>();
            Collaborators = new List<UserViewModel>();
            TrainingGuideCategoryDropDown = new List<JSTreeViewModel>();
        }

        public List<JSTreeViewModel> TrainingGuideCategoryDropDown { get; set; }
        public IEnumerable<SerializableSelectListItem> TraningGuideDropDownForLinking { get; set; }
        public IEnumerable<SerializableSelectListItem> CategoryDropDown { get; set; }
        public List<TraningGuideChapterViewModel> TraningGuideChapters { get; set; }
        public List<CategoryViewModel> TraningGuideCategories { get; set; }

        //[Required(ErrorMessage = "Please select Category/Categories")]
        public List<Guid> SelectedCategories { get; set; }

        public string ReferenceId { get; set; }

        public bool IsTestCreated { get; set; }

        public Guid TrainingTestId { get; set; }

        public Guid TrainingGuidId { get; set; }

        [Required(ErrorMessage = "Please enter title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please enter description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please select category")]
        public Guid SelectedCategoryId { get; set; }

        public CategoryViewModel Category { get; set; }

        public bool IsActive { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd }")]
        public DateTime CreatedOn { get; set; }

        public string CompanyName { get; set; }

        public int GuideStatsCount { get; set; }

        public HttpPostedFileBase CoverPicture { get; set; }

        public string CoverPictureUrl { get; set; }

        public byte[] CoverPicFileContent { get; set; }
        public object CoverPictureUpload { get; set; }
        public FileUploadResultViewModel CoverPictureVM { get; set; }

        [Required(ErrorMessage = "Please select category")]
        public string CategoryName { get; set; }

        public DateTime? DateAssigned { get; set; }

        public DateTime? DateLastViewed { get; set; }
        public List<UserViewModel> Collaborators { get; set; }
        public DateTime? LastEditDate { get; set; }
        public int UnreadFeedback { get; set; }
        public IEnumerable<FeedbackViewModel> Feedback { get; set; }
        public string TrainingLabels { get; set; }
        public PlaybookPreviewMode PlaybookPreviewMode { get; set; }
        public bool Printable { get; set; }
    }

    public class TrainingGuideViewModelShort
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? TrainingTestId { get; set; }
    }
}