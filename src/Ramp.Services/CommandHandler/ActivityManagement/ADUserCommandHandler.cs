using Common.Command;
using Common.Data;
using Common.Events;
using Domain.Customer.Models.Groups;
using Domain.Customer.Models;
using Domain.Enums;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.CommandParameter.Group;
using Ramp.Contracts.Events.CustomerManagement;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Customer.Models.Document;
using VirtuaCon;
using Role = Ramp.Contracts.Security.Role;
using Domain.Models;
using Domain.Customer.Models.CustomRole;
using Ramp.Contracts.ViewModel;
using Ramp.Contracts.CommandParameter.ActivityManagement;
using Domain.Customer;

namespace Ramp.Services.CommandHandler.ActivityManagement
{

    public class ADUserCommandHandler :CommandHandlerBase<List<ADUser>>
    {
        
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<Domain.Customer.Models.Groups.StandardUserGroup> _standardGroupRepository;
        private readonly ICommandDispatcher _commandDispatcher;



        public ADUserCommandHandler(
                        IRepository<StandardUser> userRepository, 
            ICommandDispatcher commandDispatcher,
            IRepository<Domain.Customer.Models.Groups.StandardUserGroup> standardGroupRepository
            
            )
        {
            _userRepository = userRepository;
            
            _standardGroupRepository = standardGroupRepository;
            _commandDispatcher = commandDispatcher;
            

        }

        public override CommandResponse Execute(List<ADUser> aDUsers)
        {
            //  var user =_userRepository.List.ToList();
            //  List<Emailist> emailLst = new List<Emailist>();
            //foreach (var u in user)
            //  {
            //      var userExits = aDUsers.Where(z => z.Email != u.EmailAddress).FirstOrDefault();
            //      if (userExits != null)
            //      {
            //          emailLst.Add(new Emailist(userExits.Email))
            //      }

            //  }

            //  var EmailNotExitsList = aDUsers.Join(user, B => B.Email, BT => BT.EmailAddress, (B, BT) => new { B = B, BT = BT })
            //      .Where(z=>z.B.Email!=z.BT.EmailAddress)
            //    .Select(z => new Emailist
            //    {
            //        emails = z.BT.EmailAddress
            //    }).ToList();

      
            try
            {
                foreach (var e in aDUsers)
                {
                    if (e.Email != null)
                    {
                        var ademail = e.Email.ToLower();
                        var userlst = _userRepository.List.Where(u => u.EmailAddress.ToLower() == ademail).FirstOrDefault();
                       
                        if (userlst != null)
                        {
                            userlst = new StandardUser
                            {
                                IsActive = true,
                            };
                            _userRepository.SaveChanges();
                        }
                        else
                        {
                            userlst = new StandardUser
                            {
                                IsActive = false,
                            };
                            _userRepository.SaveChanges();
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}