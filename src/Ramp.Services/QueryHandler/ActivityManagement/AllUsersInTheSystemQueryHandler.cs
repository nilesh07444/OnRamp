using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Common.Query;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler.ActivityManagement
{
    public class AllUsersInTheSystemQueryHandler :
        QueryHandlerBase<EmptyQueryParameter, List<UserModelShort>>
    {
        private readonly IRepository<Domain.Models.User> _userRepository;

        public AllUsersInTheSystemQueryHandler(IRepository<Domain.Models.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public override List<UserModelShort> ExecuteQuery(EmptyQueryParameter queryParameters)
        {
            var userlist = new List<UserModelShort>();
            List<Domain.Models.User> users = _userRepository.List.Where(u => u.LastName != "Admin").ToList();
            foreach (Domain.Models.User user in users)
            {
                var userModelShort = new UserModelShort
                {
                    Id = user.Id,
                    Name = user.FirstName
                };

                userlist.Add(userModelShort);
            }
            return userlist;
        }
    }
}

