using Domain.Customer.Models;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Customer.Models
{
    public class StandardUser : Base.CustomerDomainObject
    {
        public StandardUser()
        {
            Roles = new List<CustomerRole>();
            TrainingGuidesCollabaratedOn = new List<TrainingGuide>();
            TestCertificates = new List<TestCertificate>();
        }

        public string LayerSubDomain { get; set; }
        public Guid ParentUserId { get; set; }
        public Guid CompanyId { get; set; }
        public string FirstName { get; set; }
        public string ContactNumber { get; set; }
        public virtual CustomerGroup Group { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string MobileNumber { get; set; }
        public bool IsActive { get; set; }
        public string LogoImageUrl { get; set; }
        public string CompanyType { get; set; }
        public bool IsConfirmEmail { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool IsUserExpire { get; set; }
        public int? ExpireDays { get; set; }
        public bool IsFromSelfSignUp { get; set; }
        public virtual List<CustomerRole> Roles { get; set; }
        public string EmployeeNo { get; set; }
        public virtual List<TrainingGuide> TrainingGuidesCollabaratedOn { get; set; }
        public virtual List<TestCertificate> TestCertificates { get; set; }
        public string IDNumber { get; set; }
        public virtual GenderEnum.Gender? Gender { get; set; }
        public virtual Guid? RaceCodeId { get; set; }
        public virtual IList<StandardUserDisclaimerActivityLog> DisclaimerActivityLog { get; set; } = new List<StandardUserDisclaimerActivityLog>();
		public string TrainingLabels { get; set; } = string.Empty;
		public Guid? CustomUserRoleId { get; set; }
        public bool AdUser { get; set; }
        public string Department { get; set; }
    }
}