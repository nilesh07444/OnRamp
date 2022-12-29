using Common.Command;
using Ramp.Contracts.Command.UserFeedbackRead;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler
{
    public class UserFeedbackReadCommandHandler : ICommandHandlerAndValidator<CreateOrUpdateUserFeedbackReadCommand>
    {
        public CommandResponse Execute(CreateOrUpdateUserFeedbackReadCommand command)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IValidationResult> Validate(CreateOrUpdateUserFeedbackReadCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
