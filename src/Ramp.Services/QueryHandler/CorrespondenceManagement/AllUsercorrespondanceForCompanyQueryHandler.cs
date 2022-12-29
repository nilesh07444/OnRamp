using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using org.openxmlformats.schemas.drawingml.x2006.main;
using Ramp.Contracts.QueryParameter.CorrespondenceManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.CorrespondenceManagement
{
    public class AllUsercorrespondanceForCompanyQueryHandler : IQueryHandler<AllUserCorrespondanceForCompanyQuery, List<StandardUserCorrespondanceLog>>
    {
        private readonly IRepository<StandardUserCorrespondanceLog> _standardUserCorrespondanceLogRepository;

        public AllUsercorrespondanceForCompanyQueryHandler(IRepository<StandardUserCorrespondanceLog> standardUserCorrespondanceLogRepository)
        {
            _standardUserCorrespondanceLogRepository = standardUserCorrespondanceLogRepository;
        }

        public List<StandardUserCorrespondanceLog> ExecuteQuery(AllUserCorrespondanceForCompanyQuery query)
        {
            var result = new List<StandardUserCorrespondanceLog>();

            if (_standardUserCorrespondanceLogRepository.List.Any())
                result.AddRange(_standardUserCorrespondanceLogRepository.List);

            return result;
        }
    }
}