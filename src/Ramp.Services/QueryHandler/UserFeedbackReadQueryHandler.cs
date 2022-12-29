using Common.Query;
using Ramp.Contracts.Query.UserFeedbackRead;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler
{
    public class UserFeedbackReadQueryHandler : IQueryHandler<UserFeedbackReadListQuery, IEnumerable<UserFeedbackReadistModel>>,
                                                IQueryHandler<FetchByIdQuery,UserFeedbackReadModel>
    {
        public IEnumerable<UserFeedbackReadistModel> ExecuteQuery(UserFeedbackReadListQuery query)
        {
            throw new NotImplementedException();
        }

        public UserFeedbackReadModel ExecuteQuery(FetchByIdQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
