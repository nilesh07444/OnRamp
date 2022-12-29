using System;
using System.Linq;
using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models.Test;
using Ramp.Contracts.Command.TestSession;
using Ramp.Contracts.Events.Account;

namespace Ramp.Services.CommandHandler
{
    public class TestSessionCommandHandler : ICommandHandlerBase<TestSessionEndCommand>,
                                             ICommandHandlerBase<TestSessionStartCommand>,
        IEventHandler<StandardUserDeletedEvent>
    {
        private readonly IRepository<TestSession> _testSessionRepository;

        public TestSessionCommandHandler(IRepository<TestSession> testSessionRepository)
        {
            _testSessionRepository = testSessionRepository;
        }

        public CommandResponse Execute(TestSessionEndCommand command)
        {
            var testSession = _testSessionRepository.List.AsQueryable().FirstOrDefault(x => x.UserId == command.UserId);

            if (testSession != null)
            {
                testSession.CurrentTestId = null;
                testSession.StartTime = null;
				testSession.IsGlobalAccessed = command.IsGlobalAccessed;
				_testSessionRepository.SaveChanges();
            }

            return null;
        }

        public CommandResponse Execute(TestSessionStartCommand command)
        {
            var testSession = _testSessionRepository.List.FirstOrDefault(x => x.UserId == command.UserId);

            if (testSession == null)
            {
                testSession = new TestSession
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = command.UserId,
					IsGlobalAccessed = command.IsGlobalAccessed
				};

                _testSessionRepository.Add(testSession);
            }

            testSession.CurrentTestId = command.CurrentTestId;
            testSession.StartTime = DateTime.UtcNow;

            _testSessionRepository.SaveChanges();

            return null;
        }

        public void Handle(StandardUserDeletedEvent @event)
        {
            if (!string.IsNullOrWhiteSpace(@event.Id))
            {
                _testSessionRepository.List.AsQueryable().Where(x => x.UserId == @event.Id).ToList().ForEach(x => _testSessionRepository.Delete(x));
                _testSessionRepository.SaveChanges();
            }
        }
    }
}