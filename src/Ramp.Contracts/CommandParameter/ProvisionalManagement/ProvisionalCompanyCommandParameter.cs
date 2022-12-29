using System;
using Common.Command;
using Domain.Models;
using Domain.Enums;

namespace Ramp.Contracts.CommandParameter.ProvisionalManagement
{
    public class ProvisionalCompanyCommandParameter : ICommand
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; }
        public string PhysicalAddress { get; set; }
        public string PostalAddress { get; set; }
        public string TelephoneNumber { get; set; }
        public string WebsiteAddress { get; set; }
        public string LayerSubDomain { get; set; }
        public Guid ProvisionalAccountLink { get; set; }
        public Guid CompanyCreatedByUserId { get; set; }
        public CompanyType CompanyType { get; set; }
        public string LogoImageUrl { get; set; }

        public bool IsChangePasswordFirstLogin { get; set; }
        public bool IsSendWelcomeSMS { get; set; }
        public bool IsForSelfProvision { get; set; }
    }
}
