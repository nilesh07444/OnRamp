using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CustomRole;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.Courses {
	public class GetAllAdministrators : QueryHandlerBase<EmptyQueryParameter, IEnumerable<UserModelShort>>,
		IQueryHandler<FetchByIdQuery, IEnumerable<UserModelShort>> {

		private readonly ITransientRepository<StandardUser> _userRepository;
		//private readonly ITransientRepository<CustomerRoles> _userRepository;
		private readonly ITransientRepository<CustomUserRoles> _customUserRole;
		private readonly IRepository<AssignedCourse> _assignedCourseRepository;

		public GetAllAdministrators(ITransientRepository<StandardUser> userRepository,IRepository<AssignedCourse> assignedCourseRepository)
		{
			_userRepository = userRepository;
			_assignedCourseRepository = assignedCourseRepository;
		}
		
		public override IEnumerable<UserModelShort> ExecuteQuery(EmptyQueryParameter queryParameters)
		{
			var t = _userRepository.List.Where(r => r.Roles.Any(m => m.RoleName != "StandardUser" && r.Roles.Count <= 1)).Select(x => new UserModelShort
			{
				Id = x.Id,
				Name = x.FirstName + " " + x.LastName,
				Email = x.EmailAddress,
				CustomUserRoleId = x.CustomUserRoleId

			}).ToList();
			return _userRepository.List.Where(r=> r.Roles.Any(m=>m.RoleName != "StandardUser" && r.Roles.Count <= 1)).Select(x => new UserModelShort {
				Id = x.Id,
				Name = x.FirstName + " " + x.LastName,
				Email = x.EmailAddress,
				CustomUserRoleId = x.CustomUserRoleId
			
			}).AsEnumerable();
			
		}

		public IEnumerable<UserModelShort> ExecuteQuery(FetchByIdQuery query)
		{
			try
			{
				var g = _assignedCourseRepository.List.ToList();
				var users = _assignedCourseRepository.List.Where(x => x.CourseId.ToString() == query.Id.ToString()).Select(r => new UserModelShort
				{
					Id = r.UserId,
					Name = _userRepository.Find(r.UserId).FirstName + " " + _userRepository.Find(r.UserId).LastName,
					Email = _userRepository.Find(r.UserId).EmailAddress
				}).ToList();

				return users;
			}
			catch (Exception ex)
			{

				throw ex;
			}
			
		}
	}
}
