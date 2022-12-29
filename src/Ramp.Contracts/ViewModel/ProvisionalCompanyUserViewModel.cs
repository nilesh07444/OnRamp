using System;
using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class ProvisionalCompanyUserViewModel
    {
        public ProvisionalCompanyUserViewModel()
        {
            UserList = new List<UserViewModel>();
            CompanyList = new List<CompanyViewModel>();
        }

       
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public List<UserViewModel> UserList { get; set; }
        public List<CompanyViewModel> CompanyList { get; set; }

        public UserViewModel UserViewModel { get; set; }
    }
}