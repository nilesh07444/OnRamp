using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Ramp.Services.Projection;
using Common;
using Common.Command;
using Data.EF.Customer;
using Ramp.Contracts.Query.User;
using Ramp.Contracts.QueryParameter;
using System.Data.Entity;
using System.Linq.Expressions;
using Domain.Enums;
using Ramp.Services.Helpers;
using Ramp.Contracts.QueryParameter.CustomerRoles;
using Ramp.Contracts.QueryParameter.Group;
using Domain.Customer.Models.CustomRole;

using Role = Ramp.Contracts.Security.Role;

namespace Ramp.Services.QueryHandler
{
    public class UserQueryHandler : IQueryHandler<FindUserByIdQuery, UserModelShort>,
                                         IQueryHandler<FindRoleByIdQuery, CustomerRole>,
                                    IQueryHandler<FetchByCategoryIdQuery, List<CustomerRole>>,
                                    IQueryHandler<FindGroupByIdQuery, CustomerGroup>,
                                    IQueryHandler<FindUserByIdQuery, StandardUser>,
                                    IQueryHandler<FindUserByIdQuery, Domain.Models.User>,
                                    IQueryHandler<UserListQuery, IEnumerable<UserModelShort>>,
                                    IQueryHandler<UserSearchQuery, IEnumerable<UserModelShort>>,
                                    IQueryHandler<UserListQuery, IEnumerable<StandardUser>>,
                                    IQueryHandler<UsersAndGroupsQuery, IEnumerable<UserWithGroupModel>>,
                                    IQueryHandler<AllUsersQuery, IEnumerable<UserModelShort>>,
                                    IQueryHandler<StandardUsersQuery, IEnumerable<UserModelShort>>,
                                   IQueryHandler<UserQueryParameter, UserViewModel>,
                                    IQueryHandler<FetchAllRecordsQuery, IEnumerable<StandardUser>>,
                                    IQueryHandler<FetchAllScheduleReportQuery, IEnumerable<StandardUser>>,
                                    IQueryHandler<StandardUsersQuery, IEnumerable<UserViewModel>>
    {

        private readonly IRepository<Domain.Models.User> _userRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly ITransientRepository<StandardUser> _standardUserTransientRepository;
        private readonly IQueryExecutor _executor;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IRepository<CustomerGroup> _customerGroupRepository;
        private readonly IRepository<CustomUserRoles> _customUserRolesRepository;
        public UserQueryHandler(
            IRepository<Domain.Models.User> userRepository,
            IRepository<StandardUser> standardUserRepository,
            ITransientRepository<StandardUser> standardUserTransientRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<CustomerGroup> customerGroupRepository,

            IRepository<CustomUserRoles> customUserRolesRepository,
            IQueryExecutor executor,
            ICommandDispatcher commandDispatcher)
        {
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
            _standardUserTransientRepository = standardUserTransientRepository;
            _executor = executor;
            _commandDispatcher = commandDispatcher;
            _customerGroupRepository = customerGroupRepository;
            _customerRoleRepository = customerRoleRepository;
            _customUserRolesRepository = customUserRolesRepository;

        }

        public UserViewModel ExecuteQuery(UserQueryParameter queryParameters)
        {

            var userModel = _standardUserRepository.Find(queryParameters.UserId);
            var userViewModel = new UserViewModel();
            if (userModel != null)
            {

                userViewModel.Id = userModel.Id;
                userViewModel.EmailAddress = userModel.EmailAddress;
                userViewModel.FirstName = userModel.FirstName;
                userViewModel.LastName = userModel.LastName;
                userViewModel.FullName = $"{userModel.FirstName} {userModel.LastName}";
                userViewModel.MobileNumber = userModel.MobileNumber;
                userViewModel.Status = !userModel.IsUserExpire && userModel.IsActive;
                userViewModel.Password = new EncryptionHelper().Decrypt(userModel.Password);
                userViewModel.ConfirmPassword = new EncryptionHelper().Decrypt(userModel.Password);
                userViewModel.ParentUserId = userModel.ParentUserId;
                userViewModel.ExpireDays = userModel.ExpireDays;
                userViewModel.IsUserExpire = userModel.IsUserExpire;
                userViewModel.EmployeeNo = userModel.EmployeeNo;
                userViewModel.IDNumber = userModel.IDNumber;
                userViewModel.Gender = GenderEnum.GetDescription(userModel.Gender);
                userViewModel.RaceCodeId = userModel.RaceCodeId;
                userViewModel.IsActive = userModel.IsActive;
                userViewModel.SelectedCustomerType = userModel.Roles.FirstOrDefault().RoleName;
                userViewModel.Roles = userModel.Roles.Select(c => new RoleViewModel { Description = c.Description, RoleName = c.RoleName, RoleId = c.Id }).ToList();
                if (userModel.Group != null)
                {
                    userViewModel.SelectedGroupId = userModel.Group.Id;
                }
                userViewModel.TrainingLabels = userModel.TrainingLabels;
            }

            _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

            return userViewModel;
        }


        public UserModelShort ExecuteQuery(FindUserByIdQuery queryParameters)
        {
            var user = _userRepository.Find(queryParameters.Id.ConvertToGuid());
            var id = queryParameters.Id.ConvertToGuid();
            var standardUser = _standardUserRepository.List.AsQueryable().FirstOrDefault(x => x.Id == id);
            if (user != null)
                return Project.UserToUserModelShort.Compile().Invoke(user);
            if (standardUser != null)
                return Project.StandardUserToUserModelShort.Compile().Invoke(standardUser);
            return null;
        }

        public CustomerRole ExecuteQuery(FindRoleByIdQuery queryParameters)
        {
            var role = _customerRoleRepository.Find(queryParameters.Id.ConvertToGuid());
            return role;
        }
        public List<CustomerRole> ExecuteQuery(FetchByCategoryIdQuery queryParameters)
        {
            if (queryParameters.Id == null)
            {
                var role = _customerRoleRepository.List.ToList();
                return role;

            }
            return new List<CustomerRole>();

        }


        public CustomerGroup ExecuteQuery(FindGroupByIdQuery queryParameters)
        {
            var group = _customerGroupRepository.Find(queryParameters.Id.ConvertToGuid());
            return group;
        }

        StandardUser IQueryHandler<FindUserByIdQuery, StandardUser>.ExecuteQuery(FindUserByIdQuery query)
        {
            var id = query.Id.ConvertToGuid();
            var standardUser = _standardUserRepository.List.AsQueryable().FirstOrDefault(x => x.Id == id);
            return standardUser;
        }

        Domain.Models.User IQueryHandler<FindUserByIdQuery, Domain.Models.User>.ExecuteQuery(FindUserByIdQuery query)
        {
            return _userRepository.Find(query.Id.ConvertToGuid());
        }

        public IEnumerable<UserModelShort> ExecuteQuery(UserListQuery query)
        {
            var users = new List<UserModelShort>();
            foreach (var id in query.Ids)
            {
                var u = _executor.Execute<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery { Id = id });
                if (u != null)
                    users.Add(u);
            }

            return users;
        }
        public IEnumerable<UserModelShort> ExecuteQuery(UserSearchQuery query)
        {
            return _standardUserRepository.List.AsQueryable().Select(Project.StandardUserToUserModelShort_Firstname_LastNames).OrderBy(x => x.Name).ToList();
        }

