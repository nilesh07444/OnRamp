using Common;
using Common.Command;
using Common.Data;
using Common.Events;
using Common.Query;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.Portal;
using Ramp.Contracts.Events;
using Ramp.Contracts.QueryParameter.CorrespondenceManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using Data.EF.Customer;
using Role = Ramp.Contracts.Security.Role;
using Domain.Customer.Models.Groups;
using StandardUserGroup = Domain.Customer.Models.Groups.StandardUserGroup;

namespace Ramp.Services.QueryHandler.CorrespondenceManagement
{
    public class AllUserCorrespondenceInTimeSpanQueryHandler :
        QueryHandlerBase<AllUserCorrespondenceInTimeSpanQueryParameter, List<UserCorrespondenceLogViewModel>>
    {
		private readonly IRepository<StandardUserGroup> _standardUserGroupRepository;

		private readonly IRepository<UserCorrespondenceLog> _userCorrespondenceLogRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly ITransientRepository<StandardUser> _standardUserTransientRepository;

        public AllUserCorrespondenceInTimeSpanQueryHandler(
            IRepository<UserCorrespondenceLog> userCorrespondenceLogRepository,
            IRepository<Company> companyRepository,
			IRepository<StandardUserGroup> standardUserGroupRepository,

			ITransientRepository<StandardUser> standardUserTransientRepository)
        {
			_standardUserGroupRepository = standardUserGroupRepository;

			_userCorrespondenceLogRepository = userCorrespondenceLogRepository;
            _companyRepository = companyRepository;
            _standardUserTransientRepository = standardUserTransientRepository;
        }

        public override List<UserCorrespondenceLogViewModel> ExecuteQuery(
            AllUserCorrespondenceInTimeSpanQueryParameter queryParameters)
        {
            var userCorrespondenceList = new List<UserCorrespondenceLogViewModel>();

            var correspondance = new List<StandardUserCorrespondanceLog>();
            var resellerCorrespondance = new List<UserCorrespondenceLog>();

            if (queryParameters.SelectedOption.Equals(AllUserCorrespondenceInTimeSpanQueryParameter.ProvisionalUsers))
            {
                if (!queryParameters.UserId.Equals(Guid.Empty))
                {
                    if (_userCorrespondenceLogRepository.List.Any(entry => entry.UserId.Equals(queryParameters.UserId)))
                        resellerCorrespondance.AddRange(
                            _userCorrespondenceLogRepository.List.Where(
                                entry => entry.UserId.Equals(queryParameters.UserId)));
                }
                else if(
                    _userCorrespondenceLogRepository.List.Any(
                        entry => entry.User.CompanyId.Equals(queryParameters.ProvisionalCompanyId)))
                    resellerCorrespondance.AddRange(
                        _userCorrespondenceLogRepository.List.Where(
                            entry => entry.User.CompanyId.Equals(queryParameters.ProvisionalCompanyId)));
            }
            else if (queryParameters.SelectedOption.Equals(AllUserCorrespondenceInTimeSpanQueryParameter.CompanyUsers))
            {
                if (!queryParameters.ProvisionalCompanyId.Equals(Guid.Empty) &&
                    queryParameters.CompanyId.Equals(Guid.Empty))
                {
                    var companies =
                        _companyRepository.List.Where(
                            c => c.ProvisionalAccountLink.Equals(queryParameters.ProvisionalCompanyId)).ToList();

                    foreach (var company in companies)
                    {
                        new CommandDispatcher().Dispatch(new OverridePortalContextCommand { CompanyId = company.Id });
                        var entries =
                            new QueryExecutor()
                                .Execute<AllUserCorrespondanceForCompanyQuery, List<StandardUserCorrespondanceLog>>(
                                    new AllUserCorrespondanceForCompanyQuery());
                        entries.ForEach(entry => correspondance.Add(entry));
                    }
                }
                else if (!queryParameters.ProvisionalCompanyId.Equals(Guid.Empty) &&
                         !queryParameters.CompanyId.Equals(Guid.Empty) && queryParameters.UserId.Equals(Guid.Empty))
                {
                    new CommandDispatcher().Dispatch(new OverridePortalContextCommand { CompanyId = queryParameters.CompanyId });
                    var entries =
                        new QueryExecutor()
                            .Execute<AllUserCorrespondanceForCompanyQuery, List<StandardUserCorrespondanceLog>>(
                                new AllUserCorrespondanceForCompanyQuery());
                    entries.ForEach(entry => correspondance.Add(entry));
                }
                else if (!queryParameters.ProvisionalCompanyId.Equals(Guid.Empty) &&
                         !queryParameters.CompanyId.Equals(Guid.Empty) && !queryParameters.UserId.Equals(Guid.Empty) && queryParameters.GroupId.Equals(Guid.Empty))
                {
                    new CommandDispatcher().Dispatch(new OverridePortalContextCommand { CompanyId = queryParameters.CompanyId });
                    var entries =
                        new QueryExecutor()
                            .Execute<AllUserCorrespondanceForCompanyQuery, List<StandardUserCorrespondanceLog>>(
                                new AllUserCorrespondanceForCompanyQuery());
                    var userEntries = entries.Where(u => u.UserId.Equals(queryParameters.UserId));
                    correspondance.AddRange(userEntries);
                }
                else if (queryParameters.ProvisionalCompanyId.Equals(Guid.Empty) &&
                         !queryParameters.CompanyId.Equals(Guid.Empty) && queryParameters.UserId.Equals(Guid.Empty) && !queryParameters.GroupId.Equals(Guid.Empty))
                {
                    _standardUserTransientRepository.SetCustomerCompany(queryParameters.CompanyId.ToString());
                    var userIds = _standardUserTransientRepository.List
                        .Where(u => u.Group?.Id == queryParameters.GroupId).Select(u => u.Id).ToList();
                    new CommandDispatcher().Dispatch(new OverridePortalContextCommand { CompanyId = queryParameters.CompanyId });
                    var entries =
                        new QueryExecutor()
                            .Execute<AllUserCorrespondanceForCompanyQuery, List<StandardUserCorrespondanceLog>>(
                                new AllUserCorrespondanceForCompanyQuery());
                    var userEntries = entries.Where(u => userIds.Any(id => id == u.UserId));
                    correspondance.AddRange(userEntries);
                }
                else if (!queryParameters.CompanyId.Equals(Guid.Empty) && queryParameters.UserId.Equals(Guid.Empty) && queryParameters.GroupId.Equals(Guid.Empty) &&
                         queryParameters.ProvisionalCompanyId.Equals(Guid.Empty))
                {
                    var entries =
                         new QueryExecutor()
                             .Execute<AllUserCorrespondanceForCompanyQuery, List<StandardUserCorrespondanceLog>>(
                                 new AllUserCorrespondanceForCompanyQuery());
                    entries.ForEach(entry => correspondance.Add(entry));
                }
                else if (!queryParameters.CompanyId.Equals(Guid.Empty) && !queryParameters.UserId.Equals(Guid.Empty) &&
                         queryParameters.ProvisionalCompanyId.Equals(Guid.Empty))
                {
                    var entries =
                       new QueryExecutor()
                           .Execute<AllUserCorrespondanceForCompanyQuery, List<StandardUserCorrespondanceLog>>(
                               new AllUserCorrespondanceForCompanyQuery());
                    var userEntries = entries.Where(u => u.UserId.Equals(queryParameters.UserId));
                    correspondance.AddRange(userEntries);
                }
            }
            if (queryParameters.FromDate != default(DateTime))
            {
                queryParameters.FromDate = queryParameters.FromDate.AtBeginningOfDay();
                resellerCorrespondance =
                    resellerCorrespondance.Where(c => c.CorrespondenceDate >= queryParameters.FromDate).ToList();
                correspondance = correspondance.Where(c => c.CorrespondenceDate >= queryParameters.FromDate).ToList();
            }
            if (queryParameters.ToDate != default(DateTime))
            {
                queryParameters.ToDate = queryParameters.ToDate.AtEndOfDay();
                resellerCorrespondance =
                    resellerCorrespondance.Where(c => c.CorrespondenceDate <= queryParameters.ToDate).ToList();
                correspondance = correspondance.Where(c => c.CorrespondenceDate <= queryParameters.ToDate).ToList();
            }
            resellerCorrespondance.ForEach(
                c => userCorrespondenceList.Add(Project.UserCorrespondenceLogViewModelFrom(c)));
            correspondance.ForEach(c => userCorrespondenceList.Add(Project.UserCorrespondenceLogViewModelFrom(c)));

			

			foreach(var u in userCorrespondenceList){
				var groupList = _standardUserGroupRepository.List.Where(c => c.UserId == u.UserId).ToList();

				string name = null;

				foreach(var g in groupList) {
					if (name != null)
						name = name + "," + g.GroupId;
					else name = name + g.GroupId;
				}

				u.UserViewModel.GroupName = name;

			}

            return userCorrespondenceList.OrderByDescending(c => c.CorrespondenceDate).ToList();
        }
    }
}