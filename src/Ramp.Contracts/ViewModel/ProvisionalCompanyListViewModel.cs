using System;
using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class ProvisionalCompanyListViewModel : IViewModel
    {
        public ProvisionalCompanyListViewModel()
        {
            FromProvisionalCompanyList = new List<CompanyViewModel>();
            ToProvisionalCompanyList = new List<CompanyViewModel>();
        }
        public Guid FromSelectedProvisionalCompany { get; set; }
        public Guid ToSelectedProvisionalCompany { get; set; }
        public IEnumerable<SerializableSelectListItem> FromCompanies { get; set; }
        public IEnumerable<SerializableSelectListItem> ToCompanies { get; set; }
        public IList<CompanyViewModel> FromProvisionalCompanyList { get; set; }
        public IList<CompanyViewModel> ToProvisionalCompanyList { get; set; }
    }
}