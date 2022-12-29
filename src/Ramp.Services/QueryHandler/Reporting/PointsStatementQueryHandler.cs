using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Common;
using Common.Command;
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
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.Security;
using Ramp.Contracts.ViewModel;
using VirtuaCon;
using VirtuaCon.Collections;

using StandardUserGroup = Domain.Customer.Models.Groups.StandardUserGroup;

namespace Ramp.Services.QueryHandler.Reporting
{
	public class PointsStatementQueryHandler :
		IQueryHandler<PointsStatementQuery, PointsStatementViewModel>
	{
		private readonly IQueryExecutor _queryExecutor;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ITransientReadRepository<DocumentCategory> _categoryRepository;
		private readonly ITransientReadRepository<StandardUser> _userRepository;
		private readonly ITransientReadRepository<Test_Result> _testResultRepository;
		private readonly ITransientReadRepository<DocumentUsage> _usageRepository;
		private readonly ITransientReadRepository<PolicyResponse> _policyResponseRepository;
		private readonly ITransientReadRepository<Memo> _memoRespository;
		private readonly ITransientReadRepository<Policy> _policyRepository;
		private readonly ITransientReadRepository<Test> _testRepository;
		private readonly ITransientReadRepository<Label> _labelrepository;
		private readonly ITransientReadRepository<CheckListUserResult> _checklistUserResultRepository;
		private readonly ITransientReadRepository<TrainingManual> _trainingManualRepository;
		private readonly ITransientReadRepository<CheckList> _checkListRepository;
		private readonly IQueryHandler<CustomerCompanyQueryParameter, CompanyViewModelLong> _companyQueryHandler;
		private readonly IQueryHandler<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>> _groupsQueryHandler;

		private readonly IRepository<StandardUserGroup> _standardUserGroupRepository;

		public PointsStatementQueryHandler(
			IRepository<StandardUserGroup> standardUserGroupRepository,
			IQueryExecutor queryExecutor,
			ICommandDispatcher commandDispatcher,
			ITransientReadRepository<DocumentCategory> categoryRepository,
			ITransientReadRepository<StandardUser> userRepository,
			ITransientReadRepository<Test_Result> testResultRepository,
			ITransientReadRepository<DocumentUsage> usageRepository,
			ITransientReadRepository<PolicyResponse> policyResponseRepository,
			ITransientReadRepository<Memo> memoRespository,
			ITransientReadRepository<Policy> policyRepository,
			ITransientReadRepository<Test> testRepository,
			ITransientReadRepository<CheckListUserResult> checklistUserResultRepository,
			ITransientReadRepository<TrainingManual> trainingManualRepository,
			ITransientReadRepository<CheckList> checkListRepository,
			ITransientReadRepository<Label> labelrepository,
			IQueryHandler<CustomerCompanyQueryParameter, CompanyViewModelLong> companyQueryHandler,
			IQueryHandler<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>> groupsQueryHandler)
		{
			_queryExecutor = queryExecutor;
			_commandDispatcher = commandDispatcher;
			_categoryRepository = categoryRepository;
			_userRepository = userRepository;
			_testResultRepository = testResultRepository;
			_usageRepository = usageRepository;
			_policyResponseRepository = policyResponseRepository;
			_companyQueryHandler = companyQueryHandler;
			_groupsQueryHandler = groupsQueryHandler;
			_memoRespository = memoRespository;
			_policyRepository = policyRepository;
			_testRepository = testRepository;
			_trainingManualRepository = trainingManualRepository;
			_checkListRepository = checkListRepository;
			_labelrepository = labelrepository;
			_checklistUserResultRepository = checklistUserResultRepository;
			_standardUserGroupRepository = standardUserGroupRepository;

		}
		private PointsStatementViewModel SetupViewModel(PointsStatementQuery query)
		{
			var vm = new PointsStatementViewModel();
			vm.TrainingLabelDict = _labelrepository.Get(c => c.Deleted == false).ToDictionary(x => x.Name, x => x.Name);

			vm.DocumentTypesDict = Enum.GetValues(typeof(DocumentType))
				.Cast<DocumentType>()
				.Where(x => (int)x != 0 && (int)x != 5 && (int)x != 7)
				.ToDictionary(
					x => (int)x,
					x => VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(x));

			vm.Companies = _companyQueryHandler.ExecuteQuery(new CustomerCompanyQueryParameter
			{
				IsForAdmin = !query.ProvisionalCompanyId.HasValue,
				ProvisionalCompanyId = query.ProvisionalCompanyId ?? Guid.Empty
			}).CompanyList;

			vm.Categories = _categoryRepository.List.AsQueryable().Select(c => new DocumentCategoryViewModel
			{
				Id = c.Id,
				Title = c.Title,
				ParentId = c.ParentId
			}).OrderBy(x => x.Title).ToList();

			vm.Groups = _groupsQueryHandler.ExecuteQuery(new AllGroupsByCustomerAdminQueryParameter());
			//below code changed by neeraj removed  && u.Group != null from where

			var allUsers = _userRepository.List.AsQueryable().Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)) && u.IsActive);
			//if (query.GroupId.HasValue)
			//	allUsers = allUsers.Where(u => u.Group.Id.Equals(query.GroupId.Value));
			vm.Users = allUsers.Select(u => new UserViewModel
			{
				Id = u.Id,
				EmployeeNo = u.EmployeeNo,
				GroupName = u.Group.Title,
				SelectedGroupId = u.Group.Id,
				FullName = u.FirstName + " " + u.LastName
			}).OrderBy(u => u.FullName).ToList();
			var tags = string.Join(",", query.TrainingLabels);
			vm.Users = vm.Users.Where(x => tags.Contains(x.TrainingLabels)).ToList(); ;

			//below code added by neeraj

			List<string> userIds = new List<string>();

			foreach (var u in vm.Data)
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
					//var x = String.Join(",", query.GroupId);
					//if (x.Contains(name))
					//{
					//	userIds.Add(u.ToString());
					//}
					u.User.GroupTitle = name;
				}


			}

			//code end

			return vm;
		}
		private IList<Document_DocumentUsageModel<T>> GetDocumentModelsFromUsage<TEntity, T>(string userId, IEnumerable<AssignedDocument> assignedDocuments,
																								ITransientReadRepository<TEntity> repository,
																								Expression<Func<TEntity, T>> projection)
		  where TEntity : IdentityModel<string>
		  where T : IdentityModel<string>
		{
			if (assignedDocuments.Any() && !string.IsNullOrWhiteSpace(userId))
			{
				var ids = assignedDocuments.Select(ad => ad.DocumentId).ToList();
				var docType = assignedDocuments.First().DocumentType;
				var usages = _usageRepository.List.AsQueryable().Where(x => x.UserId == userId && x.DocumentType == docType && ids.Contains(x.DocumentId)).ToList();
				return repository.List.AsQueryable().Where(x => ids.Contains(x.Id)).Select(projection).ToList().Select(doc => new Document_DocumentUsageModel<T>
				{
					Document = doc,
					Usages = usages.Where(x => x.DocumentId == doc.Id).OrderBy(x => x.ViewDate).ToList()
				}).ToList();
			}
			return new List<Document_DocumentUsageModel<T>>();
		}

		public PointsStatementViewModel ExecuteQuery(PointsStatementQuery query)
		{
			if (query.CompanyId != null)
			{
				_categoryRepository.SetCustomerCompany(query.CompanyId.ToString());
				_userRepository.SetCustomerCompany(query.CompanyId.ToString());
				_testResultRepository.SetCustomerCompany(query.CompanyId.ToString());
				_usageRepository.SetCustomerCompany(query.CompanyId.ToString());
				_memoRespository.SetCustomerCompany(query.CompanyId.ToString());
				_policyRepository.SetCustomerCompany(query.CompanyId.ToString());
				_testRepository.SetCustomerCompany(query.CompanyId.ToString());
				_trainingManualRepository.SetCustomerCompany(query.CompanyId.ToString());
				_checkListRepository.SetCustomerCompany(query.CompanyId.ToString());
			}

			var vm = SetupViewModel(query);
			if (!query.WithData)
				return vm;

			query.FromDate = query.FromDate.AtBeginningOfDay();
			query.ToDate = query.ToDate.AtEndOfDay();

			var policyResponses = new List<PolicyResponse>();
			var testResults = new List<Test_Result>();
			var documentUsages = new List<DocumentUsage>();
			var documents = new List<DocumentListModel>();


			var documentTypes = new List<DocumentType>();
			if (query.DocumentTypes == null)
				query.DocumentTypes = new List<DocumentType>();
			if (!query.DocumentTypes.Any())
				documentTypes.AddRange(new[] { DocumentType.Memo, DocumentType.Policy, DocumentType.Test, DocumentType.TrainingManual, DocumentType.Checklist });
			else
				documentTypes = query.DocumentTypes.ToList();


			// Memo and Training Manual
			var dUsages = _usageRepository.Get(u => documentTypes.Contains(u.DocumentType) && u.DocumentType != DocumentType.Policy);
			if (query.UserIds.Any())
				dUsages = dUsages.Where(x => query.UserIds.Contains(x.UserId));
			if (query.FromDate.HasValue)
				dUsages = dUsages.Where(x => x.ViewDate >= query.FromDate.Value);
			if (query.ToDate.HasValue)
				dUsages = dUsages.Where(x => x.ViewDate <= query.ToDate.Value);

			dUsages.GroupBy(x => x.DocumentType).ToList().ForEach(usage_docTypeGrouping => {
				switch (usage_docTypeGrouping.Key)
				{
					case DocumentType.Memo:
						// var memoIds = usage_docTypeGrouping.Select(x => x.DocumentId).ToList();
						var memoDetails = usage_docTypeGrouping.OrderByDescending(c => c.ViewDate).Select(x => new { x.ViewDate, x.DocumentId, x.IsGlobalAccessed }).GroupBy(c => new { c.IsGlobalAccessed, c.DocumentId }).Select(f => f.FirstOrDefault()).ToList();
						//var memos = _memoRespository.List.AsQueryable().Where(x => memoIds.Contains(x.Id) && x.Points > 0).Include(x => x.Category);
						var memosList = new List<Memo>();
						foreach (var item in memoDetails)
						{

							var memo = _memoRespository.List.AsQueryable().Where(x => x.Id == item.DocumentId && x.Points > 0 && !x.Deleted).Include(x => x.Category).FirstOrDefault();
							if (memo != null)
							{
								memo.IsGlobalAccessed = item.IsGlobalAccessed;
								memosList.Add(memo);
							}

						}
						var memos = memosList.AsQueryable();
						if (query.CategoryId.HasValue)
							memos = memos.Where(x => x.CategoryId == query.CategoryId.Value.ToString() && x.Deleted == false);
						documents.AddRange(memos.Select(x => new DocumentListModel
						{
							Id = x.Id,
							TrainingLabels = x.TrainingLabels,
							CategoryId = x.CategoryId,
							Category = x.Category == null ? null : new CategoryViewModelShort { Id = x.Category.Id, Name = x.Category.Title },
							Title = x.Title,
							DocumentType = DocumentType.Memo,
							Points = x.Points,
							IsGlobalAccessed = x.IsGlobalAccessed
						}).ToList());
						documentUsages.AddRange(usage_docTypeGrouping);
						break;
					case DocumentType.TrainingManual:
						// var tmIds = usage_docTypeGrouping.Select(x => x.DocumentId).ToList();
						// var tms = _trainingManualRepository.List.AsQueryable().Where(x => tmIds.Contains(x.Id) && x.Points > 0).Include(x => x.Category);
						var tmDetails = usage_docTypeGrouping.OrderByDescending(c => c.ViewDate).Select(x => new { x.ViewDate, x.DocumentId, x.IsGlobalAccessed }).GroupBy(c => new { c.IsGlobalAccessed, c.DocumentId }).Select(f => f.FirstOrDefault()).ToList();
						var tmsList = new List<TrainingManual>();
						foreach (var item in tmDetails)
						{

							var tm = _trainingManualRepository.List.AsQueryable().Where(x => x.Id == item.DocumentId && x.Points > 0 && !x.Deleted).Include(x => x.Category).FirstOrDefault();
							if (tm != null)
							{
								tm.IsGlobalAccessed = item.IsGlobalAccessed;
								tmsList.Add(tm);
							}

						}
						var tms = tmsList.AsQueryable();
						if (query.CategoryId.HasValue)
							tms = tms.Where(x => x.DocumentCategoryId == query.CategoryId.Value.ToString() && x.Deleted == false);
						documents.AddRange(tms.Select(x => new DocumentListModel
						{
							Id = x.Id,
							TrainingLabels = x.TrainingLabels,
							CategoryId = x.DocumentCategoryId,
							Category = x.Category == null ? null : new CategoryViewModelShort { Id = x.Category.Id, Name = x.Category.Title },
							Title = x.Title,
							DocumentType = DocumentType.TrainingManual,
							Points = x.Points,
							IsGlobalAccessed = x.IsGlobalAccessed,
						}).ToList());
						documentUsages.AddRange(usage_docTypeGrouping);
						break;
					case DocumentType.Checklist:
						// var chlIds = usage_docTypeGrouping.Select(x => x.DocumentId).ToList();
						var chlDetails = usage_docTypeGrouping.OrderByDescending(c => c.ViewDate).Select(x => new { x.ViewDate, x.DocumentId, x.IsGlobalAccessed }).GroupBy(c => new { c.IsGlobalAccessed, c.DocumentId }).Select(f => f.FirstOrDefault()).ToList();
						var chlResult = _checklistUserResultRepository.List.AsQueryable().Where(c => c.Status == true).Select(x => x.DocumentId).ToList();

						// var chl = _checkListRepository.List.AsQueryable().Where(x => chlResult.Contains(x.Id) && chlIds.Contains(x.Id) && x.Points > 0).Include(x => x.Category);
						var chlList = new List<CheckList>();
						foreach (var item in chlDetails)
						{

							var check = _checkListRepository.List.AsQueryable().Where(x => chlResult.Contains(x.Id) && x.Id == item.DocumentId && x.Points > 0 && !x.Deleted).Include(x => x.Category).FirstOrDefault();
							if (check != null)
							{
								check.IsGlobalAccessed = item.IsGlobalAccessed;
								chlList.Add(check);
							}

						}
						var chl = chlList.AsQueryable();

						if (query.CategoryId.HasValue)
							chl = chl.Where(x => x.DocumentCategoryId == query.CategoryId.Value.ToString() && x.Deleted == false);
						documents.AddRange(chl.Select(x => new DocumentListModel
						{
							Id = x.Id,
							TrainingLabels = x.TrainingLabels,
							CategoryId = x.DocumentCategoryId,
							Category = x.Category == null ? null : new CategoryViewModelShort { Id = x.Category.Id, Name = x.Category.Title },
							Title = x.Title,
							DocumentType = DocumentType.Checklist,
							Points = x.Points,
							IsGlobalAccessed = x.IsGlobalAccessed,
						}).ToList());
						documentUsages.AddRange(usage_docTypeGrouping);
						break;
					default: break;
				}
			});

			if (documentTypes.Contains(DocumentType.Policy))
			{
				var applicablePolicies = _policyRepository.List.AsQueryable().Where(x => x.Points > 0 && x.Deleted == false).Include(x => x.Category);
				if (query.CategoryId.HasValue)
					applicablePolicies = applicablePolicies.Where(x => x.CategoryId == query.CategoryId.Value.ToString());
				documents.AddRange(applicablePolicies.Select(x => new DocumentListModel
				{
					Id = x.Id,
					CategoryId = x.CategoryId,
					TrainingLabels = x.TrainingLabels,
					Category = x.Category == null ? null : new CategoryViewModelShort { Id = x.Category.Id, Name = x.Category.Title },
					Title = x.Title,
					DocumentType = DocumentType.Policy,
					Points = x.Points,
					IsGlobalAccessed = x.IsGlobalAccessed,
				}).ToList());
				var ccc = _policyResponseRepository.List.AsQueryable().Where(x => applicablePolicies.Any(p => p.Id == x.PolicyId)).ToList();
				var pResponses = _policyResponseRepository.List.AsQueryable().Where(x => applicablePolicies.Any(p => p.Id == x.PolicyId));
				if (query.UserIds.Any())
					pResponses = pResponses.Where(x => query.UserIds.Contains(x.UserId));
				if (query.FromDate.HasValue)
					pResponses = pResponses.Where(x => x.Created >= query.FromDate.Value);
				if (query.ToDate.HasValue)
					pResponses = pResponses.Where(x => x.Created <= query.ToDate.Value);
				policyResponses = pResponses.GroupBy(r => new { r.UserId, r.PolicyId, r.IsGlobalAccessed }).Select(g => g.OrderByDescending(r => r.Created).FirstOrDefault()).ToList();
			}

			if (documentTypes.Contains(DocumentType.Test))
			{
				var applicableTests = _testRepository.List.AsQueryable().Where(x => x.Points > 0 && x.Deleted == false).Include(x => x.Category);
				if (query.CategoryId.HasValue)
					applicableTests = applicableTests.Where(x => x.CategoryId == query.CategoryId.Value.ToString());
				documents.AddRange(applicableTests.Select(x => new DocumentListModel
				{
					Id = x.Id,
					CategoryId = x.CategoryId,
					TrainingLabels = x.TrainingLabels,
					Category = x.Category == null ? null : new CategoryViewModelShort { Id = x.Category.Id, Name = x.Category.Title },
					Title = x.Title,
					DocumentType = DocumentType.Test,
					Points = x.Points,
					IsGlobalAccessed = x.IsGlobalAccessed,
					IsCertificate = x.CertificateId != null ? true : false
				}).ToList());

				var tResults = _testResultRepository.List.AsQueryable().Include(x => x.Test).Where(x => applicableTests.Any(t => t.Id == x.TestId));
				if (query.UserIds.Any())
					tResults = tResults.Where(x => query.UserIds.Contains(x.UserId));
				if (query.FromDate.HasValue)
					tResults = tResults.Where(x => x.Created >= query.FromDate.Value);
				if (query.ToDate.HasValue)
					tResults = tResults.Where(x => x.Created <= query.ToDate.Value);
				if (query.CategoryId.HasValue)
					tResults = tResults.Where(x => x.Test.CategoryId == query.CategoryId.Value.ToString());
				testResults = tResults.Where(x => x.Deleted == false).ToList();
			}

			var userIds = new string[] { }
						  .Union(testResults.Select(r => r.UserId).ToArray())
						  .Union(documentUsages.Select(u => u.UserId).ToArray())
						  .Union(policyResponses.Select(r => r.UserId).ToArray())
						  .Intersect(vm.Users.Select(x => x.Id.ToString()).ToArray());

			var users = _userRepository.List.AsQueryable()
											.Where(u => userIds.Contains(u.Id.ToString()))
											.Select(u => new PointsStatementViewModel.UserDetail
											{
												Id = u.Id,
												EmployeeNo = u.EmployeeNo,
												FullName = u.FirstName + " " + u.LastName,
												GroupTitle = u.Group.Title,
												Email = u.EmailAddress,
												Gender = u.Gender.ToString(),
												IDNumber = u.IDNumber,
												MobileNumber = u.MobileNumber,
												Race = u.RaceCodeId.ToString()
											})
											.ToList();

			var result = new ConcurrentBag<PointsStatementViewModel.DataItem>();
			var userLoopResult = Parallel.ForEach(users, user => {
				var userUsage = documentUsages
							   .Where(u => u.UserId == user.Id.ToString() && documents.Any(d => d.Id == u.DocumentId))
							   .GroupBy(u => new { u.DocumentId, u.DocumentType, u.IsGlobalAccessed })
							   .Select(g => g.OrderByDescending(u => u.ViewDate).FirstOrDefault()).ToList();
				var userResults = testResults.Where(r => r.UserId == user.Id.ToString() && r.Test != null && documents.Any(d => d.Id == r.TestId))
											 .GroupBy(x => new { x.TestId, x.IsGloballyAccessed }).Select(x => new { TestId = x.Key.TestId, IsGlobalAccessed = x.Key.IsGloballyAccessed, LastResult = x.OrderBy(r => r.Created).LastOrDefault(), LastPassedResult = x.OrderBy(r => r.Created).LastOrDefault(q => q.Passed) }).ToList();
				var userPolicyResponses = policyResponses
										  .Where(r => r.UserId == user.Id.ToString() && documents.Any(d => d.Id == r.PolicyId))
										  .GroupBy(x => new { x.PolicyId, x.IsGlobalAccessed }).Select(x => new { PolicyId = x.Key.PolicyId, IsGlobalAccessed = x.Key.IsGlobalAccessed, LastAction = x.OrderBy(r => r.Created).LastOrDefault() }).ToList();
				var testResultLoopResult = Parallel.ForEach(userResults, rg => {
					var document = documents.Where(x => x.Deleted == false).FirstOrDefault(d => d.Id == rg.TestId);
					result.Add(new PointsStatementViewModel.DataItem
					{
						Date = rg.LastPassedResult == null ? rg.LastResult != null ? rg.LastResult.Created.DateTime.ToLocalTime() : DateTime.MinValue.ToLocalTime() : rg.LastPassedResult.Created.DateTime.ToLocalTime(),
						User = user,
						TrainingLabels = document.TrainingLabels,
						DocumentTitle = document.Title,
						DocumentType = document.DocumentType,
						Category = document.Category?.Name ?? "Deleted",
						IsGlobalAccessed = rg.IsGlobalAccessed,
						IsCertificate = document.IsCertificate,
						Result = rg.LastPassedResult == null ? PointsStatementResult.Failed : rg.LastPassedResult.Passed ? PointsStatementResult.Passed : PointsStatementResult.Failed,
						Points = rg.LastPassedResult == null ? 0 : rg.LastPassedResult.Passed ? document.Points : 0
					});
				});
				var documentUsageLoopResult = Parallel.ForEach(userUsage, u => {
					var document = documents.Where(x => x.Deleted == false).FirstOrDefault(d => d.Id == u.DocumentId);
					result.Add(new PointsStatementViewModel.DataItem
					{
						Category = document.Category?.Name ?? "Deleted",
						Date = u.ViewDate.ToLocalTime(),
						Duration = u.Duration,
						TrainingLabels = document.TrainingLabels,
						DocumentType = document.DocumentType,
						DocumentTitle = document.Title,
						IsCertificate = document.IsCertificate,
						IsGlobalAccessed = u.IsGlobalAccessed,
						Result = PointsStatementResult.Viewed,
						Points = document.Points,
						User = user
					});
				});
				var policyLoopResult = Parallel.ForEach(userPolicyResponses, rg => {
					var document = documents.Where(x => x.Deleted == false).FirstOrDefault(d => d.Id == rg.PolicyId);
					result.Add(new PointsStatementViewModel.DataItem
					{
						Category = document.Category?.Name ?? "Deleted",
						Date = rg.LastAction.Created.ToLocalTime(),
						TrainingLabels = document.TrainingLabels,
						DocumentType = document.DocumentType,
						DocumentTitle = document.Title,
						IsGlobalAccessed = rg.IsGlobalAccessed,
						Points = rg.LastAction.Response.HasValue && rg.LastAction.Response.Value ? document.Points : 0,
						Result = !rg.LastAction.Response.HasValue ? PointsStatementResult.Later : rg.LastAction.Response.Value ? PointsStatementResult.Yes : PointsStatementResult.No,
						User = user
					});
				});

				while (!testResultLoopResult.IsCompleted || !documentUsageLoopResult.IsCompleted || !policyLoopResult.IsCompleted) { continue; }

			});
			while (!userLoopResult.IsCompleted) { continue; }

			if (query.GlobalAccess.Any() && query.GlobalAccess.Count() == 1 && query.EnableGlobalAccessDocuments)
			{

				if (query.GlobalAccess.Contains(1))
					vm.Data = result.OrderByDescending(d => d.Date).ThenBy(d => d.User.FullName).Where(d => d.IsGlobalAccessed).ToList();
				else
					vm.Data = result.OrderByDescending(d => d.Date).ThenBy(d => d.User.FullName).Where(d => !d.IsGlobalAccessed).ToList();
			}
			else if (query.GlobalAccess.Count() == 2)
			{
				vm.Data = result.OrderByDescending(d => d.Date).ThenBy(d => d.User.FullName).ToList();
			}
			else if (query.GlobalAccess.Count() == 0 && query.EnableGlobalAccessDocuments)
			{
				vm.Data = result.OrderByDescending(d => d.Date).ThenBy(d => d.User.FullName).ToList();
			}
			else if (query.EnableGlobalAccessDocuments == false)
			{
				vm.Data = result.OrderByDescending(d => d.Date).ThenBy(d => d.User.FullName).Where(c => !c.IsGlobalAccessed).ToList();

			}
			if (query.CategoryId.HasValue)
				vm.Data = vm.Data.Where(x => x.Category == vm.Categories.FirstOrDefault(c => c.Id == query.CategoryId.Value.ToString()).Title).ToList();

			if (query.TrainingLabels.Any())
			{
				var labels = string.Join(",", query.TrainingLabels);
				vm.Data = result.OrderByDescending(d => d.Date).ThenBy(d => d.User.FullName).Where(c => c.TrainingLabels.Contains(labels)).ToList();
			}

			_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

			//below code added by neeraj

			foreach (var u in vm.Data)
			{

				var groupList = _standardUserGroupRepository.List.Where(c => c.UserId.ToString() == u.User.Id.ToString()).ToList();

				string name = null;
				if (groupList.Count > 0)
				{
					foreach (var g in groupList)
					{
						if (name != null)
							name = name + "," + g.GroupId;
						else name = name + g.GroupId;
					}
					//var x = String.Join(",", query.GroupId);
					//if (x.Contains(name))
					//{
					//	userIds.Add(u.ToString());
					//}
					u.User.GroupTitle = name;
				}


			}

			//code end



			return vm;
		}
	}
}