using System;
using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class CompanyViewModelLong
    {
        public CompanyViewModelLong()
        {
            CompanyList = new List<CompanyViewModel>();
        }
        public Guid ToSelectedProvisionalCompany { get; set; }
        public Guid FromSelectedProvisionalCompany { get; set; }
        public List<CompanyViewModel> CompanyList { get; set; }
        public List<PackageViewModel> PackageList { get; set; }

        public CompanyViewModel CompanyViewModel { get; set; }
        public BundleViewModel BundleViewModel { get; set; }
        //public PackageViewModel PackageViewModel { get; set; }
        public bool CopyTrainingTests{ get; set; }
    }
}