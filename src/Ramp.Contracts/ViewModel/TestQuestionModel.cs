using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class TestQuestionModel : IdentityModel<string>
    {
        //Added by Softude
        public string Title { get; set; }
        public int Number { get; set; }
        public bool Deleted { get; set; }
        public int Marks { get; set; } = 1;
        public object Upload { get; set; }
        public string Question { get; set; }
        public string CorrectAnswerId { get; set; }
        public bool CheckRequired { get; set; }
        public bool AttachmentRequired { get; set; }
        public bool dynamicFields { get; set; }
        public bool IsSignOff { get; set; }
        public string SignatureUploadId { get; set; }
        public string SignatureThumbnail { get; set; }


        //added by softude
        public bool IsConditionalLogic { get; set; } = false;


        //below code added by softude
        public string selectedTestQuestion { get; set; }
        public string selectedTestAnswer { get; set; }
        public List<string> TestQuestion { get; set; }
        public List<string> TestAnswer { get; set; }


        public bool NoteAllow { get; set; }
        public bool IsStandardUserAttachements { get; set; } = false;
        public int DocLinkAndAttachmentCount { get; set; }
        public List<DocumentUrlViewModel> DocLinks { get; set; }
        public string IssueDiscription { get; set; }
        public string SelectedAnswer { get; set; }
        public bool IsChecked { get; set; } = false;
        public int CustomDocumentOrder { get; set; } = 0;
        public IEnumerable<UploadResultViewModel> Attachments { get; set; } = new List<UploadResultViewModel>();
        public IEnumerable<UploadResultViewModel> StandardUserAttachments { get; set; } = new List<UploadResultViewModel>();
        public IEnumerable<CustomDocumentChapters> CustomDocumentChapters { get; set; } = new List<CustomDocumentChapters>();

        public IEnumerable<UploadFromContentToolsResultModel> ContentToolsUploads { get; set; } = new List<UploadFromContentToolsResultModel>();
        public IEnumerable<TestQuestionAnswerModel> Answers { get; set; } = new List<TestQuestionAnswerModel>();
        
    }
    public class TestQuestionResultModel : TestQuestionModel
    {
        public TestQuestionStateModel State { get; set; } = new TestQuestionStateModel();
        public new IEnumerable<TestQuestionAnswerResultModel> Answers { get; set; } = new List<TestQuestionAnswerResultModel>();
        public bool Correct { get; set; }
    }
    public class TestQuestionStateModel
    {
        public bool ViewLater { get; set; }
        public bool UnAnswered { get; set; }
    }
}
