using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.Security;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class ManageableUsersForCompanyQueryHandler : IQueryHandler<ManageableUsersForCompanyQueryParameter, Assign_UnAssignPlaybooksAndTests>
    {
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuideRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;

        public ManageableUsersForCompanyQueryHandler(IRepository<AssignedTrainingGuides> assignedTrainingGuideRepository,
            IRepository<TestAssigned> assignedTestRepository,
            IRepository<StandardUser> userRepository,
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<TrainingTest> trainingTestRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<CustomerGroup> groupRepository)
        {
            _assignedTestRepository = assignedTestRepository;
            _assignedTrainingGuideRepository = assignedTrainingGuideRepository;
            _userRepository = userRepository;
            _trainingGuideRepository = trainingGuideRepository;
            _trainingTestRepository = trainingTestRepository;
            _testResultRepository = testResultRepository;
            _groupRepository = groupRepository;
        }

        public Assign_UnAssignPlaybooksAndTests ExecuteQuery(ManageableUsersForCompanyQueryParameter query)
        {
            var vm = new Assign_UnAssignPlaybooksAndTests();

            vm.AssignModeDropDown = new[]
            {
                new SerializableSelectListItem { Value = ((int)AssignMode.Users).ToString(), Text = AssignMode.Users.ToString()},
                new SerializableSelectListItem { Value = ((int)AssignMode.Groups).ToString() , Text = AssignMode.Groups.ToString()}
            };

            if (query.FunctionalMode == FunctionalMode.Playbooks)
            {
                vm.TrainingGuideDropDown = _trainingGuideRepository.List.AsQueryable().Where(x => x.IsActive).OrderBy(x => x.Title).Select(x => new SerializableSelectListItem
                {
                    Selected = false,
                    Text = x.Title,
                    Value = x.Id.ToString()
                }).ToList();
            }
            else if (query.FunctionalMode == FunctionalMode.Tests)
            {
                vm.TrainingTestDropDown = _trainingGuideRepository.List.AsQueryable().Where(x => x.TestVersion.LastPublishedVersion != null && x.IsActive).Select(x => x.TestVersion.LastPublishedVersion).Where(x => x.ActiveStatus && (!x.Deleted.HasValue || (x.Deleted.HasValue && !x.Deleted.Value))).OrderBy(x => x.TestTitle).Select(x => new SerializableSelectListItem
                {
                    Selected = false,
                    Text = x.TestTitle,
                    Value = x.Id.ToString()
                }).ToList();
            }
            if (!query.SelectedTrainingTestId.Equals(Guid.Empty))
            {
                query.SelectedTrainingGuideId = _trainingTestRepository.Find(query.SelectedTrainingTestId).TrainingGuideId.Value;
                vm.TrainingGuideId = query.SelectedTrainingGuideId;
            }
            if (!query.SelectedTrainingGuideId.Equals(Guid.Empty))
            {
                var mu = new List<ManageAssignmentUserViewModel>();
                var assignedTests = _assignedTestRepository.List.AsQueryable().Select(x => new AssignedTestViewModel { TestId = x.TestId, UserId = x.UserId.Value }).OrderBy(x => x.UserId);
                var assignedGuides = _assignedTrainingGuideRepository.List.AsQueryable().Select(x => new AssignedTrainingGuideViewModel { TrainingGuideId = x.TrainingGuideId, UserId = x.UserId.Value }).OrderBy(x => x.UserId);

                var users = _userRepository.List.AsQueryable().Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)) && u.IsActive && !u.IsUserExpire && u.Group != null).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).Select(r => new UserModelShort
                {
                    Id = r.Id,
                    Name = r.FirstName + " " + r.LastName,
                    GroupId = r.Group.Id
                }).OrderBy(x => x.Name);

                var trainingGuide = _trainingGuideRepository.List.AsQueryable().Where(e => e.Id.Equals(query.SelectedTrainingGuideId)).FirstOrDefault();
                var model = new TrainingGuideViewModelShort
                {
                    CreatedBy = trainingGuide.CreatedBy,
                    Id = trainingGuide.Id,
                    Name = trainingGuide.Title
                };
                var versionIds = trainingGuide.TestVersion.Versions.Select(x => x.Id).ToList();
                if (trainingGuide.TestVersion.LastPublishedVersion != null)
                    if (trainingGuide.TestVersion.LastPublishedVersion.ActiveStatus)
                    {
                        model.TrainingTestId = trainingGuide.TestVersion.LastPublishedVersion.Id;
                        versionIds.Remove(model.TrainingTestId.Value);
                    }
                vm.HasTest = model.TrainingTestId.HasValue;
                foreach (var u in users.ToList())
                {
                    var mam =  new ManageAssignmentUserViewModel
                    {
                        AssignedTrainingGuide = assignedGuides.Any(x => x.UserId.Equals(u.Id) && x.TrainingGuideId.Equals(trainingGuide.Id)),
                        AssignedTrainingTest = assignedTests.Any(x => x.UserId.Equals(u.Id) && x.TestId.Equals(model.TrainingTestId.Value)),
                        AssignedPreviousVersionOfTest = _assignedTestRepository.List.AsQueryable().Any(x => versionIds.Contains(x.TestId) && x.UserId.HasValue && x.UserId.Value.Equals(u.Id)),
                        User = u
                    };
                    if (mam.AssignedTrainingTest)
                        mam.PassedTest = _testResultRepository.List.AsQueryable().Any(x => x.TrainingTestId.HasValue && x.TrainingTestId.Value.Equals(model.TrainingTestId.Value) && x.TestTakenByUserId.Equals(u.Id) && x.TestResultStatus);
                    var results = _testResultRepository.List.AsQueryable().Where(x => x.TrainingTestId.HasValue && versionIds.Contains(x.TrainingTestId.Value) && x.TestTakenByUserId.Equals(u.Id) && x.TestResultStatus);
                    mam.PassedPreviousVersionOfTest = results.Any();
                    mu.Add(mam);
                }
                vm.ManageableUsers = mu;
                var mg = new List<ManageAssignmentGroupViewModel>();
                var groups = _groupRepository.List.AsQueryable().Select(x => new GroupViewModelShort
                {
                    Id = x.Id,
                    Name = x.Title,
                    Selected = false,
                });
                foreach(var g in groups.ToList())
                {
                    var mUsersForGroup = mu.Where(x => x.User.GroupId.HasValue && x.User.GroupId.Value.Equals(g.Id));
                    if (mUsersForGroup.Count() > 0)
                    {
                        var r = new ManageAssignmentGroupViewModel();
                        r.AllMembersAssignedTest = mUsersForGroup.All(x => x.AssignedTrainingTest);
                        r.AllMemebersAssignedTrainingGuide = mUsersForGroup.All(x => x.AssignedTrainingGuide);
                        r.Group = g;
                        mg.Add(r);
                    }
                }
                vm.ManageableGroups = mg;
                
            }
            return vm;
        }
    }
}