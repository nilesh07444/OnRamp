using Common.Data;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Document;
using Ramp.Contracts.Query.CustomDocument;
using Ramp.Contracts.Query.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuaCon.Reporting;
using static Ramp.Contracts.ViewModel.UserActivityAndPerformanceViewModel;

namespace Ramp.Services.QueryHandler.CustomDocument
{
    public class CustomDocumentScheculeReport : ReportingQueryHandler<CustomDocumentScheduleReportQuery>
    {
        private readonly ITransientReadRepository<Domain.Customer.Models.CustomDocument> _repository;
		private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
		private readonly ITransientReadRepository<DocumentUsage> _documentUsageRepository;
		private readonly IRepository<StandardUser> _userRepository;

		public CustomDocumentScheculeReport(ITransientReadRepository<Domain.Customer.Models.CustomDocument> repository,
			IRepository<AssignedDocument> assignedDocumentRepository,
			ITransientReadRepository<DocumentUsage> documentUsageRepository,
			IRepository<StandardUser> userRepository)
        {
            _repository = repository;
			_assignedDocumentRepository = assignedDocumentRepository;
			_documentUsageRepository = documentUsageRepository;
			_userRepository = userRepository;
        }

        public override void BuildReport(ReportDocument document, out string title, out string recepitents, CustomDocumentScheduleReportQuery data)
        {
			title = "Custom Document Summary";
			recepitents = data.Recepients;
			if (data.CustomDocumentSubmissionReportQuery.CustomDocumentIds.Count() == 1)
			{
				var checkLists = _repository.List.AsQueryable().Where(x => !x.Deleted && data.CustomDocumentSubmissionReportQuery.CustomDocumentIds.Contains(x.Id)).FirstOrDefault();
				title = checkLists.Title;

			}
			var section = CreateSection(title, PageOrientation.Landscape);
			// CreateHeader(data, section);

			var result = GetChecklistSummary(data.CustomDocumentSubmissionReportQuery);

			CreateTableData(result, data.CustomDocumentSubmissionReportQuery.ToggleFilter, section);

			document.AddElement(section);
		}

		private void CreateTableData(ChecklistInteractionModel data, string ToggleFilter, ReportSection section)
		{

			var grid = CreateGrid();
			var columns = new List<Tuple<string, int>>();
			var headers = ToggleFilter.Split(',').ToList();
			foreach (var header in headers)
			{
				columns.Add(new Tuple<string, int>(header, 30));
			}

			CreateTableHeader(grid, columns.ToArray());

			foreach (var dataItem in data.Checklist.ToList())
			{
				List<object> list = new List<object>();
				if (headers.Contains("ID Number"))
				{
					list.Add(dataItem.IdNumber);
				}
				if (headers.Contains("Employee Code"))
				{
					list.Add(dataItem.EmployeeCode);
				}
				if (headers.Contains("User Name"))
				{
					list.Add(dataItem.UserName);
				}
				if (headers.Contains("Viewed"))
				{
					list.Add(dataItem.Viewed ? "Yes" : "No");
				}
				if (headers.Contains("Date Assigned"))
				{
					list.Add(dataItem.DateAssigned);
				}
				if (headers.Contains("Date Viewed"))
				{
					list.Add(dataItem.DateViewed);
				}
				if (headers.Contains("Checks Completed"))
				{
					list.Add(dataItem.ChecksCompleted);
				}
				if (headers.Contains("Date Submitted"))
				{
					list.Add(dataItem.DateSubmitted == null || dataItem.Completed == "InComplete" ? "" : dataItem.DateSubmitted.ToString());
				}
				if (headers.Contains("Status"))
				{
					list.Add(dataItem.Completed);
				}
				if (headers.Contains("Access"))
				{
					list.Add(dataItem.Access);
				}
				if (headers.Contains("Group"))
				{
					list.Add(dataItem.Group);
				}

				CreateTableDataRow(grid, list.ToArray());
			}

			section.AddElement(grid);




		}

