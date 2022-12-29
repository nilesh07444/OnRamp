using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Domain.Customer.Models.CheckLists;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class GlobalSearchQueryHandler : QueryHandlerBase<GlobalSearchQueryParameter, List<GlobalSearchViewModel>>
    {
        private IRepository<TrainingGuide> _adminGuideRepository;
        private IRepository<AssignedTrainingGuides> _standardGuideRepository;
        private IRepository<TrainingTest> _adminTestRepository;
        private IRepository<TestAssigned> _standardTestRepository;
        private IRepository<TrainingManual> _trainingManualRepository;
        private IRepository<Memo> _memoRepository;
        private IRepository<Policy> _policyRepository;
		private IRepository<CheckList> _checklistRepository;
		private IRepository<Test> _testRepository;
        private IRepository<AssignedDocument> _assignedDocumentRepository;
        private GlobalSearchQueryParameter _filter;

        public GlobalSearchQueryHandler(
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

        public override List<GlobalSearchViewModel> ExecuteQuery(GlobalSearchQueryParameter queryParameters)
        {
            List<GlobalSearchViewModel> GlobalSearchList = new List<GlobalSearchViewModel>();
            _filter = queryParameters;
            setUpScope();

            var oSeachHelper = new SearchHelper(queryParameters, _adminGuideRepository, _standardGuideRepository, _adminTestRepository, _standardTestRepository, _trainingManualRepository, _memoRepository, _policyRepository,_checklistRepository, _testRepository, _assignedDocumentRepository);
            GlobalSearchList.AddRange(oSeachHelper.Search());
            return GlobalSearchList;
        }

        private void setUpScope()
        {
            try
            {
                if (!_filter.IsCustAdmin)
                {
                    _adminGuideRepository = null;
                    _adminTestRepository = null;
                }
                else
                {
                    _standardGuideRepository = null;
                    _standardTestRepository = null;
                    _assignedDocumentRepository = null;
                }
            }
            catch (InvalidCastException) { }
        }
    }
}