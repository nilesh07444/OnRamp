using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel {
	public class CustomerSelfSignUpViewModel : IViewModel
    {
        public virtual Guid CompanyId { get; set; }

		public string EmployeeNo { get; set; }

		
        public string FullName { get; set; }

       
       
        public string EmailAddress { get; set; }

      
        public string Password { get; set; }

       
        public string ConfirmPassword { get; set; }

       
        public string MobileNumber { get; set; }

        public string IDNumber { get; set; }

       
        public string Gender { get; set; }
		public bool IsEmpCodeRequired { get; set; }
		public bool IsEnabledEmployeeCode { get; set; }
		public IEnumerable<SelectListItem> GenderDropDown { get; set; }
        public string RaceCode { get; set; }
        public string LogoPath { get; set; }
        public CompanyViewModel CompanyViewModel { get; set; }
    }
}