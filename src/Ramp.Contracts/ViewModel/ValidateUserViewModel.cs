using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Ramp.Contracts.ViewModel
{
    public class ValidateUserViewModel : IViewModel
    {
        public ValidateUserViewModel()
        {
            UserRole= new List<UserRole>();
        }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public List<UserRole> UserRole { get; set; }
        public CompanyViewModel UserCompany { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string Password { get; set; }
        public string ClientSystemName { get; set; }
        public string CustomerUserConnectionString { get; set; }
    }
}