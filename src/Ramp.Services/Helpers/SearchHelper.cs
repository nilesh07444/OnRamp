using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using iTextSharp.text.pdf;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using VirtuaCon;
using Domain.Customer.Models.CheckLists;

//using Microsoft.Office.Core;

namespace Ramp.Services.Helpers
{
    public class SearchHelper
    {
        private readonly GlobalSearchQueryParameter _filter;
        private SearchLocations _currentLocation;
        private readonly IRepository<TrainingGuide> _adminGuideRepository;
        private readonly IRepository<AssignedTrainingGuides> _standardGuideRepository;
        private readonly IRepository<TrainingTest> _adminTestRepository;
        private readonly IRepository<TestAssigned> _standardTestRepository;
        private readonly IRepository<TrainingManual> _trainingManualRepository;
        private readonly IRepository<Memo> _memoRepository;
        private readonly IRepository<Policy> _policyRepository;
		private readonly IRepository<CheckList> _checklistRepository;
		private readonly IRepository<Test> _testRepository;
        private readonly IRepository<AssignedDocument> _assignedDocumentRepository;

        private readonly Dictionary<SearchLocations, searchResults> _locations = new Dictionary<SearchLocations, searchResults>()
        {
            {SearchLocations.TrainingManuals, new searchResults {FoundInBase = "Training Manual: ", SearchUrlBase = "~/TrainingManual/Preview/"} },
            {SearchLocations.TrainingManualChapters, new searchResults {FoundInBase = "Training Manual Chapter: ", SearchUrlBase = "~/TrainingManual/Preview/"} },
            {SearchLocations.TrainingManualChapterUploads, new searchResults {FoundInBase = "Training Manual Chapter Upload: ", SearchUrlBase = "~/TrainingManual/Preview/"} },

			{SearchLocations.Checklists, new searchResults {FoundInBase = "Checklist: ", SearchUrlBase = "~/CheckList/Preview/"} },
			{SearchLocations.ChecklistChapters, new searchResults {FoundInBase = "Checklist Chapter: ", SearchUrlBase = "~/CheckList/Preview/"} },
			{SearchLocations.ChecklistChapterUploads, new searchResults {FoundInBase = "Checklist Chapter Upload: ", SearchUrlBase = "~/CheckList/Preview/"} },

			{SearchLocations.Memos, new searchResults {FoundInBase = "Memo: ", SearchUrlBase = "~/Memo/Preview/"} },
            {SearchLocations.MemoContentBoxes, new searchResults {FoundInBase = "Memo Content Box: ", SearchUrlBase = "~/Memo/Preview/"} },
            {SearchLocations.MemoContentBoxUploads, new searchResults {FoundInBase = "Memo Content Box Upload: ", SearchUrlBase = "~/Memo/Preview/"} },
            {SearchLocations.Policies, new searchResults {FoundInBase = "Policy: ", SearchUrlBase = "~/Policy/Preview/"} },
			{SearchLocations.PolicyContentBoxes, new searchResults {FoundInBase = "Policy Content Box: ", SearchUrlBase = "~/Policy/Preview/"} },
            {SearchLocations.PolicyContentBoxUploads, new searchResults {FoundInBase = "Policy Content Box Upload: ", SearchUrlBase = "~/Policy/Preview/"} },
            {SearchLocations.Tests,new searchResults() { FoundInBase = "Test: ",SearchUrlBase = "~/Test/Preview/"} },
            {SearchLocations.TestQuestions,new searchResults() { FoundInBase = "Test Question: ", SearchUrlBase = "~/Test/Preview/"} },
            {SearchLocations.TestAnswers,new searchResults() {FoundInBase = "Test Question Answer: ", SearchUrlBase = "~/Test/Preview/" } }
        };

