using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using Newtonsoft.Json;
using Ramp.Contracts.CommandParameter.ActivityManagement;
using Ramp.Contracts.CommandParameter.Login;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.Login;
using Ramp.Contracts.ViewModel;
using Ramp.Security.Authorization;
using Ramp.Services.Helpers;
using Ramp.Services.Implementations;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Web.UI.Code.Extensions;
using Role = Ramp.Contracts.Security.Role;

namespace Web.UI.Code.Handlers
{
    public class LoginUserCommandHandler : ICommandHandlerBase<LoginUserCommandParameter>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<Company> _companyRepository;
        private EncryptionHelper _encryptionHelper;

        public LoginUserCommandHandler(IRepository<User> userRepository,
            IRepository<StandardUser> standardUserRepository, IRepository<Company> companyRepository)
        {
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
            _companyRepository = companyRepository;
            _encryptionHelper = new EncryptionHelper();
        }

        public CommandResponse Execute(LoginUserCommandParameter command)
        {
            User user = null;
            StandardUser standardUser = null;
            user = _userRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(command.Email.TrimAllCastToLowerInvariant()));
            if (!(PortalContext.Current.UserCompany.CompanyCreatedBy.Equals(Guid.Empty) && PortalContext.Current.UserCompany.ProvisionalAccountLink.Equals(Guid.Empty)) || PortalContext.Current.Type != null)
                standardUser =
                        _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(command.Email.TrimAllCastToLowerInvariant()));
            var userModel = standardUser != null
                ? Project.UserViewModelFrom(standardUser)
                : Project.UserViewModelFrom(user);
			PortalContext.Current.UserDetail = userModel;
            var company = _companyRepository.Find(userModel.CompanyId);
            if (user != null)
            {
                PortalContext.Override(company.Id);
            }
            SessionManager.SetSessionInformation(null);
            var userLoginStatsId = Guid.NewGuid();
            var sessionInformation = new SessionInformation()
            {
                UserHasAcceptedDisclaimer = userModel.UserDisclaimerActivity.Any(x => !x.Deleted && x.Accepted)
            };

            SessionManager.SetSessionInformation(sessionInformation);

            new CommandDispatcher().Dispatch(new AddUserActivityCommand()
            {
                ActivityDate = DateTime.Now,
                ActivityDescription = "User Login",
                ActivityType = UserActivityEnum.Login,
                CurrentUserId = userModel.Id
            });
            var userLoginStatsNew = new AddUserLoginStatsCommand
            {
                UserLoginStatsId = userLoginStatsId,
                IsUserLoggedIn = false,
                LoggedInUserId = userModel.Id,
                LogInTime = DateTime.Now,
                StandardUser = standardUser != null
            };
            new CommandDispatcher().Dispatch(userLoginStatsNew);

            if (userModel.CustomerRoles.Contains(Role.StandardUser))
            {
                var surveyModel =
                    new QueryExecutor()
                        .Execute<LatestSurveyByCustomerUserQueryParameter, CustomerSurveyDetailViewModel>(new LatestSurveyByCustomerUserQueryParameter
                        {
                            CurrentUserId = userModel.Id
                        });

                if (surveyModel != null && surveyModel.UserId != Guid.Empty)
                {
                    double monthDiff = Math.Round((DateTime.Now.Subtract(surveyModel.RatedOn).TotalDays / 30), 2);
                    if (monthDiff > (double)4.00)
                    {
                        HttpContext.Current.Session["ShowSurveyModal"] = false;
                    }
                }
            }
            return null;
        }
    }
}