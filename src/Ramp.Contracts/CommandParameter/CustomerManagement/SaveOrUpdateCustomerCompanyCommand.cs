using Common.Command;
using Domain.Enums;
using Domain.Models;
using System;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
    public class SaveOrUpdateCustomerCompanyCommand : ICommand
    {
        public Guid CurrentUserId { get; set; }
        public Guid Id { get; set; }
        public string CompanyName { get; set; }
        public string PhysicalAddress { get; set; }
        public string PostalAddress { get; set; }
        public string ClientSystemName { get; set; }
        public Guid CreatedBy { get; set; }
        public string TelephoneNumber { get; set; }
        public string WebsiteAddress { get; set; }
        public string LayerSubDomain { get; set; }

        //public Guid ProvisionalAccountLink { get; set; }
        public CompanyType CompanyType { get; set; }

        public Guid SelectedProvisionalAccountLink { get; set; }
        public string LogoImageUrl { get; set; }
        //public Guid? SelectedPackage { get; set; }
        public string SelectedBundle { get; set; }

        public bool IsChangePasswordFirstLogin { get; set; }
        public bool IsSendWelcomeSMS { get; set; }
        public bool IsLock { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool YearlySubscription { get; set; }
        public bool AutoExpire { get; set; }
		public string JitsiServerName { get; set; }
		public int DefaultUserExpireDays { get; set; }
        public bool ShowCompanyNameOnDashboard { get; set; }
        public string IconSet { get; set; }
        public bool EnableTrainingActivityLoggingModule { get; set; }
		public bool EnableChecklistDocument { get; set; }
		public bool EnableCategoryTree { get; set; }
		public bool EnableGlobalAccessDocuments { get; set; }
		public bool EnableVirtualClassRoom { get; set; }
		public bool EnableRaceCode { get; set; }
        public NotificationInterval TestExpiryNotificationInterval { get; set; }
        public int? ArbitraryTestExpiryIntervalInDaysBefore { get; set; }

		public bool ActiveDirectoryEnabled { get; set; }
		public string Domain { get; set; }
		public string Port { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
	}
}