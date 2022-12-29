using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Query;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.Security;


namespace Ramp.Services.QueryHandler.TestManagement
{
    public class AssignTrainingTestQueryHandler : IQueryHandler<AssignTrainingTestQuery, AssignTrainingTestToUsersOrGroupsViewModel>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        public AssignTrainingTestQueryHandler(IRepository<TrainingTest> trainingTestRepository,
            IRepository<TestAssigned> assignedTestRepository,
            IRepository<StandardUser> userRepository,
            IRepository<CustomerGroup> groupRepository)
        {
            _trainingTestRepository = trainingTestRepository;
            _assignedTestRepository = assignedTestRepository;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
        }
        public AssignTrainingTestToUsersOrGroupsViewModel ExecuteQuery(AssignTrainingTestQuery query)
        {
            var vm = new AssignTrainingTestToUsersOrGroupsViewModel();
            vm.Users = _userRepository.List.Where(x => x.Roles.Any(r => r.RoleName.Equals(Role.StandardUser) && x.Group != null) && x.IsActive && !x.IsUserExpire)
                .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                .Select(x => new UserViewModelShort
                {
                    FullName = $"{x.FirstName} {x.LastName}",
                    GroupId = x.Group.Id,
                    GroupName = x.Group.Title,
                    Id = x.Id
                });
            vm.Groups = _groupRepository.List.OrderBy(x => x.Title).Select(x => new GroupViewModelShort
            {
                Id = x.Id,
                Name = x.Title
            });
            vm.TrainingTests = _trainingTestRepository.List.Where(x => x.ActiveStatus).OrderBy(x => x.TestTitle).Select(x => new TrainingTestAssignInfoViewModel
            {
                Id = x.Id,
                Title = x.TestTitle,
                AssignedUserIds = _assignedTestRepository.List.Where(a => a.TestId.Equals(x.Id) && a.UserId.HasValue).Select(a => a.UserId.Value)
            });
            return vm;
        }
    }
}
