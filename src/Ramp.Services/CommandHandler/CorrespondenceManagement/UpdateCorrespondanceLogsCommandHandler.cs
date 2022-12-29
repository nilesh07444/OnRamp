using com.sun.org.apache.bcel.@internal.generic;
using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CorrespondenceManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.CorrespondenceManagement
{
    public class UpdateCorrespondanceLogsCommandHandler : ICommandHandlerBase<UpdateCorrespondanceLogsCommand>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<Domain.Models.User> _userRepository;
        private readonly IRepository<UserCorrespondenceLog> _userCorrespondanceLogRepository;
        private readonly IRepository<StandardUserCorrespondanceLog> _standardUserCorrespondanceLogRepository;

        public UpdateCorrespondanceLogsCommandHandler(IRepository<StandardUser> standardUserRepository,
            IRepository<Domain.Models.User> userRepository,
            IRepository<UserCorrespondenceLog> userCorrespondanceLogRepository,
            IRepository<StandardUserCorrespondanceLog> standardUserCorrespondanceLogRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
            _userCorrespondanceLogRepository = userCorrespondanceLogRepository;
            _standardUserCorrespondanceLogRepository = standardUserCorrespondanceLogRepository;
        }

        public CommandResponse Execute(UpdateCorrespondanceLogsCommand command)
        {
            var user = _userRepository.Find(command.PreviousUserId);
            if (user != null)
            {
                var userCorrespondance =
                    _userCorrespondanceLogRepository.List.Where(entry => entry.UserId.Equals(user.Id)).ToList();
                if (userCorrespondance.Count > 0)
                {
                    var standardUser =
                        _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(user.EmailAddress));
                    if (standardUser != null)
                    {
                        foreach (var entry in userCorrespondance)
                        {
                            var newLogEntry = new StandardUserCorrespondanceLog
                            {
                                Id = Guid.NewGuid(),
                                User = standardUser,
                                Description = entry.Description,
                                CorrespondenceDate = entry.CorrespondenceDate,
                                UserId = standardUser.Id,
                                Content = entry.Content
                            };
                            if (entry.CorrespondenceType == UserCorrespondenceEnum.Email)
                                newLogEntry.CorrespondenceType = StandardUserCorrespondenceEnum.Email;
                            if (entry.CorrespondenceType == UserCorrespondenceEnum.Sms)
                                newLogEntry.CorrespondenceType = StandardUserCorrespondenceEnum.Sms;

                            _standardUserCorrespondanceLogRepository.Add(newLogEntry);
                        }
                        _standardUserCorrespondanceLogRepository.SaveChanges();
                    }
                }
            }

            return null;
        }
    }
}