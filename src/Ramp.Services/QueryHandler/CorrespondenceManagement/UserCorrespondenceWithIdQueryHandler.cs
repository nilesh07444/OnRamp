using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.CorrespondenceManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Projection;
using System.Linq;

namespace Ramp.Services.QueryHandler.CorrespondenceManagement
{
    public class UserCorrespondenceWithIdQueryHandler :
        QueryHandlerBase<UserCorrespondenceWithIdQueryParameter, UserCorrespondenceLogViewModel>
    {
        private readonly IRepository<UserCorrespondenceLog> _userCorrespondenceLogRepository;
        private readonly IRepository<StandardUserCorrespondanceLog> _standardUserCorrespondenceLogRepository;

        public UserCorrespondenceWithIdQueryHandler(IRepository<UserCorrespondenceLog> userCorrespondenceLogRepository,
            IRepository<StandardUserCorrespondanceLog> standardUserCorrespondenceLogRepository)
        {
            _userCorrespondenceLogRepository = userCorrespondenceLogRepository;
            _standardUserCorrespondenceLogRepository = standardUserCorrespondenceLogRepository;
        }

        public override UserCorrespondenceLogViewModel ExecuteQuery(UserCorrespondenceWithIdQueryParameter queryParameters)
        {
            var userCorrespondence = _userCorrespondenceLogRepository.Find(queryParameters.Id);
            var standardUserCorrespondance = _standardUserCorrespondenceLogRepository.Find(queryParameters.Id);
            if (userCorrespondence != null)
                return Project.UserCorrespondenceLogViewModelFrom(userCorrespondence);
            if (standardUserCorrespondance != null)
                return Project.UserCorrespondenceLogViewModelFrom(standardUserCorrespondance);
            return null;
        }
    }
}