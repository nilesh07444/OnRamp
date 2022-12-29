using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts.Query.Course;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.Security;

namespace Ramp.Services.QueryHandler {
	public class CourseAssignmentQueryHandler {

		private readonly ITransientRepository<StandardUser> _userRepository;
		private readonly ITransientRepository<Course> _courseRepository;
		private readonly ITransientRepository<AssignedCourse> _assignedCourseRepository;
		private readonly ITransientReadRepository<AssociatedDocument> _associatedDocumentRepository;

		public CourseAssignmentQueryHandler(
			ITransientRepository<StandardUser> userRepository,
			ITransientRepository<Course> courseRepository,
			ITransientRepository<AssignedCourse> assignedCourseRepository,
			ITransientReadRepository<AssociatedDocument> _associatedDocumentRepository
			)
		{
			userRepository = _userRepository;
			courseRepository = _courseRepository;
			assignedCourseRepository = _assignedCourseRepository;
		}


		public IEnumerable<UserModelShort> ExecuteQuery(UsersAssignedCourseQuery query)
		{
			//_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

			var userExist = _assignedCourseRepository.List.Where(x => !x.Deleted && query.AllDocumentId.Contains(x.CourseId.ToString())).ToList();

			var z = new List<AssignedCourse>();
			foreach (var usr in userExist)
			{
				var user = userExist.Where(x => x.UserId == usr.UserId).ToList();
				if (user.Count == query.AllDocumentId.Count())
				{
					z.Add(usr);
				}
			}

			var userIds = z.Select(x => x.UserId).Distinct().ToList();

			var users = _userRepository.List
				.Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)) && u.IsActive && !u.IsUserExpire)
				.Select(u => new UserModelShort
				{
					Id = u.Id,
					Name = u.FirstName + " " + u.LastName,
				}).OrderBy(u => u.Name).ToList();

		

			return users;
		}

		public IEnumerable<UserModelShort> ExecuteQuery(UsersNotAssignedCourseQuery query)
		{
			//_commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

			var users = _userRepository.List
				.Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser)) && u.IsActive && !u.IsUserExpire)
				.Select(u => new UserModelShort
				{
					Id = u.Id,
					Name = u.FirstName + " " + u.LastName,
					//GroupId = u.Group.Id
				}).OrderBy(u => u.Name).ToList();

			return users;
		}

	}
}
