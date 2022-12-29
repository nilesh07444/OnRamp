using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Ramp.Contracts.Query.Document;
using System;
using System.Linq;
using Common.Command;
using Data.EF;
using Domain.Customer.Models.CheckLists;

namespace Ramp.Services.QueryHandler
{
    public class DocumentCountQueryHandler : IQueryHandler<CompanyDocumentCountQuery, int>,
                                             IQueryHandler<DocumentsRemainingQuery, int>
    {
        private readonly ITransientRepository<Memo> _memoRepository;
        private readonly ITransientRepository<Policy> _policyRepository;
        private readonly ITransientRepository<Test> _testRepository;
        private readonly ITransientRepository<TrainingManual> _trainingManualRepository;
        private readonly ITransientRepository<CheckList> _checkListRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        private readonly MainContext _mainContext = new MainContext();

        public DocumentCountQueryHandler(
            ITransientRepository<Memo> memoRepository,
            ITransientRepository<Policy> policyRepository,
            ITransientRepository<Test> testRepository,
            ITransientRepository<TrainingManual> trainingManualRepository,
            ITransientRepository<CheckList> checkListRepository,
            ICommandDispatcher commandDispatcher)
        {
            _memoRepository = memoRepository;
            _policyRepository = policyRepository;
            _testRepository = testRepository;
            _trainingManualRepository = trainingManualRepository;
            _commandDispatcher = commandDispatcher;
			_checkListRepository = checkListRepository;
        }

        public int ExecuteQuery(CompanyDocumentCountQuery query)
        {
            _memoRepository.SetCustomerCompany(query.CompanyId.ToString());
            _policyRepository.SetCustomerCompany(query.CompanyId.ToString());
            _testRepository.SetCustomerCompany(query.CompanyId.ToString());
            _trainingManualRepository.SetCustomerCompany(query.CompanyId.ToString());
			_checkListRepository.SetCustomerCompany(query.CompanyId.ToString());

            var total = _memoRepository.List.Count(x => x.Deleted == false) +
                        _policyRepository.List.Count(x => x.Deleted == false) +
                        _testRepository.List.Count(x => x.Deleted == false) +
                        _trainingManualRepository.List.Count(x => x.Deleted == false)+
						_checkListRepository.List.Count(x => x.Deleted == false);

            _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

            return total;
        }

        public int ExecuteQuery(DocumentsRemainingQuery query)
        {
            var company = _mainContext.Company.AsQueryable()
                .First(x => x.Id == query.CompanyId);

			if (company.Bundle != null)
				return company.Bundle.MaxNumberOfDocuments - ExecuteQuery(new CompanyDocumentCountQuery { CompanyId = query.CompanyId });
			else return 700;
        }
    }
}