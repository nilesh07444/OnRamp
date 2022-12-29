using Common.Command;
using Data.EF.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.UI.Code.Handlers
{
    public class ConnectionStringCommandHandler : ICommandHandlerBase<UpdateConnectionStringCommand>
    {
        public CommandResponse Execute(UpdateConnectionStringCommand command)
        {
            ConnectionStringResolver._companyId = command.CompanyId;
            return null;
        }
    }
}