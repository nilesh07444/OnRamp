using Common;
using Common.Data;
using Common.Query;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler
{
    public class CommandAuditTrailQueryHandler : IQueryHandler<FetchByIdQuery<CommandAuditTrail>, CommandAuditTrail>
    {
        private readonly IRepository<CommandAuditTrail> _repository;
        public CommandAuditTrailQueryHandler(IRepository<CommandAuditTrail> repository)
        {
            _repository = repository;
        }
        public CommandAuditTrail ExecuteQuery(FetchByIdQuery<CommandAuditTrail> query)
        {
            return _repository.Find(query.Id.ConvertToGuid());
        }
    }
}
