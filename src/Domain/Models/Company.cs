using System;
using System.Collections.Generic;
using Iesi.Collections.Generic;
using Domain.Enums;

namespace Domain.Models
{
    public class Company : 
        DomainObject
    {
        private ISet<User> _users;
        private ISet<Group> _groups;

        public bool ApplyCustomCss { get; set; }
        public bool? AutoExpire { get; set; }
        public string ClientSystemName { get; set; }
        public string CompanyConnectionString { get; set; }
        public string CompanyName { get; set; }
		public string CompanySiteTitle { get; set; }
        public CompanyType CompanyType { get; set; }
        public virtual Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public byte[] customCssFile { get; set; }
        public virtual IList<CustomerConfiguration> CustomerConfigurations { get; set; } = new List<CustomerConfiguration>();
        public int? DefaultUserExpireDays { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public virtual ISet<Group> GroupList
        {
            get { return _groups ?? (_groups = new LinkedHashSet<Group>()); }
            set { _groups = value; }
        }
		public bool EnableChecklistDocument { get; set; }
		public bool EnableCategoryTree { get; set; }
		public bool EnableGlobalAccessDocuments { get; set; }
		public string JitsiServerName { get; set; }
		public bool EnableVirtualClassRoom { get; set; }
		public bool IsActive { get; set; }
        public bool IsChangePasswordFirstLogin { get; set; }
        public bool IsForSelfProvision { get; set; }
        public bool IsForSelfSignUp { get; set; }
		public bool IsEnabledEmployeeCode { get; set; }
		public bool IsLock { get; set; }
        public bool IsSelfCustomer { get; set; }
        public bool IsSelfSignUpApprove { get; set; }
        public bool IsSendWelcomeSMS { get; set; }
        public string LayerSubDomain { get; set; }
        public string LogoImageUrl { get; set; }
        public virtual Bundle Bundle { get; set; }
        public virtual string BundleId { get; set; }
        public string PhysicalAddress { get; set; }
        public string PostalAddress { get; set; }
        public Guid ProvisionalAccountLink { get; set; }
        public bool ShowCompanyNameOnDashboard { get; set; }
        public bool HideDashboardLogo { get; set; }
        public string TelephoneNumber { get; set; }
		public bool IsEmployeeCodeReq { get; set; }
		public virtual ISet<User> UserList
        {
            get { return _users ?? (_users = new LinkedHashSet<User>()); }
            set { _users = value; }
        }
        public string WebsiteAddress { get; set; }
        public bool? YearlySubscription { get; set; } 
        public virtual IconSet IconSet { get; set; }
        public virtual CustomColour CustomColours { get; set; }
        public string LegalDisclaimer { get; set; }
        public bool ShowLegalDisclaimerOnLoginOnlyOnce { get; set; }
        public bool ShowLegalDisclaimerOnLogin { get; set; }
        public bool EnableTrainingActivityLoggingModule { get; set; }
        public bool EnableRaceCode { get; set; }
        public NotificationInterval TestExpiryNotificationInterval { get; set; }
        public int? ArbitraryTestExpiryIntervalInDaysBefore { get; set; }

        public virtual FileUpload DashboardVideoFile { get; set; }
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
    public enum LegalDisclaimerActivationType
    {
        Disabled = 0,
        ShowOnLogin = 1,
        ShowOnLoginOnce = 2
    }
    public enum NotificationInterval
    {
        Arbitrary =3,
        Daily = 1,
        OneDayBefore = 0,
    }
}