using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models.VirtualClassRooms;
using Ramp.Contracts.Query.VirtualClassroom;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ramp.Services.QueryHandler.VirtualClassRooms {
	public class ExternalMeetingUserQueryHandler:IQueryHandler<FetchExternalMeetingUserQuery, IEnumerable<ExternalMeetingUserModel>> {

		readonly ITransientReadRepository<ExternalMeetingUser> _repository;
		public ExternalMeetingUserQueryHandler(ITransientReadRepository<ExternalMeetingUser> repository) {
			_repository = repository;
		}

		public IEnumerable<ExternalMeetingUserModel> ExecuteQuery(FetchExternalMeetingUserQuery query) {
			
				var externalUsers = _repository.List.Where(c => c.MeetingId==query.MeetingId && c.UserId==query.UserId).AsQueryable().Select(Project.ExternalMeetingUser_ExternalMeetingUserModel);

				return externalUsers;
			

		}
	}
}
namespace Ramp.Services.Projection {
	public static partial class Project {
		
		public static readonly Expression<Func<ExternalMeetingUser, ExternalMeetingUserModel>> ExternalMeetingUser_ExternalMeetingUserModel =
		  x => new ExternalMeetingUserModel {
			  Id = x.Id,
			  UserId=x.UserId,
			  EmailAddress=x.EmailAddress,
			  CreatedOn=x.CreatedOn,
			  MeetingId=x.MeetingId
		  };


	}
}
