using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.PolicyResponse;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using org.apache.poi.util;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.DocumentCategory;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.Security;
using Ramp.Contracts.ViewModel;
using VirtuaCon;

using StandardUserGroup = Domain.Customer.Models.Groups.StandardUserGroup;

namespace Ramp.Services.QueryHandler.Reporting
{
    public class InteractionReportQueryHandler :
        IQueryHandler<InteractionReportQuery, InteractionReportViewModel>,
        IQueryHandler<InteractionReportDetailQuery, InteractionReportDetailViewModel>
    {
        private readonly IQueryExecutor _queryExecutor;
        private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
        private readonly IRepository<DocumentUsage> _usageRepository;
        private readonly IRepository<PolicyResponse> _responseRepository;
        private readonly IRepository<Test_Result> _resultRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        private readonly IRepository<Test> _testRepository;
        private readonly IRepository<CheckList> _checkListRepository;
        private readonly IRepository<Policy> _policyRepository;
        private readonly IRepository<TrainingManual> _trainingManualRepository;
        private readonly IRepository<Memo> _memoRepository;
        private readonly IRepository<CheckListUserResult> _checkListUserResultRepository;
        private readonly ITransientReadRepository<CheckListChapter> _checkListChapterRepository;
        private readonly IRepository<CheckListChapterUserResult> _checkListChapterUserResultRepository;

        private readonly IRepository<StandardUserGroup> _standardUserGroupRepository;

        public InteractionReportQueryHandler(
            IRepository<StandardUserGroup> standardUserGroupRepository,
            IQueryExecutor queryExecutor,
            IRepository<AssignedDocument> assignedDocumentRepository,
            IRepository<DocumentUsage> usageRepository,
            IRepository<PolicyResponse> responseRepository,
            IRepository<Test_Result> resultRepository,
            IRepository<StandardUser> userRepository,
            IRepository<CustomerGroup> groupRepository,
            IRepository<CheckList> checkListRepository,
            IRepository<Policy> policyRepository,
            IRepository<TrainingManual> trainingManualRepository,
            ITransientReadRepository<CheckListChapter> checkListChapterRepository,
            IRepository<CheckListChapterUserResult> checkListChapterUserResultRepository,
            IRepository<CheckListUserResult> checkListUserResultRepository,
            IRepository<Memo> memoRepository,
            IRepository<Test> testRepository)
        {
            _queryExecutor = queryExecutor;
            _assignedDocumentRepository = assignedDocumentRepository;
            _usageRepository = usageRepository;
            _responseRepository = responseRepository;
            _resultRepository = resultRepository;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _testRepository = testRepository;
            _checkListRepository = checkListRepository;
            _policyRepository = policyRepository;
            _memoRepository = memoRepository;
            _trainingManualRepository = trainingManualRepository;
            _checkListChapterRepository = checkListChapterRepository;
            _checkListChapterUserResultRepository = checkListChapterUserResultRepository;
            _checkListUserResultRepository = checkListUserResultRepository;
            _standardUserGroupRepository = standardUserGroupRepository;
        }

        public InteractionReportViewModel ExecuteQuery(InteractionReportQuery query)
        {
            var vm = new InteractionReportViewModel()
            {
                TrainingManualInteractions = new List<InteractionReportViewModel.InteractionModel>(),
                MemoInteractions = new List<InteractionReportViewModel.InteractionModel>(),
                PolicyInteractions = new List<InteractionReportViewModel.PolicyInteractionModel>(),
                TestInteractions = new List<InteractionReportViewModel.TestInteractionModel>(),
                CheckListInteractions = new List<InteractionReportViewModel.InteractionModel>(),
                GlobalTrainingManualInteractions = new List<InteractionReportViewModel.InteractionModel>(),
                GlobalMemoInteractions = new List<InteractionReportViewModel.InteractionModel>(),
                GlobalPolicyInteractions = new List<InteractionReportViewModel.PolicyInteractionModel>(),
                GlobalTestInteractions = new List<InteractionReportViewModel.TestInteractionModel>(),
                GlobalCheckListInteractions = new List<InteractionReportViewModel.InteractionModel>()
            };

            #region Global Documents
            var GlobalDocuments = new List<DocumentListModel>();
            GlobalDocuments.AddRange(_checkListRepository.Get(c => c.IsGlobalAccessed && c.DocumentStatus == DocumentStatus.Published && !c.Deleted).Select(c => new DocumentListModel
            {
                Title = c.Title,
                Description = c.Description,
                TrainingLabels = c.TrainingLabels,
                DocumentType = DocumentType.Checklist,
                CoverPictureId = c.CoverPictureId,
                Id = c.Id
            }).ToList());
            GlobalDocuments.AddRange(_policyRepository.Get(c => c.IsGlobalAccessed && c.DocumentStatus == DocumentStatus.Published && !c.Deleted).Select(c => new DocumentListModel
            {
                Title = c.Title,
                Description = c.Description,
                TrainingLabels = c.TrainingLabels,
                DocumentType = DocumentType.Policy,
                CoverPictureId = c.CoverPictureId,
                Id = c.Id
            }).ToList());
            GlobalDocuments.AddRange(_testRepository.Get(c => c.IsGlobalAccessed && c.DocumentStatus == DocumentStatus.Published && !c.Deleted).Select(c => new DocumentListModel
            {
                Title = c.Title,
                Description = c.Description,
                TrainingLabels = c.TrainingLabels,
                DocumentType = DocumentType.Test,
                CoverPictureId = c.CoverPictureId,
                Id = c.Id
            }).ToList());
            GlobalDocuments.AddRange(_memoRepository.Get(c => c.IsGlobalAccessed && c.DocumentStatus == DocumentStatus.Published && !c.Deleted).Select(c => new DocumentListModel
            {
                Title = c.Title,
                Description = c.Description,
                TrainingLabels = c.TrainingLabels,
                DocumentType = DocumentType.Memo,
                CoverPictureId = c.CoverPictureId,
                Id = c.Id
            }).ToList());
            GlobalDocuments.AddRange(_trainingManualRepository.Get(c => c.IsGlobalAccessed && c.DocumentStatus == DocumentStatus.Published && !c.Deleted).Select(c => new DocumentListModel
            {
                Title = c.Title,
                Description = c.Description,
                TrainingLabels = c.TrainingLabels,
                DocumentType = DocumentType.TrainingManual,
                CoverPictureId = c.CoverPictureId,
                Id = c.Id
            }).ToList());
            #endregion


            #region Filter
            //Date Range

            var assigned = _assignedDocumentRepository.List.Join(_userRepository.GetAll(), assign => assign.UserId, ur => ur.Id.ToString(), (assign, ur) => new { assignDocs = assign, users = ur }).Where(x =>
            x.users.IsActive && !x.assignDocs.Deleted && x.assignDocs.AssignedDate.ToLocalTime() >= query.FromDate.AtBeginningOfDay() &&
                x.assignDocs.AssignedDate.ToLocalTime() <= query.ToDate.AtEndOfDay())
                .Select(c => new AssignedDocument
                {
                    AdditionalMsg = c.assignDocs.AdditionalMsg,
                    AssignedBy = c.assignDocs.AssignedBy,
                    AssignedDate = c.assignDocs.AssignedDate,
                    Deleted = c.assignDocs.Deleted,
                    DocumentId = c.assignDocs.DocumentId,
                    DocumentType = c.assignDocs.DocumentType,
                    Id = c.assignDocs.Id,
                    IsRecurring = c.assignDocs.IsRecurring,
                    OrderNumber = c.assignDocs.OrderNumber,
                    UserId = c.assignDocs.UserId

                })
                .ToList();

            // Filter by document types
            if (query.DocumentTypes?.Any() ?? false)
            {
                assigned = assigned.Where(x => query.DocumentTypes.Contains(x.DocumentType)).ToList();
            }


            // Groups
            if (query.GroupIds?.Any() ?? false)
            {
                //reemoved && Role.IsInStandardUserRole(u.Roles.Select(x => x.RoleName).ToList()) from below condition
                var usersInGroupsIds = _userRepository.Get(u => !u.IsUserExpire && u.IsActive)
                    .Select(u => u.Id.ToString()).ToList();

                //bnelow code added by neraj
                #region
                List<string> userIds = new List<string>();

                foreach (var u in usersInGroupsIds)
                {
                    var groupList = _standardUserGroupRepository.Get(c => c.UserId.ToString() == u.ToString()).ToList();

                    string name = null;
                    if (groupList.Count > 0)
                    {
                        foreach (var g in groupList)
                        {
                            if (name != null)
                                name = name + "," + g.GroupId;
                            else name = name + g.GroupId;
                        }
                        var x = String.Join(",", query.GroupIds);
                        if (name != null && name != "" && x.Contains(name) || name.Contains(x))
                        {
                            userIds.Add(u.ToString());
                        }
                    }
                }
                //usersInGroupsIds = userIds;
                var tu = String.Join(",", userIds);
                //var r = assigned.Where(rr => rr.UserId.ToString().Contains(tu)).ToList();
                //var xi = assigned.Where(cv => tu.Contains(cv.UserId.ToString())).ToList();
                assigned = assigned.Where(u => tu.Contains(u.UserId.ToString())).ToList();
                #endregion
                //code end
            }

            // Departments
            if (query.Departments?.Any() ?? false)
            {
                //reemoved && Role.IsInStandardUserRole(u.Roles.Select(x => x.RoleName).ToList()) from below condition
                var usersInGroupsIds = _userRepository.Get(u => !u.IsUserExpire && u.IsActive && u.Department != null)
                    .Select(u => new { Department = u.Department.ToString(), Id = u.Id.ToString() }).ToList();

                //bnelow code added by neraj
                #region
                List<string> userIds = new List<string>();

                foreach (var u in usersInGroupsIds)
                {
                    var x = String.Join(",", query.Departments);
                    if (u.Department != null && x.Contains(u.Department) || u.Department.Contains(x))
                    {
                        userIds.Add(u.Id.ToString());
                    }
                }
                //usersInGroupsIds = userIds;
                var tu = String.Join(",", userIds);
                //var r = assigned.Where(rr => rr.UserId.ToString().Contains(tu)).ToList();
                //var xi = assigned.Where(cv => tu.Contains(cv.UserId.ToString())).ToList();
                assigned = assigned.Where(u => tu.Contains(u.UserId.ToString())).ToList();
                #endregion
                //code end
            }

            // Categories
            if (query.CategoryIds?.Any() ?? false)
            {
                var categoryIds = IncludeSubCategories(query.CategoryIds);
                var documentsInCategoriesIds =
                    _queryExecutor.Execute<DocumentListQuery, IEnumerable<DocumentListModel>>(new DocumentListQuery
                    {
                        DocumentStatus = new[]
                            {
                                "Status:Published",
                                "Status:Recalled"
                            }
                    })
                        .Where(d => categoryIds.Contains(d.CategoryId))
                        .Select(d => d.Id);
                assigned = assigned.Where(a => documentsInCategoriesIds.Contains(a.DocumentId)).ToList();
            }
            #endregion Filter

            // Build view model
            var documentsAssigned = from a in assigned.ToList()
                                    group a by new
                                    {
                                        a.DocumentId,
                                        a.DocumentType
                                    };

            foreach (var g in documentsAssigned)
            {
                var document = _queryExecutor.Execute<DocumentQuery, DocumentListModel>(new DocumentQuery
                {
                    Id = g.Key.DocumentId,
                    DocumentType = g.Key.DocumentType
                });

                var userIds = g.Select(x => x.UserId);

                var interacted = 0;
                if (g.Key.DocumentType == DocumentType.Memo || g.Key.DocumentType == DocumentType.TrainingManual || g.Key.DocumentType == DocumentType.Checklist)
                {
                    interacted = (from u in _usageRepository.Get(u =>
                            u.DocumentId == g.Key.DocumentId && !u.IsGlobalAccessed &&
                            u.DocumentType == g.Key.DocumentType && userIds.Contains(u.UserId))
                                  group u by new
                                  {
                                      u.UserId,
                                      u.DocumentId,
                                      u.DocumentType,
                                  }
                        into gr
                                  select gr.OrderBy(u => u.ViewDate).FirstOrDefault()).Count();
                }

                if (document != null && !string.IsNullOrEmpty(document.Title))
                {
                    switch (g.Key.DocumentType)
                    {
                        case DocumentType.TrainingManual:
                            vm.TrainingManualInteractions.Add(new InteractionReportViewModel.InteractionModel
                            {
                                Title = document.Title,
                                TrainingLabels = document.TrainingLabels,
                                Allocated = g.Count(),
                                Interacted = interacted,
                                DocumentId = g.Key.DocumentId,
                                GloballyAccessed = 0,
                                YetToInteract = g.Count() - interacted
                            });
                            break;
                        case DocumentType.Memo:
                            vm.MemoInteractions.Add(new InteractionReportViewModel.InteractionModel
                            {
                                Title = document.Title,
                                TrainingLabels = document.TrainingLabels,
                                Allocated = g.Count(),
                                Interacted = interacted,
                                DocumentId = g.Key.DocumentId,
                                GloballyAccessed = 0,
                                YetToInteract = g.Count() - interacted
                            });
                            break;
                        case DocumentType.Policy:
                            var responses = (from u in _responseRepository.Get(r => !r.IsGlobalAccessed &&
                                    r.PolicyId == g.Key.DocumentId && userIds.Contains(r.UserId))
                                             group u by u.UserId
                                into gr
                                             select gr.OrderByDescending(u => u.Created).FirstOrDefault())
                                .ToList();

                            vm.PolicyInteractions.Add(new InteractionReportViewModel.PolicyInteractionModel
                            {

                                Title = document.Title,
                                TrainingLabels = document.TrainingLabels,
                                Allocated = g.Count(),
                                ViewLater = responses.Count(r => r.Response == null),
                                Accepted = responses.Count(r => r.Response == true),
                                Rejected = responses.Count(r => r.Response == false),
                                DocumentId = g.Key.DocumentId,
                                GloballyAccessed = 0,
                                YetToInteract = g.Count() - (responses.Count(r => r.Response == null) + responses.Count(r => r.Response == true) + responses.Count(r => r.Response == false))
                            });
                            break;
                        case DocumentType.Test:
                            var results = (from r in _resultRepository.Get(r => !r.IsGloballyAccessed &&
                                    r.TestId == g.Key.DocumentId && r.Deleted == false && userIds.Contains(r.UserId))
                                           group r by r.UserId
                                into gr
                                           select gr.Any(r => r.Passed))
                                .ToList();

                            vm.TestInteractions.Add(new InteractionReportViewModel.TestInteractionModel
                            {
                                Title = document.Title,
                                TrainingLabels = document.TrainingLabels,
                                Allocated = g.Count(),
                                Failed = results.Count(r => !r),
                                Passed = results.Count(r => r),
                                DocumentId = g.Key.DocumentId,
                                GloballyAccessed = 0,
                                YetToInteract = g.Count() - (results.Count(r => r) + results.Count(r => !r))
                            });
                            break;
                        case DocumentType.Checklist:
                            vm.CheckListInteractions.Add(new InteractionReportViewModel.InteractionModel
                            {
                                Title = document.Title,
                                TrainingLabels = document.TrainingLabels,
                                Allocated = g.Count(),
                                Interacted = interacted,
                                DocumentId = g.Key.DocumentId,
                                GloballyAccessed = 0,
                                YetToInteract = g.Count() - interacted
                            });
                            break;
                        case DocumentType.Certificate:
                        case DocumentType.Unknown:
                        default:
                            break;
                    }
                }
            }
            //Globally accessed 
            if (!string.IsNullOrEmpty(query.TrainingLabels))
            {
                GlobalDocuments = GlobalDocuments.Where(c => c.TrainingLabels.Contains(query.TrainingLabels)).ToList();
                vm.MemoInteractions = vm.MemoInteractions.Where(c => c.TrainingLabels.Contains(query.TrainingLabels)).ToList();
                vm.TestInteractions = vm.TestInteractions.Where(c => c.TrainingLabels.Contains(query.TrainingLabels)).ToList();
                vm.CheckListInteractions = vm.CheckListInteractions.Where(c => c.TrainingLabels.Contains(query.TrainingLabels)).ToList();
                vm.TrainingManualInteractions = vm.TrainingManualInteractions.Where(c => c.TrainingLabels.Contains(query.TrainingLabels)).ToList();
                vm.PolicyInteractions = vm.PolicyInteractions.Where(c => c.TrainingLabels.Contains(query.TrainingLabels)).ToList();

            }
            var users = _userRepository.GetAll().Where(u => u.IsActive).ToList();
            foreach (var globalDoc in GlobalDocuments)
            {

                var interacted = 0;
                if (globalDoc.DocumentType == DocumentType.Memo || globalDoc.DocumentType == DocumentType.TrainingManual || globalDoc.DocumentType == DocumentType.Checklist)
                {
                    interacted = (from u in _usageRepository.Get(u =>
                            u.DocumentId == globalDoc.Id && u.IsGlobalAccessed == true &&
                            u.DocumentType == globalDoc.DocumentType)
                                  group u by new
                                  {
                                      u.UserId,
                                      u.DocumentId,
                                      u.DocumentType,
                                  }
                        into gr
                                  select gr.OrderBy(u => u.ViewDate).FirstOrDefault()).Count();
                }


                switch (globalDoc.DocumentType)
                {
                    case DocumentType.TrainingManual:
                        vm.GlobalTrainingManualInteractions.Add(new InteractionReportViewModel.InteractionModel
                        {
                            Title = globalDoc.Title,
                            TrainingLabels = globalDoc.TrainingLabels,
                            Allocated = 0,
                            Interacted = interacted,
                            DocumentId = globalDoc.Id,
                            YetToInteract = users.Count - interacted,
                            GloballyAccessed = _usageRepository.Get(u => u.DocumentId == globalDoc.Id).GroupBy(g => g.UserId).Count()
                        });

                        break;
                    case DocumentType.Checklist:

                        vm.GlobalCheckListInteractions.Add(new InteractionReportViewModel.InteractionModel
                        {
                            Title = globalDoc.Title,
                            TrainingLabels = globalDoc.TrainingLabels,
                            Allocated = 0,
                            YetToInteract = users.Count - interacted,
                            Interacted = interacted,
                            DocumentId = globalDoc.Id,
                            GloballyAccessed = _usageRepository.Get(u => u.DocumentId == globalDoc.Id).GroupBy(g => g.UserId).Count()
                        });

                        break;
                    case DocumentType.Policy:
                        var responses = (from u in _responseRepository.Get(r =>
                                    r.PolicyId == globalDoc.Id && r.IsGlobalAccessed == true)
                                         group u by u.UserId
                                into gr
                                         select gr.OrderByDescending(u => u.Created).FirstOrDefault())
                                .ToList();
                        vm.GlobalPolicyInteractions.Add(new InteractionReportViewModel.PolicyInteractionModel
                        {
                            Title = globalDoc.Title,
                            TrainingLabels = globalDoc.TrainingLabels,
                            Allocated = 0,
                            YetToInteract = users.Count - interacted,
                            ViewLater = responses.Count(r => r.Response == null),
                            Accepted = responses.Count(r => r.Response == true),
                            Rejected = responses.Count(r => r.Response == false),
                            DocumentId = globalDoc.Id,
                            GloballyAccessed = _usageRepository.Get(u => u.DocumentId == globalDoc.Id).GroupBy(g => g.UserId).Count()
                        });
                        break;
                    case DocumentType.Memo:
                        vm.GlobalMemoInteractions.Add(new InteractionReportViewModel.InteractionModel
                        {
                            Title = globalDoc.Title,
                            TrainingLabels = globalDoc.TrainingLabels,
                            Allocated = 0,
                            YetToInteract = users.Count - interacted,
                            Interacted = interacted,
                            DocumentId = globalDoc.Id,
                            GloballyAccessed = _usageRepository.Get(u => u.DocumentId == globalDoc.Id).GroupBy(g => g.UserId).Count()
                        });

                        break;
                    case DocumentType.Test:
                        var results = (from r in _resultRepository.Get(r =>
                                    r.TestId == globalDoc.Id && r.Deleted == false && r.IsGloballyAccessed == true)
                                       group r by r.UserId
                                into gr
                                       select gr.Any(r => r.Passed))
                                .ToList();
                        vm.GlobalTestInteractions.Add(new InteractionReportViewModel.TestInteractionModel
                        {
                            Title = globalDoc.Title,
                            TrainingLabels = globalDoc.TrainingLabels,
                            Allocated = 0,
                            YetToInteract = users.Count - interacted,
                            Failed = results.Count(r => !r),
                            Passed = results.Count(r => r),
                            DocumentId = globalDoc.Id,
                            GloballyAccessed = 0
                        });


                        break;
                }
            }



            return vm;
        }

        private string[] IncludeSubCategories(string[] queryCategoryIds)
        {
            var categories =
                _queryExecutor.Execute<DocumentCategoryListQuery, IEnumerable<JSTreeViewModel>>(
                    new DocumentCategoryListQuery()).ToList();

            var result = new List<string>();
            foreach (var categoryId in queryCategoryIds)
            {
                result.Add(categoryId);
                result.AddRange(SubCategoryIds(categories, categoryId));
            }

            return result.Distinct().ToArray();
        }

        private string[] SubCategoryIds(List<JSTreeViewModel> categories, string parentId)
        {
            var result = new List<string>();
            var children = categories.Where(c => c.parent == parentId).ToList();

            foreach (var child in children)
            {
                result.Add(child.id);
                result.AddRange(SubCategoryIds(categories, child.id));
            }

            return result.ToArray();
        }

        public InteractionReportDetailViewModel ExecuteQuery(InteractionReportDetailQuery query)
        {
            var document = _queryExecutor.Execute<DocumentQuery, DocumentListModel>(new DocumentQuery
            {
                DocumentType = query.DocumentType,
                Id = query.DocumentId
            });

            var groups = _groupRepository.List;

            if (query.FromDate.ToString() == "0001/01/01 00:00:00" || query.ToDate.ToString() == "0001/01/01 00:00:00")
            {
                query.FromDate = new DateTime(2019, 01, 01);
                query.ToDate = DateTime.Now;

            }

            var vm = new InteractionReportDetailViewModel()
            {
                Groups = new List<string>(),
                Interactions = new List<InteractionReportDetailViewModel.InteractionDetailModel>(),
                GlobalInteractions = new List<InteractionReportDetailViewModel.InteractionDetailModel>(),
                GeneratedDate = DateTime.Now,
                FromDate = query.FromDate,
                ToDate = query.ToDate,
                DocumentTitle = document.Title,
                DocumentType = query.DocumentType
            };

            var assigned = _assignedDocumentRepository.Get(a => a.DocumentId == query.DocumentId && a.DocumentType == query.DocumentType && !a.Deleted && a.AssignedDate >= query.FromDate &&
                a.AssignedDate <= query.ToDate);

            if (query.GroupIds?.Any() ?? false)
            {
                var usersInGroupsIds = _userRepository.Get(u => !u.IsUserExpire && u.IsActive)
                    .Select(u => u.Id.ToString());

                //var temp = usersInGroupsIds.ToList();

                //assigned = assigned.Where(a => usersInGroupsIds.Contains(a.UserId));

                //var temp1 = assigned.ToList();

                //bnelow code added by neraj
                #region
                List<string> userIds = new List<string>();
                var groupList = _standardUserGroupRepository.List.Join(_userRepository.Get(u => !u.IsUserExpire && u.IsActive), sug => sug.UserId, u => u.Id, (sug, u) => new { SUG = sug, U = u })

                    .Select(gl => new {
                        grplist = gl.SUG,
                        userIngrp = gl.U

                    }).ToList();
                //foreach (var u in usersInGroupsIds)
                //{
                //   var groupList = _standardUserGroupRepository.Get(c => c.UserId.ToString() == u.ToString()).ToList();

                string name = null;
                //if (groupList.Count > 0)
                //{
                foreach (var g in groupList)
                {
                    if (name != null)
                        name = name + "," + g.grplist.GroupId;
                    else name = name + g.grplist.GroupId;
                }
                //var x = String.Join(",", query.GroupIds);
                //if (name != null && name != "" && x.Contains(name) || name.Contains(x))
                //{
                //userIds.Add(u.ToString());                    
                //}
                // }
                // }
                //usersInGroupsIds = userIds;
                var x = String.Join(",", query.GroupIds);
                if (name != null && name != "" && x.Contains(name) || name.Contains(x))
                {
                    var tu = String.Join(",", groupList.Select(z => z.userIngrp.Id).ToList());
                    //var r = assigned.Where(rr => rr.UserId.ToString().Contains(tu)).ToList();
                    //var xi = assigned.Where(cv => tu.Contains(cv.UserId.ToString())).ToList();
                    assigned = assigned.Where(u => tu.Contains(u.UserId.ToString())).ToList();
                }
                #endregion
                //code end

            }

            var assignedUserIds = assigned.Select(u => u.UserId).ToList();
            var documentUsagesRepo = _usageRepository.Get(u => u.DocumentId == query.DocumentId && u.DocumentType == query.DocumentType).Distinct().ToList();


            switch (query.DocumentType)
            {
                case DocumentType.Memo:
                case DocumentType.Checklist:
                case DocumentType.TrainingManual:
                    {
                        var documentUsages = documentUsagesRepo.Where(u => !u.IsGlobalAccessed).ToList();

                        var interactedUserIds = documentUsages.Select(u => u.UserId).Distinct().ToList();

                        var fetchUser = assignedUserIds.Join(_userRepository.Get(u => u.IsActive), assign_user => assign_user.AsGuid(), users => users.Id, (assign_user, users) => new { assignUser = assign_user, Userdata = users })
                            .Select(u => u.Userdata).Distinct().ToList();

                        foreach (var user in fetchUser)
                        {
                            //var user = _userRepository.Find(userId.AsGuid());



                            //if (user != null && user.IsActive)
                            //{
                            //below code added by neeraj

                            var groupList = _standardUserGroupRepository.Get(c => c.UserId.ToString() == user.Id.ToString()).Join(groups, sug => sug.GroupId, g => g.Id, (sug, g) => new { SUG = sug, G = g })
                            .Select(gl => new { grplist = gl.SUG, grps = gl.G }).ToList();

                            string name = null;
                            //List<string> name = new List<string>();
                            if (groupList.Count > 0)
                            {
                                foreach (var gl in groupList)
                                {
                                    //foreach (var gl in groups)
                                    //{
                                    //	if (gl.Id == g.GroupId)
                                    //	{
                                    if (name != null)
                                        name = name + "," + gl.grps.Title;
                                    else name = name + gl.grps.Title;
                                    //	}
                                    //}
                                }
                            }
                            if (name != null)
                            {
                                user.Group = new CustomerGroup()
                                {
                                    Title = name
                                };
                            }
                            // code end

                            var interacted = interactedUserIds.Contains(user.Id.ToString());
                            var interactionModel = new InteractionReportDetailViewModel.InteractionDetailModel
                            {
                                UserId = user.Id.ToString(),
                                Name = $"{user.FirstName} {user.LastName}",
                                Group = user.Group?.Title,
                                IDNumber = user.IDNumber,
                                Status = interacted ? "Interacted" : "Yet to Interact",
                            };
                            if (query.DocumentType == DocumentType.Checklist)
                            {
                                var assignedDocId = assigned.Where(x => x.UserId == user.Id.ToString()).FirstOrDefault().Id;
                                var checkListUserResult1 = _checkListUserResultRepository.Get(c => c.AssignedDocumentId == assignedDocId).FirstOrDefault();
                                interactionModel.DateSubmitted = (checkListUserResult1 != null) ? checkListUserResult1.SubmittedDate : DateTime.MinValue.Date;
                                var checkListChapterUserResult = _checkListChapterUserResultRepository.Get(c => c.AssignedDocumentId == assignedDocId).ToList();
                                var checkListChapter = _checkListChapterRepository.List.AsQueryable().Where(c => c.CheckListId == query.DocumentId && !c.Deleted).ToList();
                                interactionModel.ChecksCompleted = $"{checkListChapterUserResult.Where(c => c.IsChecked == true).Count()}/{checkListChapter.Count()}";
                            }
                            if (interacted)
                            {
                                if (documentUsages.Count() > 0)
                                {
                                    var userDocUsage = documentUsages.Where(x => x.UserId == user.Id.ToString()).FirstOrDefault();
                                    interactionModel.Duration = userDocUsage.Duration;
                                    interactionModel.ViewDate = userDocUsage.ViewDate;
                                }
                            }
                            vm.Interactions.Add(interactionModel);
                        }
                        //}
                    }
                    break;
                case DocumentType.Policy:
                    {

                        var responses = (from u in _responseRepository.Get(r =>
                                r.PolicyId == query.DocumentId && !r.IsGlobalAccessed && assignedUserIds.Contains(r.UserId))
                                         group u by u.UserId
                            into gr
                                         select gr.OrderByDescending(u => u.Created).FirstOrDefault()).ToList();

                        var respondedUserIds = responses.Select(r => r.UserId).Distinct();

                        foreach (var response in responses)
                        {
                            var user = _userRepository.Find(response.UserId.AsGuid());
                            if (user != null)
                            {

                                vm.Interactions.Add(new InteractionReportDetailViewModel.InteractionDetailModel
                                {
                                    UserId = user.Id.ToString(),
                                    Name = $"{user.FirstName} {user.LastName}",
                                    Group = user.Group?.Title,
                                    IDNumber = user.IDNumber,
                                    Status = !response.Response.HasValue ? "View Later" : response.Response.Value ? "Yes" : "No"
                                });

                            }
                        }
                        foreach (var userId in assignedUserIds.Except(respondedUserIds))
                        {
                            var user = _userRepository.Find(userId.AsGuid());
                            //below code added by neeraj


                            // code end
                            if (user != null)
                            {
                                var groupList = _standardUserGroupRepository.Get(c => c.UserId.ToString() == user.Id.ToString()).ToList();

                                string name = null;
                                //List<string> name = new List<string>();
                                if (groupList.Count > 0)
                                {
                                    foreach (var g in groupList)
                                    {
                                        foreach (var gl in groups)
                                        {
                                            if (gl.Id == g.GroupId)
                                            {
                                                if (name != null)
                                                    name = name + "," + gl.Title;
                                                else name = name + gl.Title;
                                            }
                                        }
                                    }
                                }
                                if (name != null)
                                {
                                    user.Group = new CustomerGroup()
                                    {
                                        Title = name
                                    };
                                }

                                vm.Interactions.Add(new InteractionReportDetailViewModel.InteractionDetailModel
                                {
                                    UserId = user.Id.ToString(),
                                    Name = $"{user.FirstName} {user.LastName}",
                                    Group = user.Group?.Title,
                                    IDNumber = user.IDNumber,
                                    Status = "Yet to Interact"
                                });


                            }
                        }
                    }
                    break;
                case DocumentType.Test:
                    {
                        var test = _testRepository.Find(query.DocumentId);
                        var passMarks = Math.Ceiling(test.PassMarks / 100 * test.Questions.Sum(x => Math.Max(1, x.AnswerWeightage)));
                        vm.PassRequirement = $"{(test.PassMarks / 100).ToString("P")} ({passMarks}/{test.Questions.Sum(x => Math.Max(1, x.AnswerWeightage))})";

                        foreach (var userId in assignedUserIds)
                        {
                            var user = _userRepository.Find(userId.AsGuid());
                            //below code added by neeraj

                            var groupList = _standardUserGroupRepository.Get(c => c.UserId.ToString() == user.Id.ToString()).ToList();

                            string name = null;
                            //List<string> name = new List<string>();
                            if (groupList.Count > 0)
                            {
                                foreach (var g in groupList)
                                {
                                    foreach (var gl in groups)
                                    {
                                        if (gl.Id == g.GroupId)
                                        {
                                            if (name != null)
                                                name = name + "," + gl.Title;
                                            else name = name + gl.Title;
                                        }
                                    }
                                }
                            }
                            if (name != null)
                            {
                                user.Group = new CustomerGroup()
                                {
                                    Title = name
                                };
                            }
                            // code end
                            if (user != null)
                            {
                                var interaction = new InteractionReportDetailViewModel.InteractionDetailModel
                                {
                                    UserId = user.Id.ToString(),
                                    Name = $"{user.FirstName} {user.LastName}",
                                    Group = user.Group?.Title,
                                    IDNumber = user.IDNumber
                                };
                                var results = _resultRepository.List
                                    .Where(r => r.UserId == userId && !r.IsGloballyAccessed && r.TestId == query.DocumentId && !r.Deleted)
                                    .OrderBy(r => r.Created).ToList();
                                if (results.Any())
                                {
                                    var test_result = results.FirstOrDefault();
                                    interaction.Duration = test_result.TimeTaken;
                                    interaction.DateCompleted = test_result.Created.DateTime;
                                    interaction.Status = results.Any(r => r.Passed) ? "Passed" : "Failed";
                                    interaction.Result1 =
                                        $"{results[0].Score}/{results[0].Total} ({results[0].Percentage.ToString("G")}%)";
                                    interaction.ResultId1 = results[0].Id;
                                    if (results.Count() > 1)
                                    {
                                        interaction.Result2 =
                                            $"{results[1].Score}/{results[1].Total} ({results[1].Percentage.ToString("G")}%)";
                                        interaction.ResultId2 = results[1].Id;
                                    }
                                    if (results.Count() > 2)
                                    {
                                        interaction.Result3 =
                                            $"{results[2].Score}/{results[2].Total} ({results[2].Percentage.ToString("G")}%)";
                                        interaction.ResultId3 = results[2].Id;
                                    }
                                }
                                else
                                {
                                    interaction.Status = "Yet to Interact";
                                }

                                vm.Interactions.Add(interaction);
                            }
                        }
                    }
                    break;
                case DocumentType.Certificate:
                case DocumentType.Unknown:
                default:
                    break;
            }
            #region Global Access
            switch (query.DocumentType)
            {
                case DocumentType.Memo:
                case DocumentType.Checklist:

                case DocumentType.TrainingManual:
                    {
                        var documentUsages = documentUsagesRepo.Where(u => u.IsGlobalAccessed).ToList();

                        var interactedUserIds = documentUsages.Select(u => u.UserId).Distinct().ToList();

                        foreach (var userId in interactedUserIds)
                        {
                            var user = _userRepository.Find(userId.AsGuid());
                            if (user != null)
                            {
                                var interacted = interactedUserIds.Contains(userId);

                                var interactionModel = new InteractionReportDetailViewModel.InteractionDetailModel
                                {
                                    UserId = user.Id.ToString(),
                                    Name = $"{user.FirstName} {user.LastName}",
                                    Group = user.Group?.Title,
                                    IDNumber = user.IDNumber,
                                    Status = interacted ? "Interacted" : "Yet to Interact"
                                };
                                if (interacted)
                                {
                                    if (query.DocumentType == DocumentType.Checklist)
                                    {
                                        var checkListUserResult1 = _checkListUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == query.DocumentId).FirstOrDefault();
                                        interactionModel.DateSubmitted = (checkListUserResult1 != null) ? checkListUserResult1.SubmittedDate : DateTime.MinValue.Date;

                                        var checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().Where(c => c.DocumentId == query.DocumentId && c.IsGlobalAccessed && c.UserId == userId).ToList();
                                        var checkListChapter = _checkListChapterRepository.List.AsQueryable().Where(c => c.CheckListId == query.DocumentId && !c.Deleted).ToList();
                                        interactionModel.ChecksCompleted = $"{checkListChapterUserResult.Where(c => c.IsChecked == true).Count()}/{checkListChapter.Count()}";
                                    }

                                    if (documentUsages.Count() > 0)
                                    {
                                        var userDocUsage = documentUsages.Where(x => x.UserId == userId).FirstOrDefault();
                                        interactionModel.Duration = userDocUsage.Duration;
                                        interactionModel.ViewDate = userDocUsage.ViewDate;
                                    }
                                }
                                vm.GlobalInteractions.Add(interactionModel);
                            }
                        }
                    }
                    break;
                case DocumentType.Policy:
                    {
                        var responses = (from u in _responseRepository.Get(r =>
                                r.PolicyId == query.DocumentId && r.IsGlobalAccessed)
                                         group u by u.UserId
                            into gr
                                         select gr.OrderByDescending(u => u.Created).FirstOrDefault()).ToList();

                        var respondedUserIds = responses.Select(r => r.UserId).Distinct();

                        foreach (var response in responses)
                        {
                            var user = _userRepository.Find(response.UserId.AsGuid());
                            if (user != null)
                            {
                                vm.GlobalInteractions.Add(new InteractionReportDetailViewModel.InteractionDetailModel
                                {
                                    UserId = user.Id.ToString(),
                                    Name = $"{user.FirstName} {user.LastName}",
                                    Group = user.Group?.Title,
                                    IDNumber = user.IDNumber,
                                    Status = !response.Response.HasValue ? "View Later" : response.Response.Value ? "Yes" : "No"
                                });
                            }
                        }
                    }
                    break;
                case DocumentType.Test:
                    {
                        var test = _testRepository.Find(query.DocumentId);
                        var passMarks = Math.Ceiling(test.PassMarks / 100 * test.Questions.Sum(x => Math.Max(1, x.AnswerWeightage)));
                        vm.PassRequirement = $"{(test.PassMarks / 100).ToString("P")} ({passMarks}/{test.Questions.Sum(x => Math.Max(1, x.AnswerWeightage))})";

                        foreach (var userId in assignedUserIds)
                        {
                            var user = _userRepository.Find(userId.AsGuid());
                            if (user != null)
                            {
                                var interaction = new InteractionReportDetailViewModel.InteractionDetailModel
                                {
                                    Name = $"{user.FirstName} {user.LastName}",
                                    Group = user.Group?.Title,
                                    IDNumber = user.IDNumber
                                };
                                var results = _resultRepository.List
                                    .Where(r => r.UserId == userId && r.IsGloballyAccessed && r.TestId == query.DocumentId && !r.Deleted)
                                    .OrderBy(r => r.Created).ToList();
                                if (results.Any())
                                {
                                    var test_result = results.FirstOrDefault();
                                    interaction.Duration = test_result.TimeTaken;
                                    interaction.DateCompleted = test_result.Created.DateTime;
                                    interaction.Status = results.Any(r => r.Passed) ? "Passed" : "Failed";
                                    interaction.Result1 =
                                        $"{results[0].Score}/{results[0].Total} ({results[0].Percentage.ToString("G")}%)";
                                    interaction.ResultId1 = results[0].Id;
                                    if (results.Count() > 1)
                                    {
                                        interaction.Result2 =
                                            $"{results[1].Score}/{results[1].Total} ({results[1].Percentage.ToString("G")}%)";
                                        interaction.ResultId2 = results[1].Id;
                                    }
                                    if (results.Count() > 2)
                                    {
                                        interaction.Result3 =
                                            $"{results[2].Score}/{results[2].Total} ({results[2].Percentage.ToString("G")}%)";
                                        interaction.ResultId3 = results[2].Id;
                                    }
                                }
                                else
                                {
                                    interaction.Status = "Yet to Interact";
                                }

                                vm.GlobalInteractions.Add(interaction);
                            }
                        }
                    }
                    break;
                case DocumentType.Certificate:
                case DocumentType.Unknown:
                default:
                    break;
            }
            #endregion
            vm.GlobalInteractions = vm.GlobalInteractions.OrderBy(i => i.Name).ToList();
            vm.Interactions = vm.Interactions.OrderBy(i => i.Name).ToList();

            ////code added by neeraj
            // var du = documentUsagesRepo.Where(u => !u.IsGlobalAccessed).ToList();

            //var iui = du.Select(u => u.UserId).Distinct().ToList();

            //foreach (var i in vm.Interactions)
            //{
            //    foreach (var userId in iui)
            //    {
            //        //below code added by neeraj
            //        if (i.UserId == userId.ToString())
            //        {

            //            var groupList = _standardUserGroupRepository.Get(c => c.UserId.ToString() == userId.ToString()).ToList();

            //            string name = null;
            //            //List<string> name = new List<string>();
            //            if (groupList.Count > 0)
            //            {
            //                foreach (var g in groupList)
            //                {
            //                    foreach (var gl in groups)
            //                    {
            //                        if (gl.Id == g.GroupId)
            //                        {
            //                            if (name != null)
            //                                name = name + "," + gl.Title;
            //                            else name = name + gl.Title;
            //                        }
            //                    }
            //                }
            //            }
            //            if (name != null)
            //            {
            //                i.Group = name;
            //            }
            //            // code end

            //            var interacted = iui.Contains(userId);
            //            if (interacted)
            //            {
            //                if (du.Count() > 0)
            //                {
            //                    var userDocUsage = du.Where(x => x.UserId == userId).FirstOrDefault();
            //                    i.Duration = userDocUsage.Duration;
            //                    i.ViewDate = userDocUsage.ViewDate;
            //                }
            //            }

            //        }

            //    }
            //}

            return vm;
        }
    }
}