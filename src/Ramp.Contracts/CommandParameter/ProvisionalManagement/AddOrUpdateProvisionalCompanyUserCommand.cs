using Common.Command;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.CommandParameter.ProvisionalManagement
{
    public class AddOrUpdateProvisionalCompanyUserCommand : ICommand
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
        public virtual IList<RoleViewModel> Roles { get; set; }
        public string IDNumber { get; set; }
    }
}