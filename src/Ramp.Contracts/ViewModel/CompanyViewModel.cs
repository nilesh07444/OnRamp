using Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using CompanyType = Domain.Enums.CompanyType;

namespace Ramp.Contracts.ViewModel
{
    public class CompanyViewModel : 
        IViewModel
    {
        public CompanyViewModel()
        {
            CompanyList = new List<CompanyModelShort>();
            CompanyUserList = new List<UserViewModel>();
            FromCustomerCompanyList = new List<CompanyModelShort>();
            //PackageList = new List<PackageViewModel>();
            BundleList = new List<BundleViewModel>();
            ToCustomerCompanyList = new List<CompanyModelShort>();
            UserList = new List<UserViewModel>();
        }
		public bool IsActive { get; set; }
		public bool EnableChecklistDocument { get; set; }
		public bool EnableCategoryTree { get; set; }
		public bool EnableGlobalAccessDocuments { get; set; }
		public bool IsEnabledEmployeeCode { get; set; }
		public string CompanySiteTitle { get; set; }
		public string JitsiServerName { get; set; }
		public bool EnableVirtualClassRoom { get; set; }
		public bool ApplyCustomCss { get; set; }
        public bool AutoExpire { get; set; }
        public double AverageRatingByCompany { get; set; }
        [Required(ErrorMessage = "Please enter System name")]
        [Remote("DoesClientSystemNameAlreadyPresent", "ProvisionalMgmt", "ProvisionalManagement", ErrorMessage = "Client System Name already exists")]
        [MaxLength(50)]
        public string ClientSystemName { get; set; }
        public IEnumerable<SerializableSelectListItem> Companies { get; set; }
		public IList<UserViewModel> UserList { get; set; }
        public IList<UserViewModel> CompanyUserList { get; set; }
        public IList<CompanyModelShort> CompanyList { get; set; }
        //public IList<PackageViewModel> PackageList { get; set; }
        public IList<BundleViewModel> BundleList { get; set; }
        public Guid FromSelectedCustomerCompany { get; set; }
        public Guid ToSelectedCustomerCompany { get; set; }
        public IEnumerable<SerializableSelectListItem> FromCompanies { get; set; }
        public IEnumerable<SerializableSelectListItem> ToCompanies { get; set; }
        public IList<CompanyModelShort> FromCustomerCompanyList { get; set; }
        public IList<CompanyModelShort> ToCustomerCompanyList { get; set; }
        public IList<CustomerConfigurationModel> CustomerConfigurations { get; set; } = new List<CustomerConfigurationModel>();
        public CustomColourViewModel CustomColours { get; set; }
        public Guid Id { get; set; }
		public string SelectedGroupId { get; set; }

        public Guid CompanyCreatedBy { get; set; }
        public CustomConfigurationViewModel CustomConfiguration { get; set; }
        public HttpPostedFileBase CompanyLogo { get; set; }
        public HttpPostedFileBase LoginLogo { get; set; }
        public HttpPostedFileBase FooterLogo { get; set; }
		public HttpPostedFileBase NotificationHeaderLogo { get; set; }
        public HttpPostedFileBase NotificationFooterLogo { get; set; }

        [Required(ErrorMessage = "Please enter Company name")]
        [Remote("DoesCompanyNameAlreadyPresent", "ProvisionalMgmt", "ProvisionalManagement", ErrorMessage = "Company Name already exist")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Please enter Physical Address")]
        public string PhysicalAddress { get; set; }

        [Required(ErrorMessage = "Please enter Postal Address")]
        public string PostalAddress { get; set; }

        [Required(ErrorMessage = "Please enter Telephone Number")]
        [RegularExpression("^[0-9]*", ErrorMessage = "Please enter a valid telephone number")]
        public string TelephoneNumber { get; set; }

        [Required(ErrorMessage = "Please enter Website Address")]
        /*[RegularExpression(@"^[a-zA-Z0-9\-\.]+\.(com|org|net|mil|edu|COM|ORG|NET|MIL|EDU)$",
          ErrorMessage = "Please enter a valid Url")]*/
        [RegularExpression(@"^((http(s?):\/\/www\.)|(www\.)|(WWW\.)|(http:\/\/))[a-zA-Z0-9._-]+\.[a-zA-Z.]{2,5}$", ErrorMessage = "Please enter a valid Url")]
        public string WebsiteAddress { get; set; }

        public bool Status { get; set; }

        [Required(ErrorMessage = "Please enter Layer Sub-Domain")]
        [Remote("DoesLayerSubDomainAlreadyPresent", "ProvisionalMgmt", "ProvisionalManagement", ErrorMessage = "Layer Sub-Domain Name already exists")]
        public string LayerSubDomain { get; set; }

        [Required(ErrorMessage = "Please select Provisional AccountLink")]
        public Guid SelectedProvisionalAccountLink { get; set; }

        public Guid SelectedCompany { get; set; }

        public Guid ProvisionalAccountLink { get; set; }

        public String ProvisionalAccountName { get; set; }

        public CompanyType CompanyType { get; set; }

        public String CompanyConnectionString { get; set; }

        
        //public IEnumerable<SerializableSelectListItem> Packages { get; set; }

        public IEnumerable<SerializableSelectListItem> Bundles { get; set; }

        //[Required(ErrorMessage = "Please select Package")]
        //public Guid? SelectedPackage { get; set; }

        //[Required(ErrorMessage = "Please select Bundle")]
        public string SelectedBundle { get; set; }

        public string LogoImageUrl { get; set; }

        
        public DateTime CreatedOn { get; set; }
        //public string PackageName { get; set; }
        public string BundleName { get; set; }
        public string CustomersNumberOfDocuments { get; set; }
        public int BundleSize { get; set; }

        public string MobileNumber { get; set; }
        public bool IsChangePasswordFirstLogin { get; set; }
        public bool IsSendWelcomeSMS { get; set; }
        public bool IsForSelfProvision { get; set; }
		public bool IsEmployeeCodeReq { get; set; }
		public bool IsLock { get; set; }
        public bool IsSelfCustomer { get; set; }
        public DateTime ReportExpiryDate { get; set; }

        public string ExpireMessage { get; set; }

        public bool IsExpireReport { get; set; }

        public string MontlyOrAnual { get; set; }
        public byte[] customCssFile { get; set; }
        public bool IsForSelfSignUp { get; set; }
        public bool IsSelfSignUpApprove { get; set; }

        [Required(ErrorMessage = "Please enter Expiry Days")]
        [RegularExpression("^[0-9]*", ErrorMessage = "Please enter a valid Expiry days")]
        public int? DefaultUserExpireDays { get; set; }

        public HttpPostedFileBase CustomerCompanyCss { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool YearlySubscription { get; set; }
        public string IsYearly { get; set; }
        public string createdDate { get; set; }
        public bool ShowCompanyNameOnDashboard { get; set; }
        public bool ShowCompanyLogoOnDashboard { get; set; }
        public IEnumerable<SelectListItem> IconSets { get; set; } = new List<SelectListItem>();
        public IconSetModel IconSet { get; set; }
        [Required]
        public string SelectedIconSet { get; set; }
        public LegalDisclaimerActivationType? LegalDisclaimerActivationType { get; set; }
        public string LegalDisclaimer { get; set; }
        public bool EnableTrainingActivityLoggingModule { get; set; }
        public bool EnableRaceCode { get; set; }

		public NotificationInterval TestExpiryNotificationInterval { get; set; }
        public int? ArbitraryTestExpiryIntervalInDaysBefore { get; set; }
        public bool? DeleteLoginLogo { get; set; }
        public bool? DeleteDashboardLogo { get; set; }
        public bool? DeleteFooterLogo { get; set; }

		public bool? DeleteNotificationHeaderLogo { get; set; }
        public bool? DeleteNotificationFooterLogo { get; set; }

        public FileUpload DashboardVideoFile { get; set; }
        public string DashboardVideoFileId { get; set; }
        public string DashboardVideoTitle { get; set; }
        public string DashboardVideoDescription { get; set; }

        public string DashboardQuoteText { get; set; }
        public string DashboardQuoteAuthor { get; set; }


		public bool ActiveDirectoryEnabled { get; set; }
		public string Domain { get; set; }
		public string Port { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
	}

    public class CompanyModelShort
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CompanyConnectionString { get; set; }
    }

    

}