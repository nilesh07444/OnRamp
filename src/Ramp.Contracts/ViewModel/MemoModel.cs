using Common.Data;
using Domain.Customer;
using Domain.Customer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class MemoListModel : DocumentListModel
    {
    }
    public class MemoModel : MemoListModel
    {
		//public List<DocumentUrlViewModel> DocLinks { get; set; }
		public DocumentPreviewMode PreviewMode { get; set; }
        public object CoverPictureUpload { get; set; }
        public IEnumerable<AcrobatFieldContentBoxModel> AcrobatFieldContentModels { get; set; } = new List<AcrobatFieldContentBoxModel>();
        public IEnumerable<MemoContentBoxModel> ContentModels { get; set; } = new List<MemoContentBoxModel>();
       public  IEnumerable<ConditionalTable> ConditionalTable { get; set; } = new List<ConditionalTable>();

    }
}
