using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VirtuaCon;

namespace Domain.Customer.Models
{
    public class TrainingGuide : Base.CustomerDomainObject
    {
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public int GuideStatsCount { get; set; }
        public byte[] CoverPicFileContent { get; set; }
        public virtual FileUploads CoverPicture { get; set; }
        public virtual List<TraningGuideChapter> ChapterList { get; set; }
        public virtual List<Categories> Categories { get; set; }
        public virtual List<StandardUser> Collaborators { get; set; }
        public virtual DateTime? LastEditDate { get; set; }
        public virtual TestVersion TestVersion { get; set; } 
        public PlaybookPreviewMode PlaybookPreviewMode { get; set; }
        public virtual IList<TrainingLabel> TrainingLabels { get; set; } = new List<TrainingLabel>();
        public bool Printable { get; set; }
        public TrainingGuide()
        {
            Categories = new List<Categories>();
            ChapterList = new List<TraningGuideChapter>();
            Collaborators = new List<StandardUser>();
        }
    }
    public enum PlaybookPreviewMode
    {
        [EnumFriendlyName("Portrait")]
        Portrait,
        [EnumFriendlyName("Storybook")]
        Landscape
    }
}