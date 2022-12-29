using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{

    public class CustomDocumentListModel : DocumentListModel
    {
    }

    public class CustomDocumentModel : CustomDocumentListModel
    {
        public List<DocumentUrlViewModel> DocLinks { get; set; }
        public DocumentPreviewMode PreviewMode { get; set; }
        public object CoverPictureUpload { get; set; }
        public string CallToActionMessage { get; set; } = "some values";

        public IEnumerable<TrainingManualChapterModel> TMContentModels { get; set; } = new List<TrainingManualChapterModel>();
        public IEnumerable<AcrobatFieldContentBoxModel> AcrobatFieldContentModels { get; set; } = new List<AcrobatFieldContentBoxModel>();
        public IEnumerable<MemoContentBoxModel> MemoContentModels { get; set; } = new List<MemoContentBoxModel>();
        public IEnumerable<PolicyContentBoxModel> PolicyContentModels { get; set; } = new List<PolicyContentBoxModel>();        
        public IEnumerable<TestQuestionModel> TestContentModels { get; set; } = new List<TestQuestionModel>();
        public IEnumerable<FormChapterModel> FormContentModels { get; set; } = new List<FormChapterModel>();

        public FormFieldModel FormField { get; set; } = new FormFieldModel();
        public TestResultModel TestContent { get; set; } = new TestResultModel();
        public PolicyModel PolicyContent { get; set; } = new PolicyModel();
        public IEnumerable<CheckListChapterModel> CLContentModels { get; set; } = new List<CheckListChapterModel>();

        public IEnumerable<ContenTableData> contenTableDatas { get; set; } = new List<ContenTableData>();

        public string CertificateId { get; set; }
        public string CertificateThumbnailId { get; set; }
        public UploadResultViewModel Certificate { get; set; }
        public int? TestExpiresNumberDaysFromAssignment { get; set; }
        public TestExpiryNotificationInterval NotificationInteval { get; set; }
        public int? NotificationIntevalDaysBeforeExpiry { get; set; }
        public bool IsResourceCentre { get; set; }
        public List<ContenTableData> ContenTableDataList { get; set; }
        public string AssignedDocumentId { get; set; }

        public bool IsReportView { get; set; }
    }

    public class SectionInfo
    {
        public string ID { get; set; }
        public DocumentType Type { get; set; }
    }
    public class MyCustumDocumentCheckList
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public string Extra { get; set; }

    }
    public class ContenTableData
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int CustomDocumentOrder { get; set; }
        public Object SectionData { get; set; }
    }

    public class ContentModel
    {
        public Object ContentModelData { get; set; }
    }

}
