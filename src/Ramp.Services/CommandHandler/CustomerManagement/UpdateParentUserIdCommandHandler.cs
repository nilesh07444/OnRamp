using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class UpdateParentUserIdCommandHandler : ICommandHandlerBase<UpdateParentUserIdCommand>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;

        public UpdateParentUserIdCommandHandler(IRepository<StandardUser> standardUserRepository,
            IRepository<Domain.Models.User> userRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
        }

        public CommandResponse Execute(UpdateParentUserIdCommand command)
        {
            var user = _userRepository.Find(command.PreviousUserId);
            if (user != null)
            {
                var parentUser = _userRepository.Find(user.ParentUserId);
                if (parentUser != null)
                {
                    var newUser =
                        _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(user.EmailAddress));
                    var newParentUser = _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(parentUser.EmailAddress));
                    if (newUser != null && newParentUser != null)
                    {
                        newUser.ParentUserId = newParentUser.Id;
                    }
                }
            }
            _standardUserRepository.SaveChanges();
            return null;
        }
    }
}