using Common.Query;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class GetTrainingGuidesToCopyFromCompanyQueryParameter : IQuery
    {
        public GetTrainingGuidesToCopyFromCompanyQueryParameter()
        {
            TrainingGuideIDList = new List<string>();
        }

        public List<string> TrainingGuideIDList { get; set; }
        public Guid CopyFromCustomerCompanyGuideId { get; set; }
        public Guid CopyToCustomerCompanyGuideId { get; set; }
        public bool CopyTests { get; set; }
    }
}