        public SearchHelper(GlobalSearchQueryParameter filter,
            IRepository<TrainingGuide> adminGuideRepository,
            IRepository<AssignedTrainingGuides> standardGuideRepository,
            IRepository<TrainingTest> adminTestRepository,
            IRepository<TestAssigned> standardTestRepository,
            IRepository<TrainingManual> trainingManualRepository,
            IRepository<Memo> memoRepository,
            IRepository<Policy> policyRepository,
			IRepository<CheckList> checklistRepository,
			IRepository<Test> testRepository,
            IRepository<AssignedDocument> assignedDocumentRepository)
        {
            _filter = filter;
            _adminGuideRepository = adminGuideRepository;
            _adminTestRepository = adminTestRepository;
            _standardGuideRepository = standardGuideRepository;
            _standardTestRepository = standardTestRepository;
            _trainingManualRepository = trainingManualRepository;
            _memoRepository = memoRepository;
            _policyRepository = policyRepository;
            _testRepository = testRepository;
			_checklistRepository = checklistRepository;
			_assignedDocumentRepository = assignedDocumentRepository;
        }

        public List<GlobalSearchViewModel> Search()
        {
            List<GlobalSearchViewModel> result = new List<GlobalSearchViewModel>();
            switch (_filter.SearchType)
            {
                case SearchTypes.All:
                    result.AddRange(searchTrainingManuals());
                    result.AddRange(searchTrainingManualChapters());
                    result.AddRange(searchTrainingManualChapterUploads());
					result.AddRange(searchChecklists());
					result.AddRange(searchChecklistChapters());
					result.AddRange(searchChecklistChapterUploads());
					result.AddRange(searchMemos());
                    result.AddRange(searchMemoContentBoxes());
                    result.AddRange(searchMemoContentBoxUploads());
                    result.AddRange(searchPolicies());
                    result.AddRange(searchPolicyContentBoxes());
                    result.AddRange(searchPolicyContentBoxUploads());
                    result.AddRange(searchTests());
                    result.AddRange(searchTestQuestions());
                    result.AddRange(searchTestAnswers());
                    break;

                default:
                    break;
            }
            return result;
        }

        private List<GlobalSearchViewModel> searchTrainingManuals()
        {
            _currentLocation = SearchLocations.TrainingManuals;
            var result = new List<GlobalSearchViewModel>();
            var trainingManuals = _trainingManualRepository.List.Where(tm => !tm.Deleted);

            if (_assignedDocumentRepository != null)
            {
                var assignedIds = _assignedDocumentRepository.List
                    .Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.TrainingManual && !a.Deleted)
                    .Select(a => a.DocumentId);
                trainingManuals = trainingManuals.Where(tm => assignedIds.Contains(tm.Id));
            }

            trainingManuals.Where(tm => IsMatch(tm.Title) || IsMatch(tm.ReferenceId) || IsMatch(tm.Description))
                .ForEach(tm => result.Add(getSearchViewResult(tm.Title, tm.Id)));

            return result;
        }

        private List<GlobalSearchViewModel> searchTrainingManualChapters()
        {
            _currentLocation = SearchLocations.TrainingManualChapters;
            var result = new List<GlobalSearchViewModel>();
            var trainingManuals = _trainingManualRepository.List.Where(tm => !tm.Deleted);

            if (_assignedDocumentRepository != null)
            {
                var assignedIds = _assignedDocumentRepository.List
                    .Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.TrainingManual && !a.Deleted)
                    .Select(a => a.DocumentId);
                trainingManuals = trainingManuals.Where(tm => assignedIds.Contains(tm.Id));
            }

            foreach (var tm in trainingManuals)
            {
                tm.Chapters.Where(c => !c.Deleted && (IsMatch(c.Title) || IsMatch(c.Content)))
                    .ForEach(c => result.Add(getSearchViewResult(
                        $"{tm.Title} - {(tm.Chapters.OrderBy(q => q.Number).First().Number == 0 ? c.Number + 1 : c.Number)}. {c.Title}",
                        $"{tm.Id}#{c.Number}")));
            }