		public ChecklistInteractionModel GetChecklistSummary(CustomDocumentSubmissionReportQuery query)
		{
			var checkLists = _repository.List.AsQueryable().Where(x => !x.Deleted && query.CustomDocumentIds.Contains(x.Id)).ToList();

			var assignedDocs = _assignedDocumentRepository.GetAll().Where(c => query.CustomDocumentIds.Contains(c.DocumentId) && !c.Deleted).ToList();

			var docUses = _documentUsageRepository.List.AsQueryable().OrderByDescending(p => p.ViewDate).Where(c => query.CustomDocumentIds.Contains(c.DocumentId) && c.DocumentType==Domain.Customer.DocumentType.custom).ToList();

			var model = new List<ChecklistInteractionModel>();
			var result = new List<CheckLisInteractionModel>();
            if (assignedDocs != null && assignedDocs.Count > 0)
            {
                result = (from a in assignedDocs
                          join u in _userRepository.GetAll() on Guid.Parse(a.UserId) equals u.Id
                          join c in _repository.List.AsQueryable() on a.DocumentId equals c.Id
                          select new CheckLisInteractionModel
                          {
                              Id = a.DocumentId,
                              DocumentTitle = c.Title,
                              DateAssigned = a.AssignedDate,
                              UserName = u.FirstName + " " + u.LastName,
                              AssignedDocId = a.Id,
                              EmployeeCode = u.EmployeeNo,
                              IdNumber = u.IDNumber,
                              UserId = a.UserId

                          }).ToList();

                foreach (var item in result)
                {
                    var viewDocs = docUses.Where(c => c.UserId == item.UserId).FirstOrDefault();

                    item.Viewed = (viewDocs != null ) ? true : false;
                    item.DateViewed = (viewDocs != null ) ? viewDocs.ViewDate : DateTime.UtcNow;

                  //  var checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == item.AssignedDocId).FirstOrDefault();

                    //item.DateSubmitted = (viewDocs != null) ? viewDocs.SubmittedDate : DateTime.MinValue.Date;
                    item.Completed = (viewDocs != null ) ? "Completed" : "InComplete";
                    item.Status = (viewDocs != null ) ?  viewDocs.Status.ToString():string.Empty;
                    item.Access = (viewDocs != null && viewDocs.IsGlobalAccessed) ? "Global" : "Assigned";
                    
                   // item.ChecksCompleted = $"{checkListChapterUserResult.Where(c => c.IsChecked == true).Count()}/{checkListChapter.Count()}";

                }

                if (query.Status != null && query.Status.Any())
                {
                    result = result.Where(c => query.Status.Contains(c.Status)).ToList();
                }
                if (query.Access != null && query.Access.Any())
                {
                    result = result.Where(c => query.Access.Contains(c.Access)).ToList();
                }
                if (query.ToDate.HasValue && query.FromDate.HasValue)
                {
                    result = result.Where(c => c.DateAssigned.Date >= query.FromDate.Value.Date && c.DateAssigned.Date <= query.ToDate.Value.Date).ToList();
                }
                else if (query.ToDate.HasValue)
                {
                    result = result.Where(c => c.DateAssigned.Date <= query.ToDate.Value.Date).ToList();
                }
                else if (query.FromDate.HasValue)
                {
                    result = result.Where(c => c.DateAssigned.Date >= query.FromDate.Value.Date).ToList();
                }

                var check = result.Where(c => c.Id == query.CustomDocumentId).GroupBy(c => c.DocumentTitle).Select(f => f.FirstOrDefault()).ToList();
                foreach (var item in check)
                {
                    var m = new ChecklistInteractionModel
                    {
                        DocumentTitle = item.DocumentTitle,
                        Id = item.Id,
                        Checklist = result.Where(c => c.Id == item.Id).ToList()
                    };
                    model.Add(m);
                }
                return model.FirstOrDefault();
            }
            else
            {

                var checklist = checkLists.FirstOrDefault();
                //var checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.DocumentId == checklist.Id).ToList();
                //foreach (var item in checkListUserResult)
                //{
                //    var checklistresult = new CheckLisInteractionModel
                //    {
                //        Id = item.DocumentId,
                //        DocumentTitle = checklist.Title,
                //        UserId = item.UserId
                //    };
                //    var user = _userRepository.Find(Guid.Parse(item.UserId));
                //    checklistresult.Group = user?.Group?.Title ?? "";

                //    var viewDocs = docUses.Where(c => c.UserId == item.UserId).ToList();
                //    checklistresult.Viewed = (viewDocs != null && viewDocs.Any()) ? true : false;
                //    checklistresult.DateViewed = (viewDocs != null && viewDocs.Any()) ? viewDocs.FirstOrDefault().ViewDate : DateTime.UtcNow;

                //    checklistresult.DateSubmitted = item.SubmittedDate;
                //    checklistresult.Completed = item.Status ? "Completed" : "InComplete";
                //    checklistresult.Status = item.Status ? "1" : "0";
                //    checklistresult.Access = item.IsGlobalAccessed ? "Global" : "Assigned";

                //    //var checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().Where(c => c.DocumentId == item.DocumentId && c.IsGlobalAccessed && c.UserId == item.UserId).ToList();
                //    //var checkListChapter = _checkListChapterRepository.List.AsQueryable().Where(c => c.CheckListId == item.DocumentId && !c.Deleted).ToList();
                //    //checklistresult.ChecksCompleted = $"{checkListChapterUserResult.Where(c => c.IsChecked == true).Count()}/{checkListChapter.Count()}";
                //    //var userId = Guid.Parse(item.UserId);
                //    //var userDetail = _userRepository.List.Where(c => c.Id == userId).FirstOrDefault();

                //    //if (userDetail != null)
                //    //{
                //    //    checklistresult.UserName = userDetail.FirstName + " " + userDetail.LastName;
                //    //    checklistresult.EmployeeCode = userDetail.EmployeeNo;
                //    //    checklistresult.IdNumber = userDetail.IDNumber;
                //    //}
                //    result.Add(checklistresult);
                //}
                if (query.Status != null && query.Status.Any())
                {
                    result = result.Where(c => query.Status.Contains(c.Status)).ToList();
                }
                if (query.Access != null && query.Access.Any())
                {
                    result = result.Where(c => query.Access.Contains(c.Access)).ToList();
                }
                if (query.ToDate.HasValue && query.FromDate.HasValue)
                {
                    result = result.Where(c => c.DateSubmitted.Value.Date >= query.FromDate.Value.Date && c.DateSubmitted.Value.Date <= query.ToDate.Value.Date).ToList();
                }
                else if (query.ToDate.HasValue)
                {
                    result = result.Where(c => c.DateSubmitted.Value.Date <= query.ToDate.Value.Date).ToList();
                }
                else if (query.FromDate.HasValue)
                {
                    result = result.Where(c => c.DateSubmitted.Value.Date >= query.FromDate.Value.Date).ToList();
                }

                foreach (var obj in checkLists)
                {
                    List<CheckLisInteractionModel> list = new List<CheckLisInteractionModel>();
                    foreach (var item in result)
                    {
                        if (item.Id == obj.Id)
                        {
                            item.Group = _userRepository.GetAll().Where(x => x.Id == Guid.Parse(item.UserId)).FirstOrDefault().Group.Title;
                            list.Add(item);
                        }
                    }
                    var m = new ChecklistInteractionModel
                    {
                        DocumentTitle = obj.Title,
                        Id = obj.Id,
                        Checklist = list
                    };
                    model.Add(m);
                }
              
            }
            return model.FirstOrDefault();
        }
	}
}
