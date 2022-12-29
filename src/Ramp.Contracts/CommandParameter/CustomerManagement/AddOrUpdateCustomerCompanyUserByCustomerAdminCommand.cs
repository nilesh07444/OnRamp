using Common.Command;
using Domain.Enums;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using static Domain.Enums.GenderEnum;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
    public class AddOrUpdateCustomerCompanyUserByCustomerAdminCommand : ICommand
    {
        public AddOrUpdateCustomerCompanyUserByCustomerAdminCommand()
        {
            Roles = new List<string>();
        }

        public Guid UserId { get; set; }
        public Guid ParentUserId { get; set; }
        public CompanyViewModel Company { get; set; }
        public string FirstName { get; set; }
        public virtual Guid CompanyId { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string MobileNumber { get; set; }
        public PackageViewModel Package { get; set; }
        public List<string> SelectedGroupId { get; set; }
        public List<string> Roles { get; set; }
        public Guid UserRoleId { get; set; }
        public string ClientSystemName { get; set; }
        public int? ExpireDays { get; set; }
        public string EmployeeNo { get; set; }
        public string GroupName { get; set; }
        public IList<RoleViewModel> RolesViewModel { get; set; }
        public bool IsFromSelfSignUp { get; set; }
        public bool IsConfirmEmail { get; set; }
        public bool IsFromDataMigration { get; set; }
        public string IDNumber { get; set; }
        public string Gender { get; set; }
        public Guid? RaceCodeId { get; set; }
        public string TrainingLabels { get; set; }
        public Guid? SelectedCustomUserRole { get; set; }
        public bool AdUser { get; set; }
        public bool ADSync { get; set; }

        public bool IsActive { get; set; }
         public string Department { get; set; }
    }

    public class UpdateStandardUserByAdUser : ICommand
    {
        public Guid UserId { get; set; }
        public Guid ParentUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public bool AdUser { get; set; }        
        public bool IsActive { get; set; }  
        public string GroupName { get; set; }
        public List<string> Roles { get; set; }

    }

}