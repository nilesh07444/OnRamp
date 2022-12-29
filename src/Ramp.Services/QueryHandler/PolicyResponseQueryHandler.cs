using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Customer.Models.PolicyResponse;
using Ramp.Contracts.Query.PolicyResponse;

namespace Ramp.Services.QueryHandler
{
    public class PolicyResponseQueryHandler : IQueryHandler<PolicyResponseQuery, bool?>
    {
        private readonly IRepository<PolicyResponse> _policyResponseRepository;

        public PolicyResponseQueryHandler(
            IRepository<PolicyResponse> policyResponseRepository)
        {
            _policyResponseRepository = policyResponseRepository;
        }

        public bool? ExecuteQuery(PolicyResponseQuery query)
        {
            var response =
                _policyResponseRepository.List.OrderByDescending(r => r.Created).FirstOrDefault(r => r.UserId == query.UserId && r.IsGlobalAccessed==query.IsGlobalAccess && r.PolicyId == query.PolicyId && r.Response != null && r.Response!=false);
            return response?.Response;
        }
    }
}