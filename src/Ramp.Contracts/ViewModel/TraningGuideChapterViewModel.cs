using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel
{
    public class TraningGuideChapterViewModel : IViewModel
    {
        public TraningGuideChapterViewModel()
        {
            ChapterUpload = new List<ChapterUploadViewModel>();
            DocumentSequenceList = new List<DocumentSequenceViewModel>();
            Attachments = new List<FileUploadResultViewModel>();
            Errors = new List<FileUploadResultViewModel>();
            CKEUploads = new List<CKEditorUploadResultViewModel>();
        }

        public bool Minimized { get; set; }
        public Guid TraningGuideChapterId { get; set; }
        public List<ChapterUploadViewModel> ChapterUpload { get; set; }
        public Guid TraningGuidId { get; set; }
        public List<DocumentSequenceViewModel> DocumentSequenceList { get; set; }
        public bool IsDeleted { get; set; }
        public bool New { get; set; }
        [Required(ErrorMessage = "Please enter chapter name")]
        public string ChapterName { get; set; }

        public Int32 ChapterNumber { get; set; }

        public List<string> YouTubeUrl { get; set; }

        public List<string> VimeoUrl { get; set; }

        public string ChapterNumberText { get; set; }
        public string ChapterContent { get; set; }

        public Guid? SelectedTraningGuideId { get; set; }

        public string SelectedTraningGuideRefId { get; set; }

        public string SelectedTraningGuideTitle { get; set; }
        public List<FileUploadResultViewModel> Attachments { get; set; }
        public object Upload { get; set; }
        public string Link { get; set; }
        public bool AddLink { get; set; }
        public string LinkType { get; set; }
        public FileUploadResultViewModel AttachmentPreview { get; set; }
        public List<FileUploadResultViewModel> Errors { get; set; }
        public List<CKEditorUploadResultViewModel> CKEUploads { get; set; }
        public IList<UploadFromContentToolsResultModel> ContentToolsUploads { get; set; } = new List<UploadFromContentToolsResultModel>();
    }
}