using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.Catagories
{
    public class CategoryStatisticsReportQueryParameter
    {
        public Guid? ProvisionalCompanyId { get; set; }
        public Guid? CustomerCompanyId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? TrainingGuideId { get; set; }
        public List<CategoryViewModel> CatList { get; set; }
        public Guid SelectedCategoryId { get; set; }
        public Guid SelectedCompanyId { get; set; }
    }
}