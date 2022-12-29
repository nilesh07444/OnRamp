using Common.Data;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Ramp.Contracts.Query.CheckList;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;

using StandardUserGroup = Domain.Customer.Models.Groups.StandardUserGroup;

namespace Ramp.Services.QueryHandler {
	public class CheckListExportReportQueryHandler : ReportingQueryHandler<CheckListExportReportQuery> {
		private readonly IRepository<CheckList> _checklistRepository;
		private readonly IRepository<CheckListChapter> _checkListChapterRepository;
		private readonly IRepository<StandardUser> _userRepository;
		private readonly IRepository<CheckListChapterUserResult> _checkListChapterUserResultRepository;
		private readonly IRepository<CheckListChapterUserUploadResult> _checkListChapterUserUploadResultRepository;
		private readonly IRepository<CheckListUserResult> _checkListUserResultRepository;
		private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
		private Guid userId;

		private readonly IRepository<StandardUserGroup> _standardUserGroupRepository;
		private readonly IRepository<CustomerGroup> _groupRepository;

		public CheckListExportReportQueryHandler(

			IRepository<StandardUserGroup> standardUserGroupRepository,
			IRepository<CustomerGroup> groupRepository,
			IRepository<CheckList> checklistRepository,
			IRepository<CheckListChapterUserResult> checkListChapterUserResultRepository,
		   IRepository<CheckListChapter> checkListChapterRepository,
		   IRepository<CheckListUserResult> checkListUserResultRepository,
		   IRepository<AssignedDocument> assignedDocumentRepository,
		   IRepository<CheckListChapterUserUploadResult> checkListChapterUserUploadResultRepository,
			IRepository<StandardUser> userRepository) {

			_standardUserGroupRepository = standardUserGroupRepository;
			_groupRepository = groupRepository;

			_checklistRepository = checklistRepository;
			_checkListChapterRepository = checkListChapterRepository;
			_checkListChapterUserResultRepository = checkListChapterUserResultRepository;
			_assignedDocumentRepository = assignedDocumentRepository;
			_checkListChapterUserUploadResultRepository = checkListChapterUserUploadResultRepository;
			_checkListUserResultRepository = checkListUserResultRepository;
			_userRepository = userRepository;
		}

		public override void BuildReport(ReportDocument document, out string title,out string recepitents, CheckListExportReportQuery data) {

			var checkList = _checklistRepository.Find(data.ResultId);
			var checkListChapter = _checkListChapterRepository.GetAll().OrderBy(c=>c.Number).Where(c => c.CheckListId == data.ResultId).ToList();
			if (checkList == null)
				throw new ArgumentNullException($"No checkList found with id : {data.ResultId}");
			title = checkList.Title;
			recepitents = data.Recepients;
			data.AddOnrampBranding = true;
			userId = data.UserId;
			var assignedDocument = _assignedDocumentRepository.GetAll().Where(c => c.AssignedDate.Date <= DateTime.UtcNow.Date).OrderByDescending(c => c.AssignedDate).FirstOrDefault(c => c.UserId == userId.ToString() && c.DocumentId == checkList.Id);
			var section = CreateSection(title);
			if (data.IsDetail) {
				CreatePersonalDetails(data.ResultId, assignedDocument, section);

				if (data.IsChecklistTracked)
					CreateChecklistTrackedTables(data.ResultId, checkListChapter, assignedDocument, section);
				else
					CreateChecklistTables(data.ResultId, checkListChapter, assignedDocument, section);
			} else {
				CreateChecklistChapterTables(checkListChapter,section);
			}

			document.AddElement(section);
		}