            return result;
        }

        private List<GlobalSearchViewModel> searchTrainingManualChapterUploads()
        {
            _currentLocation = SearchLocations.TrainingManualChapterUploads;
            var result = new List<GlobalSearchViewModel>();
            var trainingManuals = _trainingManualRepository.List.Where(tm => !tm.Deleted);

            if (_assignedDocumentRepository != null)
            {
                var assignedIds = _assignedDocumentRepository.List
                    .Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.TrainingManual && !a.Deleted)
                    .Select(a => a.DocumentId);
                trainingManuals = trainingManuals.Where(tm => assignedIds.Contains(tm.Id));
            }

            foreach (var tm in trainingManuals)
            {
                foreach (var c in tm.Chapters.Where(c => !c.Deleted))
                {
                    c.ContentToolsUploads
                        .Where(u => !u.Deleted && (IsMatch(u.Content) || IsMatch(u.Description) || IsMatch(u.Name)))
                        .ForEach(u => result.Add(getSearchViewResult(
                            $"{tm.Title} - {(tm.Chapters.OrderBy(q => q.Number).First().Number == 0 ? c.Number + 1 : c.Number)}. {c.Title}",
                            $"{tm.Id}#{c.Number}")));
                    c.Uploads
                        .Where(u => !u.Deleted && (IsMatch(u.Content) || IsMatch(u.Description) || IsMatch(u.Name)))
                        .ForEach(u => result.Add(getSearchViewResult(
                            $"{tm.Title} - {(tm.Chapters.OrderBy(q => q.Number).First().Number == 0 ? c.Number + 1 : c.Number)}. {c.Title}",
                            $"{tm.Id}#{c.Number}")));
                }
            }

            return result;
        }


		#region checklist
		private List<GlobalSearchViewModel> searchChecklists() {
			_currentLocation = SearchLocations.Checklists;
			var result = new List<GlobalSearchViewModel>();
			var checklists = _checklistRepository.List.Where(cl => !cl.Deleted);

			if (_assignedDocumentRepository != null) {
				var assignedIds = _assignedDocumentRepository.List
					.Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.Checklist && !a.Deleted)
					.Select(a => a.DocumentId);
				checklists = checklists.Where(tm => assignedIds.Contains(tm.Id));
			}

			checklists.Where(cl => IsMatch(cl.Title) || IsMatch(cl.ReferenceId) || IsMatch(cl.Description))
				.ForEach(tm => result.Add(getSearchViewResult(tm.Title, tm.Id)));

			return result;
		}

		private List<GlobalSearchViewModel> searchChecklistChapters() {
			_currentLocation = SearchLocations.ChecklistChapters;
			var result = new List<GlobalSearchViewModel>();
			var checklists = _checklistRepository.List.Where(cl => !cl.Deleted);

			if (_assignedDocumentRepository != null) {
				var assignedIds = _assignedDocumentRepository.List
					.Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.Checklist && !a.Deleted)
					.Select(a => a.DocumentId);
				checklists = checklists.Where(cl => assignedIds.Contains(cl.Id));
			}

			foreach (var cl in checklists) {
				cl.Chapters.Where(c => !c.Deleted && (IsMatch(c.Title) || IsMatch(c.Content)))
					.ForEach(c => result.Add(getSearchViewResult(
						$"{cl.Title} - {(cl.Chapters.OrderBy(q => q.Number).First().Number == 0 ? c.Number + 1 : c.Number)}. {c.Title}",
						$"{cl.Id}#{c.Number}")));
			}

			return result;
		}

		private List<GlobalSearchViewModel> searchChecklistChapterUploads() {
			_currentLocation = SearchLocations.ChecklistChapterUploads;
			var result = new List<GlobalSearchViewModel>();
			var checklists = _checklistRepository.List.Where(cl => !cl.Deleted);

			if (_assignedDocumentRepository != null) {
				var assignedIds = _assignedDocumentRepository.List
					.Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.Checklist && !a.Deleted)
					.Select(a => a.DocumentId);
				checklists = checklists.Where(cl => assignedIds.Contains(cl.Id));
			}

			foreach (var cl in checklists) {
				foreach (var c in cl.Chapters.Where(c => !c.Deleted)) {
					c.ContentToolsUploads
						.Where(u => !u.Deleted && (IsMatch(u.Content) || IsMatch(u.Description) || IsMatch(u.Name)))
						.ForEach(u => result.Add(getSearchViewResult(
							$"{cl.Title} - {(cl.Chapters.OrderBy(q => q.Number).First().Number == 0 ? c.Number + 1 : c.Number)}. {c.Title}",
							$"{cl.Id}#{c.Number}")));
					c.Uploads
						.Where(u => !u.Deleted && (IsMatch(u.Content) || IsMatch(u.Description) || IsMatch(u.Name)))
						.ForEach(u => result.Add(getSearchViewResult(
							$"{cl.Title} - {(cl.Chapters.OrderBy(q => q.Number).First().Number == 0 ? c.Number + 1 : c.Number)}. {c.Title}",
							$"{cl.Id}#{c.Number}")));
				}
			}

			return result;
		}
		#endregion

		private List<GlobalSearchViewModel> searchMemos()
        {
            _currentLocation = SearchLocations.Memos;
            var result = new List<GlobalSearchViewModel>();
            var memos = _memoRepository.List.Where(m => !m.Deleted);

            if (_assignedDocumentRepository != null)
            {
                var assignedIds = _assignedDocumentRepository.List
                    .Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.Memo && !a.Deleted)
                    .Select(a => a.DocumentId);
                memos = memos.Where(m => assignedIds.Contains(m.Id));
            }

            memos.Where(m => IsMatch(m.Title) || IsMatch(m.ReferenceId) || IsMatch(m.Description))
                .ForEach(m => result.Add(getSearchViewResult(m.Title, m.Id)));

            return result;
        }

        private List<GlobalSearchViewModel> searchMemoContentBoxes()
        {
            _currentLocation = SearchLocations.MemoContentBoxes;
            var result = new List<GlobalSearchViewModel>();
            var memos = _memoRepository.List.Where(m => !m.Deleted);

            if (_assignedDocumentRepository != null)
            {
                var assignedIds = _assignedDocumentRepository.List
                    .Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.Memo && !a.Deleted)
                    .Select(a => a.DocumentId);
                memos = memos.Where(m => assignedIds.Contains(m.Id));
            }

            foreach (var m in memos)
            {
                m.ContentBoxes.Where(c => !c.Deleted && (IsMatch(c.Title) || IsMatch(c.Content)))
                    .ForEach(c => result.Add(getSearchViewResult(
                        $"{m.Title} - {(m.ContentBoxes.OrderBy(q => q.Number).First().Number == 0 ? c.Number + 1 : c.Number)}. {c.Title}",
                        $"{m.Id}#{c.Number}")));
            }

            return result;
        }

        private List<GlobalSearchViewModel> searchMemoContentBoxUploads()
        {
            _currentLocation = SearchLocations.MemoContentBoxUploads;
            var result = new List<GlobalSearchViewModel>();
            var memos = _memoRepository.List.Where(m => !m.Deleted);

            if (_assignedDocumentRepository != null)
            {
                var assignedIds = _assignedDocumentRepository.List
                    .Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.Memo && !a.Deleted)
                    .Select(a => a.DocumentId);
                memos = memos.Where(m => assignedIds.Contains(m.Id));
            }

            foreach (var m in memos)
            {
                foreach (var c in m.ContentBoxes.Where(c => !c.Deleted))
                {
                    c.ContentToolsUploads
                        .Where(u => !u.Deleted && (IsMatch(u.Content) || IsMatch(u.Description) || IsMatch(u.Name)))
                        .ForEach(u => result.Add(getSearchViewResult(
                            $"{m.Title} - {(m.ContentBoxes.OrderBy(q => q.Number).First().Number == 0 ? c.Number + 1 : c.Number)}. {c.Title}",
                            $"{m.Id}#{c.Number}")));
                    c.Uploads
                        .Where(u => !u.Deleted && (IsMatch(u.Content) || IsMatch(u.Description) || IsMatch(u.Name)))
                        .ForEach(u => result.Add(getSearchViewResult(
                            $"{m.Title} - {(m.ContentBoxes.OrderBy(q => q.Number).First().Number == 0 ? c.Number + 1 : c.Number)}. {c.Title}",
                            $"{m.Id}#{c.Number}")));
                }
            }

            return result;
        }

        private List<GlobalSearchViewModel> searchPolicies()
        {
            _currentLocation = SearchLocations.Policies;
            var result = new List<GlobalSearchViewModel>();
            var policies = _policyRepository.List.Where(p => !p.Deleted);

            if (_assignedDocumentRepository != null)
            {
                var assignedIds = _assignedDocumentRepository.List
                    .Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.Policy && !a.Deleted)
                    .Select(a => a.DocumentId);
                policies = policies.Where(p => assignedIds.Contains(p.Id));
            }

            policies.Where(p => IsMatch(p.Title) || IsMatch(p.ReferenceId) || IsMatch(p.Description))
                .ForEach(p => result.Add(getSearchViewResult(p.Title, p.Id)));

            return result;
        }

        private List<GlobalSearchViewModel> searchPolicyContentBoxes()
        {
            _currentLocation = SearchLocations.PolicyContentBoxes;
            var result = new List<GlobalSearchViewModel>();
            var policies = _policyRepository.List.Where(p => !p.Deleted);

            if (_assignedDocumentRepository != null)
            {
                var assignedIds = _assignedDocumentRepository.List
                    .Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.Policy && !a.Deleted)
                    .Select(a => a.DocumentId);
                policies = policies.Where(p => assignedIds.Contains(p.Id));
            }

            foreach (var p in policies)
            {
                p.ContentBoxes.Where(c => !c.Deleted && (IsMatch(c.Title) || IsMatch(c.Content)))
                    .ForEach(c => result.Add(getSearchViewResult(
                        $"{p.Title} - {(p.ContentBoxes.OrderBy(q => q.Number).First().Number == 0 ? c.Number + 1 : c.Number)}. {c.Title}",
                        $"{p.Id}#{c.Number}")));
            }

            return result;
        }

        private List<GlobalSearchViewModel> searchPolicyContentBoxUploads()
        {
            _currentLocation = SearchLocations.PolicyContentBoxUploads;
            var result = new List<GlobalSearchViewModel>();
            var policies = _policyRepository.List.Where(p => !p.Deleted);

            if (_assignedDocumentRepository != null)
            {
                var assignedIds = _assignedDocumentRepository.List
                    .Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.Policy && !a.Deleted)
                    .Select(a => a.DocumentId);
                policies = policies.Where(p => assignedIds.Contains(p.Id));
            }

            foreach (var p in policies)
            {
                foreach (var c in p.ContentBoxes.Where(c => !c.Deleted))
                {
                    c.ContentToolsUploads
                        .Where(u => !u.Deleted && (IsMatch(u.Content) || IsMatch(u.Description) || IsMatch(u.Name)))
                        .ForEach(u => result.Add(getSearchViewResult(
                            $"{p.Title} - {(p.ContentBoxes.OrderBy(q => q.Number).First().Number == 0 ? c.Number + 1 : c.Number)}. {c.Title}",
                            $"{p.Id}#{c.Number}")));
                    c.Uploads
                        .Where(u => !u.Deleted && (IsMatch(u.Content) || IsMatch(u.Description) || IsMatch(u.Name)))
                        .ForEach(u => result.Add(getSearchViewResult(
                            $"{p.Title} - {(p.ContentBoxes.OrderBy(q => q.Number).First().Number == 0 ? c.Number + 1 : c.Number)}. {c.Title}",
                            $"{p.Id}#{c.Number}")));
                }
            }

            return result;
        }

        private List<GlobalSearchViewModel> searchTests()
        {
            _currentLocation = SearchLocations.Tests;
            var result = new List<GlobalSearchViewModel>();
            var tests = _testRepository.List.Where(t => !t.Deleted && t.DocumentStatus != DocumentStatus.Draft);

            if (_assignedDocumentRepository != null)
            {
                var assignedIds = _assignedDocumentRepository.List
                    .Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.Test && !a.Deleted)
                    .Select(a => a.DocumentId);
                tests = tests.Where(t => assignedIds.Contains(t.Id));
            }

            tests.Where(t => IsMatch(t.ReferenceId) || IsMatch(t.Title) || IsMatch(t.IntroductionContent))
                .ForEach(t => result.Add(getSearchViewResult(t.Title, t.Id)));

            return result;
        }

        private List<GlobalSearchViewModel> searchTestQuestions()
        {
            _currentLocation = SearchLocations.TestQuestions;
            var result = new List<GlobalSearchViewModel>();
            var tests = _testRepository.List.Where(t => !t.Deleted && t.DocumentStatus != DocumentStatus.Draft);

            if (_assignedDocumentRepository != null)
            {
                var assignedIds = _assignedDocumentRepository.List
                    .Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.Test && !a.Deleted)
                    .Select(a => a.DocumentId);
                tests = tests.Where(t => assignedIds.Contains(t.Id));
            }

            foreach (var t in tests)
            {
                t.Questions.Where(q => !q.Deleted && IsMatch(q.Question))
                    .ForEach(q => result.Add(getSearchViewResult(t.Title, t.Id)));
            }

            return result;
        }

        private List<GlobalSearchViewModel> searchTestAnswers()
        {
            _currentLocation = SearchLocations.TestAnswers;
            var result = new List<GlobalSearchViewModel>();
            var tests = _testRepository.List.Where(t => !t.Deleted && t.DocumentStatus != DocumentStatus.Draft);

            if (_assignedDocumentRepository != null)
            {
                var assignedIds = _assignedDocumentRepository.List
                    .Where(a => a.UserId == _filter.UserId.ToString() && a.DocumentType == DocumentType.Test && !a.Deleted)
                    .Select(a => a.DocumentId);
                tests = tests.Where(t => assignedIds.Contains(t.Id));
            }

            foreach (var t in tests)
            {
                foreach (var q in t.Questions.Where(q => !q.Deleted))
                {
                    q.Answers.Where(a => !a.Deleted && IsMatch(a.Option))
                        .ForEach(a => result.Add(getSearchViewResult(t.Title, t.Id)));
                }
            }


            return result;
        }

        private GlobalSearchViewModel getSearchViewResult(string Title, string Id)
        {
            var result = new GlobalSearchViewModel();
            result.SearchText = _filter.SearchText;
            result.SearchUrl = _locations[_currentLocation].SearchUrlBase + Id;
            result.FoundIn = _locations[_currentLocation].FoundInBase + " " + Title;
            return result;
        }

        private bool IsMatch(string validString)
        {
            if (!string.IsNullOrEmpty(_filter.SearchText) && !string.IsNullOrEmpty(validString))
                return validString.ToLowerInvariant().Contains(_filter.SearchText.ToLowerInvariant());
            return false;
        }
    }

    public struct searchResults
    {
        public string SearchUrlBase { get; set; }
        public string FoundInBase { get; set; }
    }

    public enum SearchLocations
    {
        TrainingManuals,
        TrainingManualChapters,
        TrainingManualChapterUploads,
		Checklists,
		ChecklistChapters,
		ChecklistChapterUploads,
		Memos,
        MemoContentBoxes,
        MemoContentBoxUploads,
        Policies,
        PolicyContentBoxes,
        PolicyContentBoxUploads,
        Tests,
        TestQuestions,
        TestAnswers
    }
}