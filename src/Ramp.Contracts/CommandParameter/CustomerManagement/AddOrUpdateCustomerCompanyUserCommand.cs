using System;
using System.Collections.Generic;
using Common.Command;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
    public class AddOrUpdateCustomerCompanyUserCommand : ICommand
    {
        public Guid UserId { get; set; }
        public Guid ParentUserId { get; set; }
        public CompanyViewModel Company { get; set; }
        public string FirstName { get; set; }
        public virtual Guid CompanyId { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string ContactNumber { get; set; }
        public string MobileNumber { get; set; }
        public string GroupName { get; set; }
        public Guid GroupCreatedForCompanyId { get; set; }
        public Guid SelectedGroupId { get; set; }
        public PackageViewModel Package { get; set; }
        //public Guid PackageId { get; set; }
        public Guid UserRoleId { get; set; }
        public string ClientSystemName { get; set; }
        public bool HasCreated { get; set; }
        public int? ExpireDays { get; set; }
        public IList<RoleViewModel> Roles { get; set; }
        public string EmployeeNo { get; set; }
    }
}