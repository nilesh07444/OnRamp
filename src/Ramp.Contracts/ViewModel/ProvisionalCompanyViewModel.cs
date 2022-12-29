using System;
using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class ProvisionalCompanyViewModel
    {
        public ProvisionalCompanyViewModel()
        {
            CompanyList = new List<CompanyViewModel>();
        }

        public Guid ToSelectedProvisionalCompany { get; set; }

        public List<CompanyViewModel> CompanyList { get; set; }

        public CompanyViewModel CompanyViewModel { get; set; }
    }
}