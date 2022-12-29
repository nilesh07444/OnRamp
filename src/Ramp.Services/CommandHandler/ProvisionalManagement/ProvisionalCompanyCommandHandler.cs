using System;
using Common.Data;
using Common.Command;
using Domain.Models;
using System.Collections.Generic;
using System.Linq;
using Ramp.Contracts.CommandParameter.ProvisionalManagement;
using Domain.Enums;

namespace Ramp.Services.CommandHandler.ProvisionalManagement
{
    public class ProvisionalCompanyCommandHandler : CommandHandlerBase<ProvisionalCompanyCommandParameter>
    {
        private readonly IRepository<Company> _companyRepository;

        public ProvisionalCompanyCommandHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override CommandResponse Execute(ProvisionalCompanyCommandParameter command)
        {
            Company companyModel = _companyRepository.Find(command.Id);
            if (command.IsForSelfProvision == true)
            {
                // code for reset the previous self provision
                var ProvisionalCompany = _companyRepository.GetAll();
                Company selfProvisionalCompany = ProvisionalCompany.Where(c => c.IsForSelfProvision == true).FirstOrDefault();
                if (selfProvisionalCompany != null)
                {
                    selfProvisionalCompany.IsForSelfProvision = false;
                    _companyRepository.SaveChanges();
                }
            }

            if (companyModel == null)
            {
                Guid compId = Guid.NewGuid();
                var company = new Company
                {
                    Id = compId,
                    CompanyName = command.CompanyName,
                    LayerSubDomain = command.LayerSubDomain,
                    CreatedBy = command.CompanyCreatedByUserId,
                    PhysicalAddress = command.PhysicalAddress,
                    PostalAddress = command.PostalAddress,
                    ProvisionalAccountLink = Guid.Empty,
                    TelephoneNumber = command.TelephoneNumber,
                    WebsiteAddress = command.WebsiteAddress,
                    CompanyType = CompanyType.ProvisionalCompany,
                    LogoImageUrl = command.LogoImageUrl,
                    CreatedOn = DateTime.Now,
                    UserList = null,
                    IsChangePasswordFirstLogin = command.IsChangePasswordFirstLogin,
                    IsSendWelcomeSMS = command.IsSendWelcomeSMS,
                    IsForSelfProvision = command.IsForSelfProvision,
                    IsLock = false,
                    IsSelfCustomer = false,
                    ApplyCustomCss = false,
                    customCssFile = null,
                    IsSelfSignUpApprove = true,
                    IsForSelfSignUp = false,
                };
                _companyRepository.Add(company);
                _companyRepository.SaveChanges();

                if (command.IsForSelfProvision == true)
                {
                    var Company = _companyRepository.GetAll();
                    var selfCustomerCompany = Company.Where(c => c.IsSelfCustomer == true && c.CompanyType == CompanyType.CustomerCompany).ToList();
                    if (selfCustomerCompany.Count > 0)
                    {

                        foreach (Company comp in selfCustomerCompany)
                        {
                            comp.ProvisionalAccountLink = compId;
                            _companyRepository.SaveChanges();
                        }
                    }

                }

            }
            else
            {
                companyModel.CreatedBy = command.CompanyCreatedByUserId;
                companyModel.CompanyName = command.CompanyName;
                companyModel.LayerSubDomain = command.LayerSubDomain;
                companyModel.PhysicalAddress = command.PhysicalAddress;
                companyModel.PostalAddress = command.PostalAddress;
                companyModel.ProvisionalAccountLink = command.ProvisionalAccountLink;
                companyModel.TelephoneNumber = command.TelephoneNumber;
                companyModel.WebsiteAddress = command.WebsiteAddress;
                if (command.LogoImageUrl != "")
                    companyModel.LogoImageUrl = command.LogoImageUrl;
                companyModel.IsChangePasswordFirstLogin = command.IsChangePasswordFirstLogin;
                companyModel.IsSendWelcomeSMS = command.IsSendWelcomeSMS;
                companyModel.IsForSelfProvision = command.IsForSelfProvision;
                companyModel.IsLock = false;
                companyModel.IsSelfCustomer = false;
                companyModel.IsSelfSignUpApprove = true;
                companyModel.IsForSelfSignUp = false;
                _companyRepository.SaveChanges();

                if (command.IsForSelfProvision == true)
                {
                    var Company = _companyRepository.GetAll();
                    var selfCustomerCompany = Company.Where(c => c.IsSelfCustomer == true && c.CompanyType == CompanyType.CustomerCompany).ToList();
                    if (selfCustomerCompany.Count > 0)
                    {

                        foreach (Company comp in selfCustomerCompany)
                        {
                            comp.ProvisionalAccountLink = companyModel.Id;
                            _companyRepository.SaveChanges();
                        }
                    }

                }

            }
            return null;
        }
    }
}