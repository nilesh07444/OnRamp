using Common.Command;
using Common.Events;
using Ramp.Contracts.CommandParameter.Portal;
using Ramp.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.UI.Code.Handlers
{
    public class OverridePortalContextCommandHandler : ICommandHandlerBase<OverridePortalContextCommand>
    {
        public CommandResponse Execute(OverridePortalContextCommand command)
        {
            PortalContext.Override(command.CompanyId);
            return null;
        }
    }
}