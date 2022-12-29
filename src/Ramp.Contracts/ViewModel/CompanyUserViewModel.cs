using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel {
	public class CompanyUserViewModel
    {
        public CompanyUserViewModel()
        {
            UserList = new List<UserViewModel>();
			Paginate = new Paginate();

		}
		public UserChartViewModel UserChartViewModel { get; set; }
				
		public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public Guid SelectedPackageId { get; set; }
        public List<UserViewModel> UserList { get; set; }
        public IEnumerable<SerializableSelectListItem> PackageList { get; set; }
        public UserViewModel UserViewModel { get; set; }
        public SelfSignUpViewModel SelfSignUpViewModel { get; set; }
		public List<FilterCompanyCustomer> FilterCompanyCustomer { get; set; }
		public Paginate Paginate { get; set; }
		//[Required(ErrorMessage = "Please enter email")]
		//[RegularExpression(".+\\@.+\\..+", ErrorMessage = "Please enter a valid email address")]
		//[Remote("DoesEmailAlreadyPresent", "UserMgmt", "UserManagement",
		//   ErrorMessage = "This email address is already in use in the OnRAMP system, please enter a unique email address")]
		public string EmailAddress { get { return UserViewModel.EmailAddress; } }
	}
	public class FilterCompanyCustomer {
		public string Type { get; set; }
		public List<FilterData> FilterData { get; set; }
	}
	public class UserChartViewModel {
		public UserChartViewModel() {
			RoleName = new List<string>();
			RoleCount = new List<int>();
		}

		public List<string> RoleName { get; set; }
		public List<int> RoleCount { get; set; }
		public List<string> TypeName { get; set; }
		public List<int> TypeCount { get; set; }
		public List<string> Status { get; set; }
		public List<int> StatusCount { get; set; }
	}

	public class FilterData {
		public string Id { get; set; }
		public string Name { get; set; }
	}

	public class Paginate {

		public int PageSize { get; set; } =10;
		public int PageIndex { get; set; } = 1;
		public bool IsFirstPage { get; set; } = true;
		public bool IsLastPage { get; set; } = false;
		public int Page { get; set; } = 7;
		public int StartPage { get; set; } = 1;
		public int EndPage { get; set; } 
		public int TotalItems { get; set; }
		public int FirstPage { get; set; }
		public int LastPage { get; set; }
	}
}