using Common.Query;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
    public class ManageableUsersForCompanyQueryParameter : IQuery
    {
        public Guid SelectedTrainingGuideId { get; set; }
        public Guid SelectedTrainingTestId { get; set; }
        public FunctionalMode FunctionalMode { get; set; }
    }
}