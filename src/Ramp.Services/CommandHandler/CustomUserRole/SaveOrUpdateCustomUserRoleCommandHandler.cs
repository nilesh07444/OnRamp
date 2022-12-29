using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Customer.Models.CustomRole;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.CommandParameter.CustomUserRole;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.Security;

namespace Ramp.Services.CommandHandler {
	class SaveOrUpdateCustomUserRoleCommandHandler : CommandHandlerBase<SaveOrUpdateCustomUserRoleCommand> {

		private readonly IRepository<CustomUserRoles> _customUserRole;
		private readonly IRepository<StandardUser> _user;
		private readonly IRepository<CustomerRole> _roleRepository;

		public SaveOrUpdateCustomUserRoleCommandHandler(
			IRepository<CustomUserRoles> customUserRole,
			IRepository<StandardUser> user,
			IRepository<CustomerRole> roleRepository)
		{
			_customUserRole = customUserRole;
			_user = user;
			_roleRepository = roleRepository;
		}

		public override CommandResponse Execute(SaveOrUpdateCustomUserRoleCommand command)
		{
			var response = new CommandResponse();
			response.ErrorMessage = null;
			response.Id = command.Id.ToString();
			response.Validation = null;

			var data = new CustomUserRoles();
			try
			{
				if (command.Id == Guid.Empty)
				{
					//code to create role

					data.Id = Guid.NewGuid();

					data.UserId = command.UserId;
					data.Title = command.Title;

					data.StandardUser = command.StandardUser;
					data.ContentAdmin = command.ContentAdmin;
					data.ContentApprover = command.ContentApprover;
					data.ContentCreator = command.ContentCreator;
					data.ManageTags = command.ManageTags;
					data.ManageVirtualMeetings = command.ManageVirtualMeetings;
					data.PortalAdmin = command.PortalAdmin;
					data.Publisher = command.Publisher;
					data.Reporter = command.Reporter;
					data.UserAdmin = command.UserAdmin;
					data.ManageAutoWorkflow = command.ManageAutoWorkflow;
					data.ManageReportSchedule = command.ManageReportSchedule;

					data.IsActive = true;
					data.IsDeleted = false;
					data.DateCreated = DateTime.Now;

					_customUserRole.Add(data);
					_customUserRole.SaveChanges();

					response.Id = data.Id.ToString();
				}
				else if (command.Id != Guid.Empty)
				{
					var entity = _customUserRole.Find(command.Id);

					entity.Title = command.Title;
					entity.StandardUser = command.StandardUser;
					entity.ContentAdmin = command.ContentAdmin;
					entity.ContentApprover = command.ContentApprover;
					entity.ContentCreator = command.ContentCreator;
					entity.ManageTags = command.ManageTags;
					entity.ManageVirtualMeetings = command.ManageVirtualMeetings;
					entity.PortalAdmin = command.PortalAdmin;
					entity.Publisher = command.Publisher;
					entity.Reporter = command.Reporter;
					entity.UserAdmin = command.UserAdmin;
					entity.ManageAutoWorkflow = command.ManageAutoWorkflow;
					entity.ManageReportSchedule = command.ManageReportSchedule;
					entity.DateEdited = DateTime.Now;

					_customUserRole.SaveChanges();

					UpdateAllUsersPermissions(command.Id, entity);
				}
			}
			catch (Exception ex)
			{
				response.ErrorMessage = ex.Message;
			}

			return response;
		}

		private void UpdateAllUsersPermissions(Guid roleId, CustomUserRoles customRole)
		{
			//get all users having this custom role Id
			var users = _user.List.Where(x => x.CustomUserRoleId == roleId).ToList();

			var roles = GetRolesFromViewModel(customRole);

			//update role of each user
			foreach (var user in users)
			{
				user.Roles.Clear();
				foreach (var roleName in roles)
				{
					var roleEntity = _roleRepository.List.FirstOrDefault(r => r.RoleName.Equals(roleName));

					if (roleEntity != null)
					{

						if (!user.Roles.Any(r => r.RoleName.Equals(roleEntity.RoleName)))
							user.Roles.Add(roleEntity);
					}
				}

				_user.SaveChanges();
			}

		}

		private List<string> GetRolesFromViewModel(CustomUserRoles customRole)
		{
			var result = new List<string>();

			if (customRole.StandardUser)
			{
				result.Add(Role.StandardUser);
			}
			if (customRole.ContentAdmin)
			{
				result.Add(Role.ContentAdmin);
			}
			if (customRole.ContentApprover)
			{
				result.Add(Role.ContentApprover);
			}
			if (customRole.ContentCreator)
			{
				result.Add(Role.ContentCreator);
			}
			if (customRole.ManageTags)
			{
				result.Add(Role.ManageTags);
			}
			if (customRole.ManageVirtualMeetings)
			{
				result.Add(Role.ManageVirtualMeetings);
			}
			if (customRole.PortalAdmin)
			{
				result.Add(Role.PortalAdmin);
			}
			if (customRole.Publisher)
			{
				result.Add(Role.Publisher);
			}
			if (customRole.Reporter)
			{
				result.Add(Role.Reporter);
			}
			if (customRole.UserAdmin)
			{
				result.Add(Role.UserAdmin);
			}
			if (customRole.ManageAutoWorkflow)
			{
				result.Add(Role.ManageAutoWorkflow);
			}
			if (customRole.ManageReportSchedule)
			{
				result.Add(Role.ManageReportSchedule);
			}

			return result;
		}
	}
}
