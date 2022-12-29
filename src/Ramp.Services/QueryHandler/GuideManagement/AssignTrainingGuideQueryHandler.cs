using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.ViewModel;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.Security;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class AssignTrainingGuideQueryHandler : IQueryHandler<AssignTrainingGuideQuery, AssignTrainingGuideToUsersOrGroupsViewModel>
    {
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuidesRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        public AssignTrainingGuideQueryHandler(IRepository<StandardUser> userRepository,
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<AssignedTrainingGuides> assignedTrainingGuidesRepository,
            IRepository<CustomerGroup> groupRepository)
        {
            _userRepository = userRepository;
            _trainingGuideRepository = trainingGuideRepository;
            _assignedTrainingGuidesRepository = assignedTrainingGuidesRepository;
            _groupRepository = groupRepository;
        }
        public AssignTrainingGuideToUsersOrGroupsViewModel ExecuteQuery(AssignTrainingGuideQuery query)
        {
            var vm = new AssignTrainingGuideToUsersOrGroupsViewModel();
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
            vm.TrainingGuides = _trainingGuideRepository.List.OrderBy(x => x.Title).Where(x => x.IsActive).Select(x => new TrainingGuideAssignInfoViewModel
            {
                Id = x.Id,
                Title = x.Title,
                AssignedUserIds = _assignedTrainingGuidesRepository.List.Where(a => a.TrainingGuideId.Equals(x.Id) && a.UserId.HasValue).Select(a => a.UserId.Value)
            });
            return vm;
        }
    }
}