		private void CreateChecklistTables(string checklistId,List<CheckListChapter> checkListChapter, AssignedDocument assignDoc, ReportSection section) {
			var grid = CreateGrid();
			var columns = new[]
			{
				new Tuple<string, int>("Check Item", 30),
				new Tuple<string, int>("Check Status", 40),
				new Tuple<string, int>("Comments", 40),
				new Tuple<string, int>("Attachments", 100)
			};


			CreateTableHeader(grid, columns);

			foreach (var chapter in checkListChapter.OrderBy(c=>c.Number)) {
				var checkListChapterUserResult = new CheckListChapterUserResult();
				var  chapterUploads = new List<CheckListChapterUserUploadResult>();
				if (assignDoc!=null) {
					checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.AssignedDocumentId == assignDoc.Id && c.CheckListChapterId == chapter.Id && !c.IsGlobalAccessed);
					chapterUploads = _checkListChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.AssignedDocumentId == assignDoc.Id && c.CheckListChapterId == chapter.Id && !c.IsGlobalAccessed).ToList();
				} else {
					checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.DocumentId == checklistId && c.UserId==userId.ToString() && c.IsGlobalAccessed && c.CheckListChapterId == chapter.Id);
					chapterUploads = _checkListChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.DocumentId == checklistId && c.UserId==userId.ToString() && c.IsGlobalAccessed && c.CheckListChapterId == chapter.Id).ToList();
				}

				var attachments = new List<string>();
				foreach (var item in chapterUploads) {
					if (item.Upload != null) {
						attachments.Add(item.Upload.Name);
					}
				}
				
				var status = (checkListChapterUserResult!=null && checkListChapterUserResult.IsChecked) ? "Yes" : "No";
				CreateTableDataRow(grid,
					chapter.Title,
					status,
					checkListChapterUserResult!=null?checkListChapterUserResult.IssueDiscription:"",
					string.Join(",", attachments)
					);
			}

			section.AddElement(grid);
		}
		private void CreateChecklistTrackedTables(string checklistId,List<CheckListChapter> checkListChapter, AssignedDocument assignDoc, ReportSection section) {
			var grid = CreateGrid();
			var columns = new[]
			{
				new Tuple<string, int>("Check Item", 30),
				new Tuple<string, int>("Check Status", 40),
				new Tuple<string, int>("Date Completed", 30),
				new Tuple<string, int>("Comments", 30),

				new Tuple<string, int>("Attachments", 100)
			};


			CreateTableHeader(grid, columns);

			foreach (var chapter in checkListChapter.OrderBy(c => c.Number)) {

				var checkListChapterUserResult = new CheckListChapterUserResult();
				var chapterUploads = new List<CheckListChapterUserUploadResult>();

				if (assignDoc!=null) {
					checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.AssignedDocumentId == assignDoc.Id && c.CheckListChapterId == chapter.Id && !c.IsGlobalAccessed).FirstOrDefault();
					chapterUploads = _checkListChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.AssignedDocumentId == assignDoc.Id && c.CheckListChapterId == chapter.Id && !c.IsGlobalAccessed).ToList();
				} else {
					checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.DocumentId == checklistId && c.UserId == userId.ToString() && c.CheckListChapterId == chapter.Id && c.IsGlobalAccessed).FirstOrDefault();
					chapterUploads = _checkListChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.DocumentId == checklistId && c.UserId==userId.ToString() && c.CheckListChapterId == chapter.Id && c.IsGlobalAccessed).ToList();
				}

				var attachments = new List<string>();
				foreach (var item in chapterUploads) {
					if (item.Upload != null) {
						attachments.Add(item.Upload.Name);
					}
				}

				var status = (checkListChapterUserResult != null && checkListChapterUserResult.IsChecked) ? "Yes" : "No";
				var dateCompleted = ((checkListChapterUserResult != null) ? (checkListChapterUserResult.ChapterTrackedDate != null ? checkListChapterUserResult.ChapterTrackedDate.Value.ToLocalTime().ToString("G") : "") : "");
				//var dateCompleted = (checkListChapterUserResult != null) ? checkListChapterUserResult.ChapterTrackedDate.Value.ToLocalTime().ToString("G") : "";				
				CreateTableDataRow(grid,
					chapter.Title,
					status,
					dateCompleted,
					checkListChapterUserResult != null ? checkListChapterUserResult.IssueDiscription : "",
					string.Join(",", attachments)
					);
			}

			section.AddElement(grid);
		}
		private void CreatePersonalDetails(string checkListId, AssignedDocument assignedDocument, ReportSection section) {
			var grid = CreateGrid();
			CreateTableDataRowWithStyles(grid, HeaderStyle, new[] { "Details" });

			var user = _userRepository.Find(userId);

			//below code added by neeraj

			//below code added by neeraj
			
					var groups = _groupRepository.List;

					var groupList = _standardUserGroupRepository.List.Where(c => c.UserId.ToString() == user.Id.ToString()).ToList();

					string name = null;
					//List<string> name = new List<string>();
					if (groupList.Count > 0)
					{
						foreach (var g in groupList)
						{
							foreach (var gl in groups)
							{
								if (gl.Id == g.GroupId)
								{
									//name.Add(gl.Title);
									if (name != null)
										name = name + "," + gl.Title;
									else name = name + gl.Title;
								}
							}
						}
					}
					if (name != null)
					{
				user.Group = new CustomerGroup { Title = name};
					}
				
				

			
			//code region end

			var userDetail = Project.UserViewModelFrom(user);
			var checkListUserResult = new CheckListUserResult();
			if (assignedDocument != null) {
				 checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == assignedDocument.Id && !c.IsGlobalAccessed).FirstOrDefault();
			} else {
				checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.DocumentId == checkListId && c.UserId==userId.ToString() && c.IsGlobalAccessed).FirstOrDefault();
			}

			var status = (checkListUserResult != null && checkListUserResult.Status) ? "Completed" : "Incomplete";

			var userDetailModel = new UserDetail() {
				Name = userDetail.FullName,
				Email = userDetail.EmailAddress,
				Status = status,
				DateCompleted = checkListUserResult != null ? checkListUserResult.SubmittedDate.ToShortDateString() : "",
				Group= user.Group.Title
			};
			if (assignedDocument != null) {
				userDetailModel.DateAssigned = assignedDocument.AssignedDate.ToShortDateString();
			}


			var fields = new List<Tuple<string, Func<UserDetail, string>>>();
			fields.Add(new Tuple<string, Func<UserDetail, string>>(
				"Name", model => model.Name));
			fields.Add(new Tuple<string, Func<UserDetail, string>>(
				"Email", model => model.Email));
			fields.Add(new Tuple<string, Func<UserDetail, string>>(
				"Status", model => model.Status));
			fields.Add(new Tuple<string, Func<UserDetail, string>>(
				"Date Assigned", model => model.DateAssigned));
			fields.Add(new Tuple<string, Func<UserDetail, string>>(
				"Date Completed", model => model.DateCompleted));
			fields.Add(new Tuple<string, Func<UserDetail, string>>(
				"Group", model => model.Group));


			foreach (var field in fields) {
				var row = new GridRowBlock();
				row.AddElement(new GridCellBlock(field.Item1,
					new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
				row.AddElement(new GridCellBlock(field.Item2(userDetailModel)));

				grid.AddElement(row);
			}

			section.AddElement(grid);
		}

		//public override void BuildReport(ReportDocument document, out string title, CheckListExportReportQuery data) {

		//	var checkList = _checklistRepository.Find(data.ResultId);
		//	var checkListChapter = _checkListChapterRepository.GetAll().Where(c => c.CheckListId == data.ResultId);
		//	if (checkList == null)
		//		throw new ArgumentNullException($"No checkList found with id : {data.ResultId}");
		//	title = checkList.Title;
		//	data.AddOnrampBranding = true;
		//	userId = data.UserId;

		//	var assignedDocument = _assignedDocumentRepository.GetAll().Where(c => c.AssignedDate.Date <= DateTime.UtcNow.Date).OrderByDescending(c => c.AssignedDate).FirstOrDefault(c => c.UserId == userId.ToString() && c.DocumentId == checkList.Id);

		//	document.AddElement(CreateHeaderSection(checkList));

		//	document.AddElement(CreateUserSection(checkList.Id, assignedDocument));

		//	var checkListChapters = _checkListChapterRepository.GetAll().Where(c => c.CheckListId == checkList.Id).OrderBy(c => c.Number).ToList();
		//	var count = 1;
		//	foreach (var item in checkListChapters) {

		//		document.AddElement(CreateChapterSection(item, assignedDocument != null ? assignedDocument.Id : string.Empty, count));
		//		count++;
		//	}

		//}
		//private ReportSection CreateHeaderSection(CheckList checkList) {
		//	var section = CreateSection("", PageOrientation.Portrait, false);
		//	section.AddElement(CreateParagraph(checkList.Title, Centered));
		//	section.AddElement(CreateCenteredHorizontalRule());
		//	section.AddElement(AddBreak(30));
		//	return section;
		//}

		private ReportSection CreateUserSection(string checkListId, AssignedDocument assignedDocument) {
			var section = CreateSection("", PageOrientation.Portrait, false);

			var user = _userRepository.Find(userId);
			var userDetail = Project.UserViewModelFrom(user);

			var checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == assignedDocument.Id).FirstOrDefault();

			section.AddElement(CreateParagraph("User: " + userDetail.FullName, headingFont, LeftAligned));
			section.AddElement(CreateParagraph("Email: " + userDetail.EmailAddress, headingFont, LeftAligned));

			var status = (checkListUserResult != null && checkListUserResult.Status) ? "Completed" : "Pending";

			section.AddElement(CreateParagraph("Document Status: " + status, headingFont, LeftAligned));

			section.AddElement(CreateParagraph("Date Assigned: " + assignedDocument.AssignedDate, headingFont, LeftAligned));
			if (checkListUserResult != null)
				section.AddElement(CreateParagraph("Date Completed: " + checkListUserResult.SubmittedDate, headingFont, LeftAligned));
			else
				section.AddElement(CreateParagraph("Date Completed: ", headingFont, LeftAligned));

			section.AddElement(AddBreak(30));

			return section;
		}

		private ReportSection CreateChapterSection(CheckListChapter checkListChapter, string assignDocId, int count) {
			var section = CreateSection("", PageOrientation.Portrait, false);
			var checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == assignDocId && c.CheckListChapterId == checkListChapter.Id).FirstOrDefault();
			if (checkListChapterUserResult != null && checkListChapterUserResult.IsChecked) {
				section.AddElement(CreateParagraph(count.ToString() + ". " + "[ X ]  " + checkListChapter.Title, LeftAligned));
			} else {
				section.AddElement(CreateParagraph(count.ToString() + ". " + "[ ]  " + checkListChapter.Title, LeftAligned));
			}
			section.AddElement(AddBreak(5));
			if (checkListChapterUserResult != null && !string.IsNullOrEmpty(checkListChapterUserResult.IssueDiscription)) {
				section.AddElement(CreateParagraph("Additional Notes: " + checkListChapterUserResult.IssueDiscription, LeftAligned));

				//section.AddElement(CreateParagraph(checkListChapterUserResult.IssueDiscription, LeftAligned));
			}
			section.AddElement(AddBreak(5));

			section.AddElement(AddBreak(30));
			return section;
		}
		private void CreateChecklistChapterTables(List<CheckListChapter> checkListChapter, ReportSection section) {
			var grid = CreateGrid();
			var columns = new[]
			{
				new Tuple<string, int>("Check Item", 30),
				new Tuple<string, int>("Check Status", 40),
				new Tuple<string, int>("Comments", 40),
				new Tuple<string, int>("Attachments", 100)
			};

			CreateTableHeader(grid, columns);

			foreach (var chapter in checkListChapter.OrderBy(c => c.Number)) {
				
				CreateTableDataRow(grid,
					chapter.Title,
					"",
					"",
					""
					);
			}

			section.AddElement(grid);
		}
	}

	public class UserDetail {

		public string Name { get; set; }
		public string Email { get; set; }
		public string Status { get; set; }
		public string DateAssigned { get; set; }
		public string DateCompleted { get; set; }
		public string Group { get; set; }

	}

}
