using Domain.Customer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ramp.Contracts.ViewModel.AcrobatFieldDetailsViewModel;

namespace Ramp.Contracts.ViewModel
{
    public class AcrobatFieldContentBoxModel
    {

        public string Id { get; set; }
        public string ParentId { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public string Content { get; set; }
        public bool Deleted { get; set; }
        public bool AttachmentRequired { get; set; }
        public bool IsConditionalLogic { get; set; }
        public List<string> TestQuestion { get; set; }
        public List<string> TestAnswer { get; set; }
        public bool IsAttached { get; set; }
        public bool IsSignOff { get; set; } = false;
        public string SignatureUploadId { get; set; }
        public string SignatureThumbnail { get; set; }
        public bool IsChecked { get; set; } = false;
        public bool NoteAllow { get; set; }
        public bool New { get; set; }
        public object Upload { get; set; }
        public string selectedTestQuestion { get; set; }
        public string AcrofieldValue { get; set; }
        public string selectedTestAnswer { get; set; }
        public string ChapterDocLinks { get; set; }
        public string ChapterDocNames { get; set; }
        public bool IsStandardUserAttachements { get; set; } = false;
        public int DocLinkAndAttachmentCount { get; set; }
        public List<DocumentUrlViewModel> DocLinks { get; set; }
        public string IssueDiscription { get; set; }
        public int CustomDocumentOrder { get; set; } = 0;
        public IEnumerable<ConditionalTable> ConditionalTable { get; set; } = new List<ConditionalTable>();
        public IEnumerable<UploadResultViewModel> Attachments { get; set; } = new List<UploadResultViewModel>();
        public IEnumerable<UploadResultViewModel> StandardUserAttachments { get; set; } = new List<UploadResultViewModel>();
        public IEnumerable<UploadFromContentToolsResultModel> ContentToolsUploads { get; set; } = new List<UploadFromContentToolsResultModel>();
        public IEnumerable<StandardUserAdobeFieldValues> AdobeFieldValues { get; set; } = new List<StandardUserAdobeFieldValues>();
    }
}
