using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Domain.Customer.Models.VirtualClassRooms;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.Calendar {
	public class CalendarQueryHandler : QueryHandlerBase<FetchAllQuery, CalendarData> {

		private readonly IRepository<AssignedDocument> _docs;
		private readonly IRepository<StandardUser> _users;

		private readonly IRepository<TrainingManual> _tm;
		private readonly IRepository<Test> _test;
		private readonly IRepository<Memo> _memo;
		private readonly IRepository<CheckList> _checklist;
		private readonly IRepository<Policy> _policy;
		private readonly IRepository<VirtualClassRoom> _virtualClassRoom;


		public CalendarQueryHandler(
			IRepository<AssignedDocument> docs,
			IRepository<StandardUser> users,
			IRepository<TrainingManual> tm,
			IRepository<Test> test,
			IRepository<Memo> memo,
			IRepository<CheckList> checklist,
			IRepository<Policy> policy,
			IRepository<VirtualClassRoom> virtualClassRoom
			)
		{
			_docs = docs;
			_users = users;

			_tm = tm;
			_test = test;
			_memo = memo;
			_checklist = checklist;
			_policy = policy;
			_virtualClassRoom = virtualClassRoom;
		}

		public override CalendarData ExecuteQuery(FetchAllQuery query)
		{


			var userList = _users.List.Where(x => x.IsActive).ToList();
			var docs = new List<CalendarViewModel>();

			if (query.Filters == "su")
			{
				docs = _docs.List.Where(x => x.UserId.ToString() == query.Id.ToString())
				.Select(y => new CalendarViewModel
				{
					assignedById = y.AssignedBy,
					createdById = y.UserId,
					type = (int)y.DocumentType,
					id = y.DocumentId,
					start = y.AssignedDate.ToString("yyyy-MM-dd"),
					additionalMessage = y.AdditionalMsg,
					assignedDate = y.AssignedDate
				}).ToList();
			}
			else if (query.Filters == "admin")
			{
				docs = _docs.List.Where(x => x.Deleted == false && x.AssignedBy.ToString() == query.Id.ToString())
		   .Select(y => new CalendarViewModel
		   {
			   assignedById = y.AssignedBy,
			   createdById = y.UserId,
			   type = (int)y.DocumentType,
			   id = y.DocumentId,
			   start = y.AssignedDate.ToString("yyyy-MM-dd"),
			   additionalMessage = y.AdditionalMsg,
			   assignedDate = y.AssignedDate
		   }).OrderByDescending(v=>v.assignedDate).ToList();
			}

			foreach (var user in userList)
			{
				foreach (var doc in docs)
				{
					if (doc.assignedById == user.Id.ToString())
					{
						doc.assignedBy = user.FirstName + " " + user.LastName;
					}
					else if (doc.createdById == user.Id.ToString())
					{
						doc.userName = user.FirstName + " " + user.LastName;
					}
				}
			}

			foreach (var doc in docs)
			{

				if (doc.type == 1)
				{
					//TrainingManual
					var title = _tm.List.Where(x => x.Id.ToString() == doc.id).FirstOrDefault();
					doc.title = title.Title;
					doc.className = "training-manual-icon";
				}
				else if (doc.type == 2)
				{
					//Test
					var title = _test.List.Where(x => x.Id.ToString() == doc.id).FirstOrDefault();
					doc.title = title.Title;
					doc.className = "test-icon";
				}
				else if (doc.type == 3)
				{
					//Policy
					var title = _policy.List.Where(x => x.Id.ToString() == doc.id).FirstOrDefault();
					doc.title = title.Title;
					doc.className = "policy-icon";
				}
				else if (doc.type == 4)
				{
					//Memo
					var title = _memo.List.Where(x => x.Id.ToString() == doc.id).FirstOrDefault();
					doc.title = title.Title;
					doc.className = "memo-icon";
				}
				else if (doc.type == 5)
				{
					
				}
				else if (doc.type == 6)
				{
					//Checklist
					var title = _checklist.List.Where(x => x.Id.ToString() == doc.id).FirstOrDefault();
					doc.title = title.Title;
					doc.className = "checklist-icon";
				}
				else if(doc.type == 7)
				{
					var title = _virtualClassRoom.List.Where(x => x.Id.ToString() == doc.id.ToString()).FirstOrDefault();
					if (title != null)
					{
						doc.title = title.VirtualClassRoomName;
						doc.className = "checklist-icon";
					}
				}
			}

			var results =docs.GroupBy(
				p => p.assignedDate,
				p => new CalendarViewModel
				{
					assignedById = p.assignedBy,
					createdById = p.createdById,
					type = p.type,
					id = p.id,
					start = p.start,
					additionalMessage = p.additionalMessage,
					assignedDate = p.assignedDate,
					title = p.title,
					assignedBy = p.assignedBy,
					userName = p.userName,
					className =p.className
				},
			(key, g) => new CalendarListViewModel { Date = key.ToString("yyyy-MM-dd"), Docs = g.ToList() });
			
			var c = new CalendarData();

			c.Data = results.ToList();

			return c;
		}
	}

}
