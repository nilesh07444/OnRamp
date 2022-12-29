using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models.Groups;
using Domain.Customer.Models;
using Domain.Enums;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.CommandParameter.Group;
using Ramp.Contracts.Events.CustomerManagement;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Customer.Models.Document;
using VirtuaCon;
using Role = Ramp.Contracts.Security.Role;
using Domain.Models;
using Domain.Customer.Models.CustomRole;
using Ramp.Contracts.ViewModel;
using Ramp.Contracts.Command.Document;
using Domain.Customer;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class AddOrUpdateCustomerCompanyUserByCustomerAdminCommandHandler :
        CommandHandlerBase<AddOrUpdateCustomerCompanyUserByCustomerAdminCommand>

    {
        private readonly IRepository<CustomUserRoles> _customUserRolesRepository;
        private readonly IRepository<CustomerRole> _roleRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<RaceCodes> _raceCodeRepository;
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuideRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
        private readonly IRepository<Domain.Customer.Models.Groups.StandardUserGroup> _standardGroupRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        private readonly IRepository<AutoAssignWorkflow> _autoRepository;
        private readonly IRepository<AutoAssignDocuments> _docsRepository;
        private readonly IRepository<AutoAssignGroups> _grpRepository;


        public AddOrUpdateCustomerCompanyUserByCustomerAdminCommandHandler(IRepository<CustomerRole> roleRepository,
            IRepository<CustomUserRoles> customUserRolesRepository,
            IRepository<StandardUser> userRepository, IRepository<CustomerGroup> groupRepository,
            IRepository<Company> companyRepository, IRepository<RaceCodes> raceCodeRepository,
            IRepository<AssignedTrainingGuides> assignedTrainingGuideRepository,
            IRepository<TestAssigned> assignedTestRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<AssignedDocument> assignedDocumentRepository,
            ICommandDispatcher commandDispatcher,
            IRepository<Domain.Customer.Models.Groups.StandardUserGroup> standardGroupRepository,
            IRepository<AutoAssignWorkflow> autoRepository,
            IRepository<AutoAssignDocuments> docsRepository,
            IRepository<AutoAssignGroups> grpRepository
            )
        {
            _customUserRolesRepository = customUserRolesRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _companyRepository = companyRepository;
            _raceCodeRepository = raceCodeRepository;
            _assignedTrainingGuideRepository = assignedTrainingGuideRepository;
            _assignedTestRepository = assignedTestRepository;
            _testResultRepository = testResultRepository;
            _assignedDocumentRepository = assignedDocumentRepository;
            _standardGroupRepository = standardGroupRepository;
            _commandDispatcher = commandDispatcher;
            _autoRepository = autoRepository;
            _docsRepository = docsRepository;
            _grpRepository = grpRepository;

        }




        public override CommandResponse Execute(AddOrUpdateCustomerCompanyUserByCustomerAdminCommand command)
        {
            var adminUser = command.GroupName == null &&
                            Role.IsInAdminRole(command.Roles);
            var user =
                _userRepository.List.SingleOrDefault(
                    u => u.Id.Equals(command.UserId) || u.EmailAddress.Equals(command.EmailAddress));

            var checkuser = new StandardUser();
            if (adminUser)
            {
                if (user == null)
                {
                    user = new StandardUser
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = command.CompanyId,
                        EmailAddress = command.EmailAddress,
                        FirstName = command.FirstName,
                        LastName = command.LastName,
                        IsActive = true,
                        MobileNumber = command.MobileNumber,
                        ParentUserId = command.ParentUserId,
                        Password = new EncryptionHelper().Encrypt(command.Password),
                        EmployeeNo = command.EmployeeNo,
                        CreatedOn = DateTime.UtcNow,
                        IsUserExpire = false,
                        IsConfirmEmail = true,
                        IsFromSelfSignUp = command.IsFromSelfSignUp,
                        IDNumber = command.IDNumber,
                        Gender = GenderEnum.GetGenderFromString(command.Gender),
                        TrainingLabels = command.TrainingLabels,
                        CustomUserRoleId = (command.SelectedCustomUserRole == null && command.SelectedCustomUserRole == Guid.Empty) ? null : command.SelectedCustomUserRole,

                    };
                    if (command.IsFromSelfSignUp)
                        user.IsConfirmEmail = false;

                    if (command.SelectedGroupId != null)
                    {

                        command.SelectedGroupId.ForEach(x =>
                        {
                            var data = new Domain.Customer.Models.Groups.StandardUserGroup
                            {
                                Id = Guid.NewGuid(),
                                UserId = user.Id,
                                GroupId = Guid.Parse(x),
                                DateCreated = DateTime.Now

                            };
                            _standardGroupRepository.Add(data);
                        });
                        _standardGroupRepository.SaveChanges();

                        ////assign docs to user based on goups in workflow
                        AssignDocsToUser(command.SelectedGroupId, user.Id.ToString(), user.CompanyId.ToString(), command.ParentUserId);
                    }

                    if (command.RaceCodeId.HasValue)
                    {
                        var race = _raceCodeRepository.Find(command.RaceCodeId);
                        if (race != null)
                        {
                            user.RaceCodeId = race.Id;
                        }
                    }

                    var userModel = Projection.Project.UserViewModelFrom(user);
                    userModel.Password = new EncryptionHelper().Decrypt(userModel.Password);
                    _userRepository.Add(user);
                    if (!command.IsFromDataMigration && !user.AdUser)
                    {
                        new EventPublisher().Publish(new UserCreatedEvent
                        {
                            CompanyViewModel =
                                Projection.Project.CompanyViewModelFrom(_companyRepository.Find(command.CompanyId)),
                            UserViewModel = userModel,
                            Subject = UserCreatedEvent.DefaultSubject
                        });
                    }
                    if (command.SelectedGroupId != null)
                    {
                        AssignDocsToUser(command.SelectedGroupId, user.Id.ToString(), user.CompanyId.ToString(), command.ParentUserId);
                    }
                }
                else
                {
                    user.CompanyId = command.CompanyId;
                    user.FirstName = command.FirstName;
                    user.LastName = command.LastName;
                    user.ParentUserId = command.ParentUserId;
                    if (!string.IsNullOrEmpty(command.Password))
                        user.Password = new EncryptionHelper().Encrypt(command.Password);
                    user.MobileNumber = command.MobileNumber;
                    user.EmployeeNo = command.EmployeeNo;
                    user.IsConfirmEmail = command.IsConfirmEmail;
                    user.IDNumber = command.IDNumber;
                    user.Gender = GenderEnum.GetGenderFromString(command.Gender);

                    user.CustomUserRoleId = (command.SelectedCustomUserRole == null && command.SelectedCustomUserRole == Guid.Empty) ? null : command.SelectedCustomUserRole;

                    if (command.RaceCodeId.HasValue)
                    {
                        var race = _raceCodeRepository.Find(command.RaceCodeId);
                        if (race != null)
                        {
                            user.RaceCodeId = race.Id;
                        }
                    }
                    user.TrainingLabels = command.TrainingLabels;
                }
                while (user.Group != null)
                {
                    user.Group = null;
                }

                _userRepository.SaveChanges();
                //RemoveAllAssignedDocuments(user.Id);
                //RemoveAllAssignedTestsPlaybooksAndTestResults(user.Id);
            }
            else
            {
                CustomerGroup group;
                if (command.GroupName != null)
                {
                    new CommandDispatcher().Dispatch(new SaveOrUpdateGroupCommand()

                    {
                        Title = command.GroupName,
                        CompanyId = command.CompanyId
                    });

                    group = _groupRepository.List.SingleOrDefault((g => g.Title != null && g.Title.Equals(command.GroupName)));

                    command.SelectedGroupId = new List<string>();

                    command.SelectedGroupId.Add(group.Id.ToString());
                }
                else
                {
                    group = _groupRepository.List.SingleOrDefault(g => g.Id.Equals(command.SelectedGroupId));
                }

                //if (group == null)
                //    return null;

                if (user == null)
                {
                    user = new StandardUser
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = command.CompanyId,
                        EmailAddress = command.EmailAddress,
                        FirstName = command.FirstName,
                        LastName = command.LastName,
                        IsActive = true,
                        MobileNumber = command.MobileNumber,
                        ParentUserId = command.ParentUserId,
                        Password = new EncryptionHelper().Encrypt(command.Password),
                        EmployeeNo = command.EmployeeNo,
                        CreatedOn = DateTime.UtcNow,
                        IsUserExpire = false,
                        IsConfirmEmail = true,
                        CustomUserRoleId = (command.SelectedCustomUserRole == null && command.SelectedCustomUserRole == Guid.Empty) ? null : command.SelectedCustomUserRole,
                        AdUser = command.AdUser,
                        Department = command.Department,

                        IDNumber = command.IDNumber,
                        Gender = GenderEnum.GetGenderFromString(command.Gender),
                        TrainingLabels = command.TrainingLabels
                    };

                    //saveUserGroups(user.Id, command.GroupName, true);
                    var userModel = Projection.Project.UserViewModelFrom(user);
                    userModel.Password = new EncryptionHelper().Decrypt(userModel.Password);
                    checkuser = _userRepository.List.SingleOrDefault(u => u.Id.Equals(user.Id));
                    _userRepository.Add(user);
                    if (!command.IsFromDataMigration && !user.AdUser)
                    {
                        new EventPublisher().Publish(new UserCreatedEvent
                        {
                            CompanyViewModel =
                                Projection.Project.CompanyViewModelFrom(_companyRepository.Find(command.CompanyId)),
                            UserViewModel = userModel,
                            Subject = UserCreatedEvent.DefaultSubject
                        });
                    }
                    if (command.SelectedGroupId != null)
                    {

                        command.SelectedGroupId.ForEach(x =>
                        {
                            var data = new Domain.Customer.Models.Groups.StandardUserGroup
                            {
                                Id = Guid.NewGuid(),
                                UserId = user.Id,
                                GroupId = Guid.Parse(x),
                                DateCreated = DateTime.Now

                            };
                            _standardGroupRepository.Add(data);
                        });
                        _standardGroupRepository.SaveChanges();

                        ////assign docs to user based on goups in workflow
                        AssignDocsToUser(command.SelectedGroupId, user.Id.ToString(), user.CompanyId.ToString(), command.ParentUserId);
                    }
                    if (command.RaceCodeId.HasValue)
                    {
                        var race = _raceCodeRepository.Find(command.RaceCodeId);
                        if (race != null)
                        {
                            user.RaceCodeId = race.Id;
                        }
                    }
                  
                    
                }

                else
                {
                    user.CompanyId = command.CompanyId;
                    user.FirstName = command.FirstName;
                    user.LastName = command.LastName;
                    user.ParentUserId = command.ParentUserId;
                    if (!string.IsNullOrEmpty(command.Password))
                        user.Password = new EncryptionHelper().Encrypt(command.Password);
                    user.MobileNumber = command.MobileNumber;
                    user.EmployeeNo = command.EmployeeNo;
                     user.IsActive = command.IsActive;
                    //user.Group = group;
                    user.IDNumber = command.IDNumber;
                    user.RaceCodeId = command.RaceCodeId;
                    user.Gender = GenderEnum.GetGenderFromString(command.Gender);
                    user.TrainingLabels = command.TrainingLabels;
            
                    checkuser = _userRepository.List.SingleOrDefault(u => u.Id.Equals(user.Id));
                    if (!user.AdUser)
                    {
                        user.CustomUserRoleId = (command.SelectedCustomUserRole == null && command.SelectedCustomUserRole == Guid.Empty) ? null : command.SelectedCustomUserRole;
                    }



                    //saveUserGroups(user.Id, command.GroupName, false);
                }
            }


            //if (user.Roles.Count >= 1 && user.AdUser)
            //{
            //
            //}
            Guid asm = new Guid();
            
            if (command.UserId != asm && user.Id != null && user.AdUser)
            {
                AddUpdateUserRoles(user, command);
            }
            else if (command.UserId != asm && user.AdUser)
            {
                AddUpdateUserRoles(user, command);
            }
            else if (user.Id != null && !user.AdUser || user.Id == null && !user.AdUser)
            {
                AddUpdateUserRoles(user, command);
            }
            else if (checkuser == null && user.AdUser)
            {
                AddUpdateUserRoles(user, command);
            }

            ////code added by neeraj for custom user role
            //if(command.SelectedCustomUserRole != null && command.SelectedCustomUserRole != Guid.Empty)
            //{
            //	var roleEntity = _customUserRolesRepository.List.FirstOrDefault(r => r.Id == command.SelectedCustomUserRole);

            //	user.Roles.Add(new CustomerRole {
            //		Id = roleEntity.Id,
            //		Description = roleEntity.Title
            //	});
            //}

            if (!command.CompanyId.Equals(Guid.Empty))
                user.LayerSubDomain = _companyRepository.Find(command.CompanyId).LayerSubDomain;



            _userRepository.SaveChanges();
            _roleRepository.SaveChanges();
            _groupRepository.SaveChanges();

            if (user.Id != null)
            {

                var entity = _standardGroupRepository.List.Where(x => x.UserId == user.Id).AsQueryable().ToList();

                entity.ForEach(x =>
                {
                    _standardGroupRepository.Delete(x);
                });
                _standardGroupRepository.SaveChanges();

                if (command.SelectedGroupId != null)
                    command.SelectedGroupId.ForEach(x =>
                    {
                        var data = new Domain.Customer.Models.Groups.StandardUserGroup
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            GroupId = Guid.Parse(x),
                            DateCreated = DateTime.Now

                        };
                        _standardGroupRepository.Add(data);
                    });
                _standardGroupRepository.SaveChanges();
            }
            return null;
        }

        private void AddUpdateUserRoles(StandardUser user, AddOrUpdateCustomerCompanyUserByCustomerAdminCommand command)
        {
            user.Roles.Clear();


            foreach (var roleName in command.Roles)
            {
                var roleEntity = _roleRepository.List.FirstOrDefault(r => r.RoleName.Equals(roleName));
                if (roleEntity != null)
                {
                    if (roleEntity.RoleName.Equals(Role.StandardUser))
                    {
                        if (user.Roles.Count > 0)
                        {
                            user.Roles.Clear();
                        }
                        user.Roles.Add(roleEntity);
                        //break;
                    }
                    else
                    {
                        if (!user.Roles.Any(r => r.RoleName.Equals(roleEntity.RoleName)))
                            user.Roles.Add(roleEntity);
                    }
                }
                else
                {
                    roleEntity = new CustomerRole()
                    {
                        Id = Guid.NewGuid(),
                        RoleName = roleName
                    };
                    if (Role.RoleDescriptionDictionary.ContainsKey(roleName))
                        roleEntity.Description = Role.RoleDescriptionDictionary[roleName];

                    user.Roles.Add(roleEntity);
                }
            }

        }

        private void RemoveAllAssignedDocuments(Guid userId)
        {
            _assignedDocumentRepository.List.Where(ad => userId.ToString() == ad.UserId && !ad.Deleted).ForEach(ad => ad.Deleted = true);
            _assignedDocumentRepository.SaveChanges();
        }

        private void RemoveAllAssignedTestsPlaybooksAndTestResults(Guid userId)
        {
            _testResultRepository.List.AsQueryable().Where(x => x.TestTakenByUserId == userId).ToList().ForEach(delegate (TestResult result)
            {
                _testResultRepository.Delete(result);
            });
            _testResultRepository.SaveChanges();

            _assignedTrainingGuideRepository.List.AsQueryable().Where(x => x.UserId.HasValue && x.UserId.Value == userId).ToList().ForEach(delegate (AssignedTrainingGuides assignedGuide)
            {
                _assignedTrainingGuideRepository.Delete(assignedGuide);
            });
            _assignedTrainingGuideRepository.SaveChanges();

            _assignedTestRepository.List.AsQueryable().Where(x => x.UserId.HasValue && x.UserId.Value == userId).ToList().ForEach(delegate (TestAssigned assignedTest)
            {
                _assignedTestRepository.Delete(assignedTest);
            });
            _assignedTestRepository.SaveChanges();

        }


        private void saveUserGroups(Guid userId, string groupName, bool add)
        {
            if (!add)
            {
                var groupList = _standardGroupRepository.List.Where(cc => cc.UserId == userId).AsQueryable().ToList();

                groupList.ForEach(x =>
                {
                    _standardGroupRepository.Delete(x);
                });
                _standardGroupRepository.SaveChanges();
            }
            Guid groupId = Guid.Empty;
            if (groupName != null && groupName != "")
            {
                groupId = _groupRepository.List.Where(i => i.Title.ToLower() == groupName.ToLower()).Select(id => id.Id).FirstOrDefault();
            }


            var data = new Domain.Customer.Models.Groups.StandardUserGroup
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GroupId = (groupName != null && groupName != "") ? groupId : Guid.Empty,
                DateCreated = DateTime.Now

            };

            _standardGroupRepository.Add(data);
            _standardGroupRepository.SaveChanges();

        }

        private void AssignDocsToUser(List<string> ids, string uId, string cId, Guid parentUserId)
        {
            var allgroups = _standardGroupRepository.List.ToList();
            var data = new List<AssignmentViewModel>();
            var data1 = new List<AssignmentViewModel>();

            var autoWorkFlowIds = new List<string>();
            var allWorkflowDocs = new List<AutoAssignDocuments>();

            var allNotiEnabledDocs = new List<string>();


            foreach (var group in ids)
            {
                //get all forkflow id a group is associateed with
                var allWorkflowsContainingGroups =// _grpRepository.List.Where(x => x.GroupId.ToString() == group.ToString()).ToList();
                _grpRepository.List.Where(x => x.GroupId.ToString() == group.ToString()).Join(_autoRepository.List.Where(x => !x.IsDeleted), g => g.WorkFlowId, w => w.Id, (g,w) => new {
                    WorkFlowId=g.WorkFlowId
                }).ToList();

                autoWorkFlowIds.AddRange(allWorkflowsContainingGroups.Select(x => x.WorkFlowId.ToString()));
            }
            foreach (var wId in autoWorkFlowIds)
            {

                var notiEnabled = _autoRepository.List.Where(x => x.Id.ToString() == wId && x.SendNotiEnabled == true).FirstOrDefault();

                //get all the documents for all the workflows
                var allDocsInWorkflow = _docsRepository.List.Where(x => x.WorkFlowId.ToString() == wId).ToList();

                allWorkflowDocs.AddRange(allDocsInWorkflow);
            }


            try
            {
                #region assign docs

                var xx = new List<AutoAssignDocuments>();

                foreach (var doc in allWorkflowDocs)
                {

                    bool alreadyInList = false;

                    foreach (var item in xx)
                    {
                        if (item.DocumentId == doc.DocumentId)
                        {
                            alreadyInList = true;
                            break; // break the loop
                        }
                    }

                    if (!alreadyInList)
                    {
                        xx.Add(doc);
                    }
                }

                allWorkflowDocs = xx;
                foreach (var doc in allWorkflowDocs)
                {
                    DocumentType type;
                    //Enum.TryParse<DocumentType>(doc.Type, out type);

                    var docs = new AssignmentViewModel()
                    {
                        AssignedDate = DateTime.Now,
                        AdditionalMsg = "",
                        DocumentId = doc.DocumentId.ToString(),
                        DocumentType = (DocumentType)doc.Type,
                        MultipleAssignedDates = null,
                        OrderNumber = doc.Order,
                        UserId = uId
                    };
                    data.Add(docs);
                    var noti = _autoRepository.Find(doc.WorkFlowId).SendNotiEnabled;
                    if (noti != null && noti == true)
                    {
                        data1.Add(docs);
                    }
                }

                var assignedDocuments = _assignedDocumentRepository.List.Where(x => !x.Deleted).ToList();

                var existDocument = data.Where(y => assignedDocuments.Any(z => z.DocumentId == y.DocumentId && z.UserId == y.UserId)).ToList();

                var result = data.Except(existDocument);
                var datarcd = data.FirstOrDefault();
                var response = new AssignDocumentsToUsers
                {
                    AssignedBy = parentUserId.ToString(),
                    AssignmentViewModels =result,
                    CompanyId = Guid.Parse(cId),
                    AssignedDate = datarcd == null ? DateTime.UtcNow : datarcd.AssignedDate.Value.ToLocalTime(),
                    MultipleAssignedDates =  datarcd != null?datarcd.MultipleAssignedDates:null,
                    
                };

                var res = _commandDispatcher.Dispatch(response);


                var documentNotifications = new List<DocumentNotificationViewModel>();

                if (data1.Count > 0)
                {
                    foreach (var m in data1)
                    {
                        var additionalMsg = "";
                        var notificationModel = new DocumentNotificationViewModel
                        {
                            AssignedDate = DateTime.Now,
                            IsViewed = false,
                            DocId = m.DocumentId,
                            UserId = m.UserId,
                            NotificationType = m.DocumentType.ToString(),
                        };

                        if (m.AdditionalMsg != "" && m.AdditionalMsg != null)
                        {
                            var additionalMsgList = m.AdditionalMsg.Split('^').Select(f => new { Id = f.Split('~')[0], Msg = f.Split('~')[1] }).ToList();
                            additionalMsg = additionalMsgList.FirstOrDefault(f => f.Id == m.DocumentId).Msg;
                            if (additionalMsg != "")
                            {
                                notificationModel.Message = additionalMsg;
                            }
                        }
                        documentNotifications.Add(notificationModel);
                    }
                    _commandDispatcher.Dispatch(documentNotifications);
                }


                #endregion
            }
            catch (Exception e)
            {

                throw;
            }


        }
    }
}