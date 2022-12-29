using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.Portal;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.IconSet;
using Ramp.Contracts.QueryParameter.Settings;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.CustomerManagement {
	public class CustomerCompanyQueryHandler : QueryHandlerBase<CustomerCompanyQueryParameter, CompanyViewModelLong>,
		IQueryHandler<FetchByIdQuery, string> {
		private readonly IRepository<Company> _companyRepository;
		private readonly IRepository<Package> _packageRepository;
		private readonly IRepository<Bundle> _bundleRepository;
		private readonly ICommandDispatcher _dispactcher;
		private readonly IQueryExecutor _executor;
		public CustomerCompanyQueryHandler(
			IRepository<Company> companyRepository,
			IRepository<Package> packageRepository,
			IRepository<Bundle> bundleRepository,
			ICommandDispatcher dispactcher,
			IQueryExecutor executor)
		{
			_companyRepository = companyRepository;
			_packageRepository = packageRepository;
			_bundleRepository = bundleRepository;
			_dispactcher = dispactcher;
			_executor = executor;
		}
		public string ExecuteQuery(FetchByIdQuery query)
		{
			var id = Guid.Parse(query.Id.ToString());
			return _companyRepository.List.AsQueryable().Where(x => x.Id == id).FirstOrDefault().CompanySiteTitle;
		}
		public override CompanyViewModelLong ExecuteQuery(CustomerCompanyQueryParameter queryParameters)
		{
			var customerCompanyViewModel = new CompanyViewModelLong();

			if (queryParameters.Id != null && queryParameters.Id != Guid.Empty)
			{
				Company companyModel = _companyRepository.Find(queryParameters.Id);
				if (companyModel != null)
				{
					customerCompanyViewModel.CompanyViewModel = new CompanyViewModel
					{
						Id = companyModel.Id,
						CompanyCreatedBy = companyModel.CreatedBy,
						ClientSystemName = companyModel.ClientSystemName,
						CompanyName = companyModel.CompanyName,
						LayerSubDomain = companyModel.LayerSubDomain,
						PhysicalAddress = companyModel.PhysicalAddress,
						PostalAddress = companyModel.PostalAddress,
						SelectedProvisionalAccountLink = companyModel.ProvisionalAccountLink,
						ProvisionalAccountLink = companyModel.ProvisionalAccountLink,
						TelephoneNumber = companyModel.TelephoneNumber,
						WebsiteAddress = companyModel.WebsiteAddress,
						CompanyConnectionString = companyModel.CompanyConnectionString,
						LogoImageUrl = companyModel.LogoImageUrl,
						//SelectedPackage = companyModel.PackageId,
						SelectedBundle = companyModel.BundleId,
						IsChangePasswordFirstLogin = companyModel.IsChangePasswordFirstLogin,
						IsSendWelcomeSMS = companyModel.IsSendWelcomeSMS,
						IsLock = companyModel.IsLock,
						IsForSelfSignUp = companyModel.IsForSelfSignUp,
						IsSelfSignUpApprove = companyModel.IsSelfSignUpApprove,
						DefaultUserExpireDays = companyModel.DefaultUserExpireDays,
						ShowCompanyNameOnDashboard = companyModel.ShowCompanyNameOnDashboard,
						EnableTrainingActivityLoggingModule = companyModel.EnableTrainingActivityLoggingModule,
						EnableRaceCode = companyModel.EnableRaceCode,
						EnableChecklistDocument = companyModel.EnableChecklistDocument,
						ShowCompanyLogoOnDashboard = !companyModel.HideDashboardLogo,
						EnableCategoryTree = companyModel.EnableCategoryTree,
						EnableGlobalAccessDocuments = companyModel.EnableGlobalAccessDocuments,
						EnableVirtualClassRoom = companyModel.EnableVirtualClassRoom,
						JitsiServerName = companyModel.JitsiServerName,

						ActiveDirectoryEnabled = companyModel.ActiveDirectoryEnabled,
						Domain = companyModel.Domain,
						Port = companyModel.Port,
						UserName = companyModel.UserName,
						Password = companyModel.Password
					};
					// {
					// Expiry Date
					if (companyModel.ExpiryDate != null)
					{
						customerCompanyViewModel.CompanyViewModel.ExpiryDate = Convert.ToDateTime(companyModel.ExpiryDate);
					}
					else
					{
						//  check is provisional then set 1 month as expiry

						if (companyModel.IsSelfCustomer == true)
						{
							// set Expiry date 1 month

							DateTime createdDate = companyModel.CreatedOn;
							DateTime expiryDate = createdDate.AddMonths(1);
							customerCompanyViewModel.CompanyViewModel.ExpiryDate = Convert.ToDateTime(expiryDate);
						}
						else
						{
							// set Expiry date 1 year
							DateTime createdDate = companyModel.CreatedOn;
							DateTime expiryDate = createdDate.AddYears(1);
							customerCompanyViewModel.CompanyViewModel.ExpiryDate = Convert.ToDateTime(expiryDate);
						}
					}

					// Auto Expire

					if (companyModel.AutoExpire != null)
					{
						customerCompanyViewModel.CompanyViewModel.AutoExpire = (bool)companyModel.AutoExpire;
					}
					else
					{
						if (companyModel.IsSelfCustomer == true)
						{
							//  company is Monthly auto expire false

							customerCompanyViewModel.CompanyViewModel.AutoExpire = true;
						}
						else
						{
							// company is yearly auto expire false

							customerCompanyViewModel.CompanyViewModel.AutoExpire = false;
						}
					}

					// is Yearly
					if ((companyModel.YearlySubscription) != null)
					{
						if (companyModel.YearlySubscription == true)
						{
							customerCompanyViewModel.CompanyViewModel.IsYearly = "1";
						}
						else
						{
							customerCompanyViewModel.CompanyViewModel.IsYearly = "0";
						}
					}
					else
					{
						if (companyModel.IsSelfCustomer == true)
						{
							//  company is Monthly auto expire false

							customerCompanyViewModel.CompanyViewModel.IsYearly = "0";
							companyModel.YearlySubscription = false;
						}
						else
						{
							// company is yearly auto expire false

							customerCompanyViewModel.CompanyViewModel.IsYearly = "1";
							companyModel.YearlySubscription = true;
						}
					}
					//_dispactcher.Dispatch(new OverridePortalContextCommand { CompanyId = companyModel.Id });
					customerCompanyViewModel.CompanyViewModel.CustomerConfigurations = companyModel.CustomerConfigurations.AsQueryable().Select(Project.ToCutomerConfigurationModel).ToList();
					customerCompanyViewModel.CompanyViewModel.IconSets = _executor.Execute<IconSetListQuery, IEnumerable<IconSetModel>>(new IconSetListQuery()).Select(x => new System.Web.Mvc.SelectListItem { Text = x.Name, Value = x.Id });
					if (companyModel.IconSet != null)
					{
						customerCompanyViewModel.CompanyViewModel.IconSet = Project.ToIconSetModel.Compile().Invoke(companyModel.IconSet);
						customerCompanyViewModel.CompanyViewModel.SelectedIconSet = companyModel.IconSet.Id.ToString();
					}
				}
			}
			else
			{
				DateTime currentDate = DateTime.Now;
				DateTime expirydate = currentDate.AddYears(1);

				customerCompanyViewModel.CompanyViewModel = new CompanyViewModel();
				customerCompanyViewModel.CompanyViewModel.ExpiryDate = expirydate;
			}

			IEnumerable<Company> companyList;
			if (queryParameters.IsForAdmin)
			{
				companyList =
			   _companyRepository.List
				   .Where(a => a.PhysicalAddress != "Dummy")
				   .Where(x => x.CompanyType == CompanyType.CustomerCompany);
			}
			else
			{
				companyList =
				_companyRepository.List
					.Where(x => x.CompanyType == CompanyType.CustomerCompany
						&& x.ProvisionalAccountLink == queryParameters.ProvisionalCompanyId);
			}

			foreach (Company company in companyList.OrderBy(c => c.CompanyName))
			{
				var companyModel = new CompanyViewModel
				{
					Id = company.Id,
					Status = company.IsActive,
					CompanyName = company.CompanyName,
					ClientSystemName = company.ClientSystemName,
					LayerSubDomain = company.LayerSubDomain,
					PhysicalAddress = company.PhysicalAddress,
					PostalAddress = company.PostalAddress,
					SelectedProvisionalAccountLink = company.ProvisionalAccountLink,
					CompanyCreatedBy = company.CreatedBy,
					TelephoneNumber = company.TelephoneNumber,
					WebsiteAddress = company.WebsiteAddress,
					CompanyConnectionString = company.CompanyConnectionString,
					LogoImageUrl = company.LogoImageUrl,
					//SelectedPackage = company.PackageId,
					//PackageName = _packageRepository.Find(company.PackageId).Title,
					SelectedBundle = company.BundleId,
					BundleName = _bundleRepository.Find(company.BundleId) != null ? _bundleRepository.Find(company.BundleId).Title : "",
					IsChangePasswordFirstLogin = company.IsChangePasswordFirstLogin,
					IsSendWelcomeSMS = company.IsSendWelcomeSMS,
					IsLock = company.IsLock,
					IsForSelfSignUp = company.IsForSelfSignUp,
					EnableChecklistDocument = company.EnableChecklistDocument,
					EnableCategoryTree = company.EnableCategoryTree,
					EnableGlobalAccessDocuments = company.EnableGlobalAccessDocuments,
					EnableVirtualClassRoom = company.EnableVirtualClassRoom,
					JitsiServerName = company.JitsiServerName,
					ActiveDirectoryEnabled = company.ActiveDirectoryEnabled,
					IsSelfSignUpApprove = company.IsSelfSignUpApprove,
				};


				if (company.ProvisionalAccountLink != Guid.Empty)
				{
					companyModel.ProvisionalAccountName =
						_companyRepository.Find(company.ProvisionalAccountLink).CompanyName;
				}
				customerCompanyViewModel.CompanyList.Add(companyModel);
			}
			return customerCompanyViewModel;
		}
	}
}