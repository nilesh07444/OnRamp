using Common.Query;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class AllAssignedGuidesToStandardUserWithCategoryQueryParameter : IQuery
    {
        public Guid UserId { get; set; }
        public Guid CatId { get; set; }
        public List<CategoryViewModel> CatList { get; set; }
    }
}