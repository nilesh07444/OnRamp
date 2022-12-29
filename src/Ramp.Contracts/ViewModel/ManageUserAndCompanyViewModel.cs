using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class ManageUserAndCompanyViewModel : IViewModel
    {
        public ManageUserAndCompanyViewModel()
        {
            CompanyList = new List<CompanyViewModel>();
            UserList = new List<UserViewModel>();
        }

        public List<CompanyViewModel> CompanyList { get; set; }
        public List<UserViewModel> UserList { get; set; }
    }
}