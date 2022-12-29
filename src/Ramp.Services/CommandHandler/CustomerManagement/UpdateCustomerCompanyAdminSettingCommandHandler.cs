using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.Command.StandardUser;
using VirtuaCon;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class
        UpdateCustomerCompanyAdminSettingCommandHandler : CommandHandlerBase<UpdateCustomerCompanyAdminSettingCommand>
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<FileUpload> _uploadRepository;


        public UpdateCustomerCompanyAdminSettingCommandHandler(ICommandDispatcher commandDispatcher,
            IRepository<Company> companyRepository,
            IRepository<FileUpload> uploadRepository)
        {
            _commandDispatcher = commandDispatcher;
            _companyRepository = companyRepository;
            _uploadRepository = uploadRepository;
        }

        public override CommandResponse Execute(UpdateCustomerCompanyAdminSettingCommand command)
        {
            Company companyModel = _companyRepository.Find(command.Id);
			if (command.IsCompanySiteTitleReset) {
				companyModel.CompanySiteTitle = "OnRamp Training";
				_companyRepository.SaveChanges();
				return null;
			}

			if (companyModel != null)
            {
				companyModel.CompanySiteTitle = command.CompanySiteTitle;
				companyModel.IsEmployeeCodeReq = command.IsEmployeeCodeReq;
				companyModel.IsEnabledEmployeeCode = command.IsEnabledEmployeeCode;
				companyModel.IsForSelfSignUp = command.IsForSelfSignUp;
				companyModel.IsSelfSignUpApprove = command.IsSelfSignUpApprove;
                companyModel.DefaultUserExpireDays = command.DefaultUserExpireDays;
                companyModel.ShowCompanyNameOnDashboard = command.ShowCompanyNameOnDashboard;
                companyModel.HideDashboardLogo = command.HideDashboardLogo;
                if (command.LegalDisclaimerActivationType.HasValue)
                {
                    if (command.LegalDisclaimerActivationType == LegalDisclaimerActivationType.ShowOnLoginOnce &&
                        (companyModel.LegalDisclaimer != command.LegalDisclaimer ||
                         companyModel.ShowLegalDisclaimerOnLogin ||
                         (!companyModel.ShowLegalDisclaimerOnLogin &&
                          !companyModel.ShowLegalDisclaimerOnLoginOnlyOnce)))
                    {
                        // Mark previous disclaimer activity as deleted.
                        _commandDispatcher.Dispatch(new DeleteDisclaimerActivityLogsCommand
                        {
                            CompanyId = command.Id
                        });
                    }

                    switch (command.LegalDisclaimerActivationType)
                    {
                        case LegalDisclaimerActivationType.ShowOnLogin:
                            companyModel.ShowLegalDisclaimerOnLogin = true;
                            companyModel.ShowLegalDisclaimerOnLoginOnlyOnce = false;
                            break;
                        case LegalDisclaimerActivationType.ShowOnLoginOnce:
                            companyModel.ShowLegalDisclaimerOnLoginOnlyOnce = true;
                            companyModel.ShowLegalDisclaimerOnLogin = false;
                            break;
                        case LegalDisclaimerActivationType.Disabled:
                            companyModel.ShowLegalDisclaimerOnLoginOnlyOnce = false;
                            companyModel.ShowLegalDisclaimerOnLogin = false;
                            break;
                        default:
                            break;
                    }

                    companyModel.LegalDisclaimer = command.LegalDisclaimer;
                }

                if (!string.IsNullOrEmpty(command.DashboardVideoFileId))
                {
                    var upload = _uploadRepository.Find(command.DashboardVideoFileId.AsGuid());
                    if (upload != null)
                        companyModel.DashboardVideoFile = upload;
                }
                else
                {
                    var remove = companyModel.DashboardVideoFile; // hack
                    companyModel.DashboardVideoFile = null;
                }

                companyModel.DashboardVideoTitle = command.DashboardVideoTitle;
                companyModel.DashboardVideoDescription = command.DashboardVideoDescription;
                companyModel.DashboardQuoteAuthor = command.DashboardQuoteAuthor;
                companyModel.DashboardQuoteText = command.DashboardQuoteText;

                _companyRepository.SaveChanges();
            }

            return null;
        }
    }
}