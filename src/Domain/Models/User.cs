using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class User : DomainObject
    {
        public Guid ParentUserId { get; set; }
        public virtual Company Company { get; set; }
        public string FirstName { get; set; }
        public string ContactNumber { get; set; }
        public virtual Guid CompanyId { get; set; }
        public virtual Guid? GroupId { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string MobileNumber { get; set; }
        public bool IsActive { get; set; }
        public string LogoImageUrl { get; set; }
        public string CompanyType { get; set; }
        public virtual Group Group { get; set; }
        public bool IsConfirmEmail { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsUserExpire { get; set; }
        public int? ExpireDays { get; set; }
        public bool IsFromSelfSignUp { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public string EmployeeNo { get; set; }
        public string IDNumber { get; set; }
        public string Department { get; set; }
    }
}