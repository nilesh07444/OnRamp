using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class PolicyContentBoxModel : IdentityModel<string>
    {
        public string ParentId { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public string Content { get; set; }
        public bool Deleted { get; set; }
        /*public bool IsConditionalLogic { get; set; }*/    /*commented by softude*/
        public List<string> TestQuestion { get; set; }
        public List<string> TestAnswer { get; set; }
        public bool New { get; set; }
        public object Upload { get; set; }
        public string selectedTestQuestion { get; set; }
        public string selectedTestAnswer { get; set; }
        public string ChapterDocLinks { get; set; }
		public string ChapterDocNames { get; set; }
		public int DocLinkAndAttachmentCount { get; set; }
        public int CustomDocumentOrder { get; set; } = 0;
        public List<DocumentUrlViewModel> DocLinks { get; set; }
		public IEnumerable<UploadResultViewModel> Attachments { get; set; } = new List<UploadResultViewModel>();
        public IEnumerable<UploadFromContentToolsResultModel> ContentToolsUploads { get; set; } = new List<UploadFromContentToolsResultModel>();

        public bool IsConditionalLogic { get; set; } = false;  /*added by softude*/
        public bool AttachmentRequired { get; set; }    /*added by softude*/
        public IEnumerable<UploadResultViewModel> StandardUserAttachments { get; set; } = new List<UploadResultViewModel>();    /*added by softude*/
        public bool NoteAllow { get; set; } = false;        /*added by softude*/

        /*added by softude*/
        public bool IsAttached { get; set; } = false;
        public bool IsSignOff { get; set; } = false;
        public string SignatureUploadId { get; set; }
        public string SignatureThumbnail { get; set; }
        public bool IsChecked { get; set; } = false;
        public bool IsStandardUserAttachements { get; set; } = false;
        public string IssueDiscription { get; set; }
        public bool CheckRequired { get; set; }    /*added by softude*/


        //added by softude

        public string IsActionNeeded { get; set; }





    }
}
