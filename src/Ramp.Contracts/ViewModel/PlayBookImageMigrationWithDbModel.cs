using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models;

namespace Ramp.Contracts.ViewModel
{
    public class PlayBookImageMigrationWithDbModel : IViewModel
    {
        public PlayBookImageMigrationWithDbModel()
        {
            dicCompany = new Dictionary<Guid, Company>();
            dicPlayBook = new Dictionary<Guid, TrainingGuide>();
            dicPlayBookChapter = new Dictionary<Guid, TraningGuideChapter>();
            dicChapterUpload = new Dictionary<Guid, ChapterUpload>();
            CustomerCompanies = new List<CustomerCompany>();
        }

        public Dictionary<Guid, Company> dicCompany { get; set; }
        public Dictionary<Guid, TrainingGuide> dicPlayBook { get; set; }
        public Dictionary<Guid, TraningGuideChapter> dicPlayBookChapter { get; set; }
        public Dictionary<Guid, ChapterUpload> dicChapterUpload { get; set; }
        public List<CustomerCompany> CustomerCompanies { get; set; }
    }

    public class CustomerCompany
    {
        public CustomerCompany()
        {
            CompanyTrainingGuides = new List<CompanyTrainingGuide>();
            CompanyTests = new List<CompanyTests>();
        }
        public string CompanyName { get; set; }
        public List<CompanyTrainingGuide> CompanyTrainingGuides { get; set; }
        public List<CompanyTests> CompanyTests { get; set; }
    }

    public class CompanyTrainingGuide
    {
        public CompanyTrainingGuide()
        {
            CompanyTrainingGuideChapters = new List<CompanyTrainingGuideChapter>();
        }
        public string TrainingGuideName { get; set; }
        public List<CompanyTrainingGuideChapter> CompanyTrainingGuideChapters { get; set; }
    }

    public class CompanyTrainingGuideChapter
    {
        public CompanyTrainingGuideChapter()
        {
            chapterUploads = new List<Uploads>();
        }
        public string ChapterName { get; set; }
        public List<Uploads> chapterUploads { get; set; }
    }

    public class CompanyTests
    {
        public CompanyTests()
        {
            TestUploads = new List<Uploads>();
        }
        public List<Uploads> TestUploads { get; set; }
    }

    public class Uploads
    {
        public Guid UploadId { get; set; }
        public string DocumentName { get; set; }
    }

}
