using Common.Data;
using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class PolicyListModel: DocumentListModel
    {
        
    }
    public class PolicyModel : PolicyListModel
    {
		//public List<DocumentUrlViewModel> DocLinks { get; set; }
		public bool CallToAction { get; set; }
        public string selectedTestQuestion { get; set; }
        public string selectedTestAnswer { get; set; }
        public List<string> TestQuestion { get; set; } = new List<string>();
        public List<string> TestAnswer { get; set; } = new List<string>();
        public bool IsShowPolicy { get; set; } = false;
        public string CallToActionMessage { get; set; }
        public DocumentPreviewMode PreviewMode { get; set; }
        public object CoverPictureUpload { get; set; }
        public bool IsConditionalLogic { get; set; }
        public int CustomDocumentOrder { get; set; } = 0;
        public IEnumerable<PolicyContentBoxModel> ContentModels { get; set; } = new List<PolicyContentBoxModel>();

        
        //********************* This Block Has Been Added By Softude *******************************

        public bool IsSignOff { get; set; } = false;
        public bool NoteAllow { get; set; } = false;
        public string Content { get; set; }

        //***************************** Block End **************************************************
    }
}