        IEnumerable<StandardUser> IQueryHandler<UserListQuery, IEnumerable<StandardUser>>.ExecuteQuery(UserListQuery query)
        {
            return _standardUserRepository.List.AsQueryable().Where(x => query.Emails.Contains(x.EmailAddress));
        }

        public IEnumerable<UserWithGroupModel> ExecuteQuery(UsersAndGroupsQuery query)
        {
            return _standardUserRepository.List
                .Where(u => u.Roles.Any(r => r.RoleName.Equals(Ramp.Contracts.Security.Role.StandardUser))).Select(u =>
                    new UserWithGroupModel
                    {
                        Id = u.Id.ToString(),
                        Name = u.FirstName + " " + u.LastName,
                        //GroupId = u.Group.Id.ToString(),
                        //GroupTitle = u.Group.Title
                    });
        }

        public IEnumerable<UserModelShort> ExecuteQuery(AllUsersQuery query)
        {
            _standardUserTransientRepository.SetCustomerCompany(query.CompanyId);

            var users = _standardUserTransientRepository.List.OrderBy(u => u.FirstName).AsQueryable()
                .Select(Project.StandardUserToUserModelShort_Firstname_LastNames);

            _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

            return users;
        }

        public IEnumerable<UserModelShort> ExecuteQuery(StandardUsersQuery query)
        {

            //var xy = _customerRoleRepository.List.ToList();
            //var t = _customUserRolesRepository.List.ToList();
            //var rr = _standardUserRepository.List.ToList().OrderByDescending(x=> x.CreatedOn);


            var queryable = _standardUserRepository.List.AsQueryable().Include(x => x.Roles).Where(x => x.Roles.Any(r => r.RoleName == Ramp.Contracts.Security.Role.StandardUser));
            if (query.Ids != null && query.Ids.Any())
            {
                var gIds = new List<Guid>();
                query.Ids.ToList().ForEach(x =>
                {
                    if (Guid.TryParse(x, out var g))
                        gIds.Add(g);
                });
                queryable.Where(x => gIds.Contains(x.Id));
            }
            return queryable.Select(Project.StandardUserToUserModelShort_Firstname_LastNames).OrderBy(u => u.Name).ToList();
        }

