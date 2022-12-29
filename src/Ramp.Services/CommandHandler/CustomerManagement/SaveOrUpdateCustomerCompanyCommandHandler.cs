using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.CommandParameter.Group;
using System;

namespace Ramp.Services.CommandHandler.CustomerManagement {
	public class SaveOrUpdateCustomerCompanyCommandHandler : CommandHandlerBase<SaveOrUpdateCustomerCompanyCommand> {
		private readonly IRepository<Company> _companyRepository;
		private readonly IRepository<User> _userRepository;
		private readonly IQueryExecutor _executor;
		private readonly IRepository<IconSet> _iconSetRepository;
		private readonly ICommandDispatcher _dispatcher;
		public SaveOrUpdateCustomerCompanyCommandHandler(IRepository<Company> companyRepository, IRepository<User> userRepository, IQueryExecutor executor, IRepository<IconSet> iconSetRepository, ICommandDispatcher dispacther)
		{
			_companyRepository = companyRepository;
			_userRepository = userRepository;
			_executor = executor;
			_iconSetRepository = iconSetRepository;
			_dispatcher = dispacther;
		}

		public override CommandResponse Execute(SaveOrUpdateCustomerCompanyCommand command)
		{
			try
			{
				Company companyModel = _companyRepository.Find(command.Id);
				if (companyModel == null)
				{
					var company = new Company
					{
						Id = Guid.NewGuid(),
						CompanyName = command.CompanyName,
						CreatedBy = command.CreatedBy,
						LayerSubDomain = command.LayerSubDomain,
						PhysicalAddress = command.PhysicalAddress,
						PostalAddress = command.PostalAddress,
						ClientSystemName = command.ClientSystemName,
						TelephoneNumber = command.TelephoneNumber,
						WebsiteAddress = command.WebsiteAddress,
						CompanyType = CompanyType.CustomerCompany,
						LogoImageUrl = command.LogoImageUrl,
						UserList = null,
						BundleId = command.SelectedBundle,
						CreatedOn = DateTime.Now,
						IsChangePasswordFirstLogin = command.IsChangePasswordFirstLogin,
						IsSendWelcomeSMS = command.IsSendWelcomeSMS,
						IsLock = false,
						IsSelfCustomer = false,
						YearlySubscription = command.YearlySubscription,
						AutoExpire = command.AutoExpire,
						ExpiryDate = command.ExpiryDate,
						DefaultUserExpireDays = command.DefaultUserExpireDays,
						ShowCompanyNameOnDashboard = command.ShowCompanyNameOnDashboard,
						EnableTrainingActivityLoggingModule = command.EnableTrainingActivityLoggingModule,
						EnableRaceCode = command.EnableRaceCode,
						EnableChecklistDocument = command.EnableChecklistDocument,
						TestExpiryNotificationInterval = NotificationInterval.OneDayBefore,
						EnableCategoryTree = command.EnableCategoryTree,
						EnableGlobalAccessDocuments = command.EnableGlobalAccessDocuments,
						EnableVirtualClassRoom = command.EnableVirtualClassRoom,
						JitsiServerName = command.JitsiServerName,

						ActiveDirectoryEnabled = command.ActiveDirectoryEnabled,
						Domain = command.Domain,
						Port = command.Port,
						UserName = command.UserName,
						Password = command.Password

					};

					if (command.SelectedProvisionalAccountLink != Guid.Empty)
					{
						company.ProvisionalAccountLink = command.SelectedProvisionalAccountLink;
					}
					else
					{
						var currentUser = _userRepository.Find(command.CurrentUserId);
						company.ProvisionalAccountLink = currentUser.CompanyId;
					}
					if (!string.IsNullOrWhiteSpace(command.IconSet))
					{
						Guid setId;
						if (Guid.TryParse(command.IconSet, out setId))
							company.IconSet = _iconSetRepository.Find(setId);
					}
					_companyRepository.Add(company);
					command.Id = company.Id;
					_companyRepository.SaveChanges();
				}
				else
				{
					companyModel.CreatedBy = command.CreatedBy;
					companyModel.CompanyName = command.CompanyName;
					companyModel.LayerSubDomain = command.LayerSubDomain;
					companyModel.PhysicalAddress = command.PhysicalAddress;
					companyModel.PostalAddress = command.PostalAddress;
					if (command.SelectedProvisionalAccountLink == Guid.Empty)
					{
						var currentUser = _userRepository.Find(command.CurrentUserId);
						companyModel.ProvisionalAccountLink = currentUser.CompanyId;
					}
					else
					{
						companyModel.ProvisionalAccountLink = command.SelectedProvisionalAccountLink;
					}
					companyModel.ClientSystemName = command.ClientSystemName;
					companyModel.ProvisionalAccountLink = command.SelectedProvisionalAccountLink;
					companyModel.TelephoneNumber = command.TelephoneNumber;
					companyModel.WebsiteAddress = command.WebsiteAddress;
					companyModel.BundleId = command.SelectedBundle;
					companyModel.IsChangePasswordFirstLogin = command.IsChangePasswordFirstLogin;
					companyModel.IsSendWelcomeSMS = command.IsSendWelcomeSMS;
					companyModel.IsLock = command.IsLock;
					companyModel.YearlySubscription = command.YearlySubscription;
					companyModel.AutoExpire = command.AutoExpire;
					companyModel.ExpiryDate = command.ExpiryDate;
					companyModel.DefaultUserExpireDays = command.DefaultUserExpireDays;
					companyModel.EnableChecklistDocument = command.EnableChecklistDocument;
					companyModel.EnableCategoryTree = command.EnableCategoryTree;
					companyModel.EnableGlobalAccessDocuments = command.EnableGlobalAccessDocuments;
					companyModel.EnableVirtualClassRoom = command.EnableVirtualClassRoom;
					companyModel.JitsiServerName = command.JitsiServerName;

					companyModel.ActiveDirectoryEnabled = command.ActiveDirectoryEnabled;
					companyModel.Domain = command.Domain;
					companyModel.Port = command.Port;
					companyModel.UserName = command.UserName;
					companyModel.Password = command.Password;

					if (String.IsNullOrEmpty(command.LogoImageUrl))
					{
						companyModel.LogoImageUrl = companyModel.LogoImageUrl;
					}
					else
					{
						if (command.LogoImageUrl == "clear")
							companyModel.LogoImageUrl = null;
						else
							companyModel.LogoImageUrl = command.LogoImageUrl;
					}
					if (companyModel.IsSelfCustomer == true)
					{
						companyModel.IsSelfCustomer = true;
					}
					else
					{
						companyModel.IsSelfCustomer = false;
					}
					companyModel.ShowCompanyNameOnDashboard = command.ShowCompanyNameOnDashboard;
					if (!string.IsNullOrWhiteSpace(command.IconSet))
					{
						Guid setId;
						if (Guid.TryParse(command.IconSet, out setId))
							companyModel.IconSet = _iconSetRepository.Find(setId);
					}
					companyModel.EnableTrainingActivityLoggingModule = command.EnableTrainingActivityLoggingModule;
					companyModel.EnableRaceCode = command.EnableRaceCode;
					//companyModel.TestExpiryNotificationInterval = command.TestExpiryNotificationInterval;
					//if (companyModel.TestExpiryNotificationInterval == NotificationInterval.Arbitrary)
					//    companyModel.ArbitraryTestExpiryIntervalInDaysBefore = command.ArbitraryTestExpiryIntervalInDaysBefore;
					//else
					//    companyModel.ArbitraryTestExpiryIntervalInDaysBefore = new int?();
					_companyRepository.SaveChanges();

				}
				return null;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}