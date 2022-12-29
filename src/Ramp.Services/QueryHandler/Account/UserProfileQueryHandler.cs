using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.Account;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.Account
{
    public class UserProfileQueryHandler : IQueryHandler<UserProfileQuery, EditUserProfileViewModel>
    {
        private readonly IRepository<Domain.Models.User> _userRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;

        public UserProfileQueryHandler(IRepository<Domain.Models.User> userRepository, IRepository<StandardUser> standardUserRepository)
        {
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
        }

        public EditUserProfileViewModel ExecuteQuery(UserProfileQuery query)
        {
            var profileViewModel = new EditUserProfileViewModel();
            var user = _userRepository.Find(query.UserId);
            var standardUser = _standardUserRepository.Find(query.UserId);
            if (user != null || standardUser != null)
            {
                if (user != null)
                {
                    profileViewModel = Project.EditUserProfileViewModelFrom(user);
                    if (user.GroupId.HasValue)
                    {
                        var groupViewModelList =
                             new QueryExecutor().Execute<AllGroupsByCustomerGroupIdParameter, List<GroupViewModel>>
                             (
                                 new AllGroupsByCustomerGroupIdParameter
                                 {
                                     CustomerId = user.Id,
                                     GroupId = user.GroupId.Value
                                 });

                        profileViewModel.DropDownForGroup =
                            groupViewModelList.Select(c => new SerializableSelectListItem
                            {
                                Text = c.Title,
                                Value = c.GroupId.ToString()
                            });
                    }
                }
                else
                {
                    profileViewModel = Project.EditUserProfileViewModelFrom(standardUser);
                    if (standardUser.Group != null)
                    {
                        var groupViewModelList =
                            new QueryExecutor().Execute<AllGroupsByCustomerGroupIdParameter, List<GroupViewModel>>
                                (
                                    new AllGroupsByCustomerGroupIdParameter
                                    {
                                        CustomerId = standardUser.Id,
                                        GroupId = standardUser.Group.Id
                                    });

                        profileViewModel.DropDownForGroup =
                            groupViewModelList.Select(c => new SerializableSelectListItem
                            {
                                Text = c.Title,
                                Value = c.GroupId.ToString()
                            });
                    }
                }
                return profileViewModel;
            }
            return null;
        }
    }
}