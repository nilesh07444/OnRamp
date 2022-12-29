using Common.Command;
using Common.Data;
using Ramp.Contracts.CommandParameter;
using System.Linq;
using Domain.Customer.Models.CustomRole;
using System;
using Domain.Customer.Models;

namespace Ramp.Services.CommandHandler {
	public class DeleteCustomUserRoleCommandHandler :
		CommandHandlerBase<DeleteCustomUserRoleCommand> {

		private readonly IRepository<Domain.Customer.Models.CustomRole.CustomUserRoles> _customUserRole;
		private readonly IRepository<StandardUser> _user;

		public DeleteCustomUserRoleCommandHandler(
			IRepository<StandardUser> user,
			IRepository<Domain.Customer.Models.CustomRole.CustomUserRoles> customUserRole)
		{
			_customUserRole = customUserRole;
			_user = user;
		}

		public override CommandResponse Execute(DeleteCustomUserRoleCommand command)
		{
			var response = new CommandResponse();
			//response.ErrorMessage = null;
			//response.Id = command.Id.ToString();
			//response.Validation = null;

			var entity = _customUserRole.List.Where(x => x.Id == command.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
			var user = _user.List.Where(x => x.CustomUserRoleId != null && x.CustomUserRoleId == command.Id).FirstOrDefault();

			try
			{
				if (user == null)
				{
					if (entity != null)
					{
						_customUserRole.Delete(entity);
						_customUserRole.SaveChanges();

						response.Id = command.Id.ToString();
						response.ErrorMessage = null;
						response.Validation = null;

					}
					else
					{
						response.Id = command.Id.ToString();
						response.ErrorMessage = "No record exist";
						response.Validation = null;
					}
				}
				else
				{
					response.Id = command.Id.ToString();
					response.ErrorMessage = "Can not delete role, users associated to it";
					response.Validation = null;

				}
			}
			catch (Exception ex)
			{
				response.ErrorMessage = ex.Message;
			}

			return response;
		}
	}
}
