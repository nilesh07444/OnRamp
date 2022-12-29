using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.Group
{
    public class UpdateRoleDescriptionsCommandHandler : ICommandHandlerBase<UpdateRoleDescriptionsCommand>
    {
        private readonly IRepository<CustomerRole> _customerRoleRepository;

        public UpdateRoleDescriptionsCommandHandler(IRepository<CustomerRole> customerRoleRepository)
        {
            _customerRoleRepository = customerRoleRepository;
        }

        public CommandResponse Execute(UpdateRoleDescriptionsCommand command)
        {
            foreach (var customerRole in _customerRoleRepository.GetAll().ToList())
            {
                if (command.RoleDescriptionDictionary.ContainsKey(customerRole.RoleName))
                {
                    customerRole.Description = command.RoleDescriptionDictionary[customerRole.RoleName];
                }
            }
            _customerRoleRepository.SaveChanges();
            return null;
        }
    }
}