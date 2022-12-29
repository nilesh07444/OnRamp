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
    public class UpdateTestAnswerCommandHandler : ICommandHandlerBase<UpdateTestAnswerCommand>
    {
        private readonly IRepository<TestAnswer> _answerRepository;
        public UpdateTestAnswerCommandHandler(IRepository<TestAnswer> answerRepository)
        {
            _answerRepository = answerRepository;
        }
        public CommandResponse Execute(UpdateTestAnswerCommand command)
        {
            var aDomain = _answerRepository.Find(command.Model.TestAnswerId);
            var aEdit = command.Model;

            aDomain.Correct = aEdit.Correct;
            aDomain.Option = aEdit.Option;
            aDomain.Position = aEdit.Position;

            _answerRepository.SaveChanges();
            return null;
        }
    }
}
