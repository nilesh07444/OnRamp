using Common.Command;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
    public class SaveSelfProvisionalCustomerCompanyCommand : ICommand
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; }
        public string PhysicalAddress { get; set; }
        public string PostalAddress { get; set; }
        public string ClientSystemName { get; set; }
        public string TelephoneNumber { get; set; }
        public string WebsiteAddress { get; set; }
        public string LayerSubDomain { get; set; }
        public string LogoImageUrl { get; set; }

        public Guid UserId { get; set; }
        public Guid ParentUserId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string MobileNumber { get; set; }
        public string AbsoluteUrl { get; set; }
        public string IDNumber { get; set; }
        public string Gender { get; set; }
    }
}