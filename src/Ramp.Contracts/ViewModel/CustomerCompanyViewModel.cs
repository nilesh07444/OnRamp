using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class CustomerCompanyViewModel
    {
        public CustomerCompanyViewModel()
        {
            CompanyList = new List<CompanyViewModel>();
        }

        public List<CompanyViewModel> CompanyList { get; set; }

        public CompanyViewModel CompanyViewModel { get; set; }
    }
}