        //neeraj
        public IEnumerable<StandardUser> ExecuteQuery(FetchAllRecordsQuery query)
        {
            List<StandardUser> result = new List<StandardUser>();

            var users = _standardUserRepository.List.ToList().OrderByDescending(x => x.CreatedOn);
            foreach (var user in users)
            {
                foreach (var role in user.Roles)
                {
                    if (role.RoleName == Role.ContentApprover)
                    {
                        result.Add(user);
                    }
                }
            }

            return result;

        }

        public IEnumerable<StandardUser> ExecuteQuery(FetchAllScheduleReportQuery query)
        {
            List<StandardUser> result = new List<StandardUser>();

            var users = _standardUserRepository.List.ToList().OrderByDescending(x => x.CreatedOn);


            return users;

        }

        IEnumerable<UserViewModel> IQueryHandler<StandardUsersQuery, IEnumerable<UserViewModel>>.ExecuteQuery(StandardUsersQuery query)
        {
            var queryable = _standardUserRepository.List.AsQueryable().Include(x => x.Group).Include(x => x.Roles).Where(x => x.Roles.Any(r => r.RoleName == Ramp.Contracts.Security.Role.StandardUser));
            if (query.GroupIds != null && query.GroupIds.Any())
                queryable = queryable.Where(x => x.Group != null && query.GroupIds.Contains(x.Group.Id.ToString()));
            if (query.Ids != null && query.Ids.Any())
                queryable = queryable.Where(x => query.Ids.Contains(x.Id.ToString()));
            if (query.TagNames != null && query.TagNames.Any())
            {
                List<StandardUser> Users = new List<StandardUser>();
                queryable = queryable.Where(x => x.IsActive);
                foreach (var userList in queryable)
                {
                    if (userList.TrainingLabels != null)
                    {
                        foreach (var selectedTags in query.TagNames)
                        {
                            if (userList.TrainingLabels.Contains(','))
                            {
                                var userTag = userList.TrainingLabels.Split(',');
                                foreach (var tag in userTag)
                                {
                                    if (tag.Equals(selectedTags))
                                    {
                                        Users.Add(userList);
                                        break;
                                    }
                                }
                            }
                            else if (userList.TrainingLabels.Equals(selectedTags))
                            {
                                Users.Add(userList);
                                break;
                            }
                        }
                    }
                }
                queryable = Users.Distinct().ToList().AsQueryable();
            }
            return queryable.Where(x => x.IsActive).Select(Project.StandardUser_UserViewModel).OrderBy(u => u.FullName).ToList();
        }


    }
}
namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<StandardUser, UserViewModel>> StandardUser_UserViewModel =
            x => new UserViewModel
            {
                Id = x.Id,
                EmailAddress = x.EmailAddress,
                EmployeeNo = x.EmployeeNo,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Gender = x.Gender.HasValue ? x.Gender.ToString() : null,
                ContactNumber = x.MobileNumber,
                FullName = !(x.FirstName == null || x.FirstName == "") && !(x.LastName == null || x.LastName == "") ? x.FirstName + " " + x.LastName : x.FirstName,
                SelectedGroupId = x.Group != null ? x.Group.Id : new Guid?(),
                GroupName = x.Group != null ? x.Group.Title : null,
                IDNumber = x.IDNumber
            };
    }
}
