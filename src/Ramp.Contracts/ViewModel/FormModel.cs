using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class FormListModel : DocumentListModel
    {
        
    }

    public class FormModel : FormListModel
    {
        public string Title { get; set; }
        public IEnumerable<FormChapterModel> FormContentModels { get; set; } = new List<FormChapterModel>();
    }
}
