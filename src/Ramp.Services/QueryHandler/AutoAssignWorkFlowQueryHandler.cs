using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.ViewModel;
using System.Collections.Generic;
using System.Linq;


namespace Ramp.Services.QueryHandler
{
    public class AutoAssignWorkFlowQueryHandler :
		IQueryHandler<FetchAllRecordsQuery, List<AutoAssignWorkflowViewModel>>,
		IQueryHandler<FetchByIdQuery, AutoAssignWorkflowViewModel> {

		private readonly IRepository<AutoAssignWorkflow> _workflowList;
		private readonly IRepository<AutoAssignDocuments> _docsRepository;
		private readonly IRepository<AutoAssignGroups> _grpRepository;

		public AutoAssignWorkFlowQueryHandler(
			IRepository<AutoAssignWorkflow> workflowList,
			IRepository<AutoAssignDocuments> docsRepository,
			IRepository<AutoAssignGroups> grpRepository
			)
		{
			_workflowList = workflowList;
			_docsRepository = docsRepository;
			_grpRepository = grpRepository;
		}

		public List<AutoAssignWorkflowViewModel> ExecuteQuery(FetchAllRecordsQuery queryParameters)
		{
			var response = new List<AutoAssignWorkflowViewModel>();

			try
			{

				var auto = _workflowList.List.Where(c=>c.IsDeleted == false).Select(x => new AutoAssignWorkflowViewModel
				{
					Id = x.Id.ToString(),
					WorkflowName = x.WorkflowName,
					DateCreated = x.DateCreated,
					SendNotiEnabled = x.SendNotiEnabled
				}).ToList();
				var grp = _grpRepository.List.ToList();
				var docs = _docsRepository.List.ToList();

				foreach (var a in auto)
				{
					var data = new AutoAssignWorkflowViewModel();
					data.DocumentList = new List<AutoWorkFlowDocs>();
					List<string> groups = new List<string>();
					data.Id = a.Id;
					data.WorkflowName = a.WorkflowName;
					data.SendNotiEnabled = a.SendNotiEnabled;
					data.DateCreated = a.DateCreated;

					foreach (var g in grp)
					{
						if (a.Id.ToString() == g.WorkFlowId.ToString())
						{
							groups.Add(g.GroupId.ToString());
						}
					}
					data.GroupIds = groups.ToArray();
					foreach (var d in docs)
					{
						if (a.Id.ToString() == d.WorkFlowId.ToString())
						{
							data.DocumentList.Add(new AutoWorkFlowDocs { Id = d.DocumentId.ToString(), Title = "", Type = d.Type.ToString(), Order = d.Order, AditionalMsg = "" });
						}
					}

					response.Add(data);
				}
			}
			catch (System.Exception ex)
			{

				throw;
			}
			
			return response;
		}

		public AutoAssignWorkflowViewModel ExecuteQuery(FetchByIdQuery query)
		{
			var data = new AutoAssignWorkflowViewModel();

			try
			{
				var auto = _workflowList.List.Where(c => c.Id.ToString() == query.Id.ToString() && c.IsDeleted == false).Select(x => new AutoAssignWorkflowViewModel
				{
					Id = x.Id.ToString(),
					WorkflowName = x.WorkflowName,
					DateCreated = x.DateCreated,
					SendNotiEnabled = x.SendNotiEnabled
				}).FirstOrDefault();
				var grp = _grpRepository.List.ToList();
				var docs = _docsRepository.List.ToList();
				
					data.DocumentList = new List<AutoWorkFlowDocs>();
					List<string> groups = new List<string>();
					data.Id = auto.Id;
					data.WorkflowName = auto.WorkflowName;
					data.SendNotiEnabled = auto.SendNotiEnabled;
					data.DateCreated = data.DateCreated;

					foreach (var g in grp)
					{
						if (data.Id.ToString() == g.WorkFlowId.ToString())
						{
							groups.Add(g.GroupId.ToString());
						}
					}
					data.GroupIds = groups.ToArray();
					foreach (var d in docs)
					{
						if (data.Id.ToString() == d.WorkFlowId.ToString())
						{
							data.DocumentList.Add(new AutoWorkFlowDocs { Id = d.DocumentId.ToString(), Title = "", Type = d.Type.ToString(), Order = d.Order, AditionalMsg = "" });
						}
					}
					
			}
			catch (System.Exception ex)
			{

				throw;
			}

			return data;
		}
	}
}
