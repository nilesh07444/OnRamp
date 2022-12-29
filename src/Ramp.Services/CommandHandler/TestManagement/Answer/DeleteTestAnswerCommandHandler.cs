using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement.Answer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement.Answer
{
    public class DeleteTestAnswerCommandHandler : ICommandHandlerBase<DeleteTestAnswerCommand>
    {
        private readonly IRepository<TestAnswer> _answerRepository;
        public DeleteTestAnswerCommandHandler(IRepository<TestAnswer> answerRepository)
        {
            _answerRepository = answerRepository;
        }
        public CommandResponse Execute(DeleteTestAnswerCommand command)
        {
            _answerRepository.Delete(_answerRepository.Find(command.Model.TestAnswerId));
            return null;
        }
    }
}
