using System;
using System.Linq;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models.Test;
using Ramp.Contracts.Query.TestSession;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler
{
    public class TestSessionQueryHandler : IQueryHandler<TestSessionQuery, TestSessionViewModel>
    {
        private readonly IReadRepository<TestSession> _testSessionRepository;
        private readonly IReadRepository<Test> _testRepository;

        public TestSessionQueryHandler(
            IReadRepository<TestSession> testSessionRepository,
            IReadRepository<Test> testRepository)
        {
            _testSessionRepository = testSessionRepository;
            _testRepository = testRepository;
        }

        public TestSessionViewModel ExecuteQuery(TestSessionQuery query)
        {
            var testSession = _testSessionRepository.List.FirstOrDefault(x => x.UserId == query.UserId);
            if (testSession == null) return null;

            var test = _testRepository.List.AsQueryable().FirstOrDefault(x => x.Id == testSession.CurrentTestId);
            if (test == null) return null;

            var timeLeft = test.EnableTimer ?  testSession.StartTime?.AddMinutes(test.Duration).Subtract(DateTime.UtcNow) ?? TimeSpan.Zero : TimeSpan.Zero;

            var result = new TestSessionViewModel
            {
                TimeLeft = timeLeft > TimeSpan.Zero ? timeLeft : TimeSpan.Zero,
                OpenTest = test.OpenTest,
                DocumentStatus = test.DocumentStatus,
                CurrentTestId = test.Id,
                EnableTimer = test.EnableTimer,
				Title=test.Title,
				IsGloballyAccessed = test.IsGlobalAccessed
			};

            return result;
        }
    }
}