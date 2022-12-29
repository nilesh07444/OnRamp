using Common.Data;
using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class TrainingManualListModel  : DocumentListModel
    {
    }
    public class TrainingManualModel : TrainingManualListModel
    {
		public List<DocumentUrlViewModel> DocLinks { get; set; }
		public DocumentPreviewMode PreviewMode { get; set; }
        public object CoverPictureUpload { get; set; }
        public IEnumerable<TrainingManualChapterModel> ContentModels { get; set; } = new List<TrainingManualChapterModel>();
    }
}
