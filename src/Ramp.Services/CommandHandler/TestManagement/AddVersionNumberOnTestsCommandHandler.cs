using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class AddVersionNumberOnTestsCommandHandler : ICommandHandlerBase<AddVersionNumberOnTestsCommandParameter>
    {
        private readonly IRepository<TrainingTest> _testRepository;

        public AddVersionNumberOnTestsCommandHandler(IRepository<TrainingTest> testRepository)
        {
            _testRepository = testRepository;
        }

        public CommandResponse Execute(AddVersionNumberOnTestsCommandParameter command)
        {
            foreach (var test in _testRepository.List.Where(t => t.ActiveStatus).ToList())
            {
                test.Version = 1;
            }
            _testRepository.SaveChanges();
            return null;
        }
    }
}