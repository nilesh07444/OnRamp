using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Data.EF.Customer.Migrations;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Policy;
using javax.xml.ws;
using Ramp.Contracts.Query.CheckList;
using Ramp.Contracts.Query.CustomDocument;
using Ramp.Contracts.Query.TrainingManual;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler.CustomDocument;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;

using StandardUserGroup = Domain.Customer.Models.Groups.StandardUserGroup;

namespace Ramp.Services.QueryHandler
{
    public class CustomDocumentSummaryExportReportQueryHandler : ReportingQueryHandler<CustomDocumentSummaryExportReportQuery>
    {
        private readonly IRepository<CheckList> _checklistRepository;
        private readonly IRepository<Domain.Customer.Models.CustomDocument> _customDocumentrepository;
        private readonly IRepository<CheckListChapter> _checkListChapterRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<CheckListChapterUserResult> _checkListChapterUserResultRepository;
        private readonly IRepository<CheckListChapterUserUploadResult> _checkListChapterUserUploadResultRepository;
        private readonly IRepository<CheckListUserResult> _checkListUserResultRepository;
        private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
        private Guid userId;

        private readonly IRepository<StandardUserGroup> _standardUserGroupRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        private readonly IQueryExecutor _queryDispatcher;
        private readonly IRepository<TrainingManualChapterUserResult> _trainingManualChapterUserResultRepository;
        private readonly IRepository<TrainingManualChapterUserUploadResult> _trainingManualChapterUserUploadResultRepository;
        private readonly IRepository<MemoChapterUserResult> _MemoChapterUserResultRepository;
        private readonly IRepository<MemoChapterUserUploadResult> _MemoChapterUserUploadResultRepository;
        private readonly IRepository<TestChapterUserResult> _TestChapterUserResultRepository;
        private readonly IRepository<TestChapterUserUploadResult> _TestChapterUserUploadResultRepository;
        private readonly IRepository<AcrobatFieldChapterUserResult> _AcrobatFieldChapterUserResultRepository;
        private readonly IRepository<AcrobatFieldChapterUserUploadResult> _AcrobatFieldChapterUserUploadResultRepository;
        private readonly IRepository<PolicyContentBoxUserResult> _PolicyContentBoxUserResultRepository;
        private readonly IRepository<PolicyContentBoxUserUploadResult> _PolicyContentBoxUserUploadResultRepository;

        public CustomDocumentSummaryExportReportQueryHandler(IQueryExecutor queryDispatcher,
IRepository<Domain.Customer.Models.CustomDocument> customDocumentrepository,
            IRepository<StandardUserGroup> standardUserGroupRepository,
            IRepository<CustomerGroup> groupRepository,
            IRepository<CheckList> checklistRepository,
            IRepository<CheckListChapterUserResult> checkListChapterUserResultRepository,
           IRepository<CheckListChapter> checkListChapterRepository,
           IRepository<CheckListUserResult> checkListUserResultRepository,
           IRepository<AssignedDocument> assignedDocumentRepository,
           IRepository<CheckListChapterUserUploadResult> checkListChapterUserUploadResultRepository,
            IRepository<StandardUser> userRepository,
            IRepository<TrainingManualChapterUserResult> trainingManualChapterUserResultRepository,
            IRepository<TrainingManualChapterUserUploadResult> trainingManualChapterUserUploadResultRepository,
           IRepository<MemoChapterUserResult> MemoChapterUserResultRepository,
            IRepository<MemoChapterUserUploadResult> MemoChapterUserUploadResultRepository,
    IRepository<TestChapterUserResult> TestChapterUserResultRepository,
            IRepository<TestChapterUserUploadResult> TestChapterUserUploadResultRepository,
  IRepository<AcrobatFieldChapterUserResult> AcrobatFieldChapterUserResultRepository,
            IRepository<AcrobatFieldChapterUserUploadResult> AcrobatFieldChapterUserUploadResultRepository,
IRepository<PolicyContentBoxUserResult> PolicyContentBoxUserResultRepository,
IRepository<PolicyContentBoxUserUploadResult> PolicyContentBoxUserUploadResultRepository
            )
        {
            _customDocumentrepository = customDocumentrepository;
            _standardUserGroupRepository = standardUserGroupRepository;
            _groupRepository = groupRepository;
            _queryDispatcher = queryDispatcher;
            _checklistRepository = checklistRepository;
            _checkListChapterRepository = checkListChapterRepository;
            _checkListChapterUserResultRepository = checkListChapterUserResultRepository;
            _assignedDocumentRepository = assignedDocumentRepository;
            _checkListChapterUserUploadResultRepository = checkListChapterUserUploadResultRepository;
            _checkListUserResultRepository = checkListUserResultRepository;
            _userRepository = userRepository;
            _trainingManualChapterUserResultRepository = trainingManualChapterUserResultRepository;
            _trainingManualChapterUserUploadResultRepository = trainingManualChapterUserUploadResultRepository;
            _MemoChapterUserResultRepository = MemoChapterUserResultRepository;
            _MemoChapterUserUploadResultRepository = MemoChapterUserUploadResultRepository;
            _TestChapterUserResultRepository = TestChapterUserResultRepository;
            _TestChapterUserUploadResultRepository = TestChapterUserUploadResultRepository;
            _AcrobatFieldChapterUserResultRepository = AcrobatFieldChapterUserResultRepository;
            _AcrobatFieldChapterUserUploadResultRepository = AcrobatFieldChapterUserUploadResultRepository;
            _PolicyContentBoxUserResultRepository = PolicyContentBoxUserResultRepository;
            _PolicyContentBoxUserUploadResultRepository = PolicyContentBoxUserUploadResultRepository;

        }

        public override void BuildReport(ReportDocument document, out string title, out string recepitents, CustomDocumentSummaryExportReportQuery data)
        {

            var customDocumentList = _queryDispatcher.Execute<FetchByIdQuery, CustomDocumentModel>(new FetchByIdQuery
            {
                Id = data.CustomDocumentID
            });
            //var checkListChapter = _checkListChapterRepository.GetAll().OrderBy(c=>c.Number).Where(c => c.CheckListId == data.ResultId).ToList();
            if (customDocumentList == null)
                throw new ArgumentNullException($"No customDocumentListfound with id : {data.CustomDocumentID}");
            title = customDocumentList.Title;
            recepitents = data.Recepients;
            data.AddOnrampBranding = false;
            userId = data.UserId;
            var assignedDocument = _assignedDocumentRepository.GetAll().Where(c => c.AssignedDate.Date <= DateTime.UtcNow.Date).OrderByDescending(c => c.AssignedDate).FirstOrDefault(c => c.UserId == userId.ToString() && c.DocumentId == customDocumentList.Id);
            var section = CreateSection(title);
            if (data.IsDetail)
            {
                CreatePersonalDetails(data.CustomDocumentID, assignedDocument, section);

                //if (data.IsChecklistTracked)
                //	CreateChecklistTrackedTables(data.CustomDocumentID, customDocumentList, assignedDocument, section);
                //else
                CreateChecklistTables(data.CustomDocumentID, customDocumentList, assignedDocument, section);
            }
            else
            {
                //CreateChecklistChapterTables(customDocumentList, section);
            }

            document.AddElement(section);
        }

        private void CreateChecklistTables(string checklistId, CustomDocumentModel customDocument, AssignedDocument assignDoc, ReportSection section)
        {
            var grid = CreateGrid();
            var columns = new[]
            {
                new Tuple<string, int>("Section Type ", 30),
                new Tuple<string, int>("Section Title", 40),
                new Tuple<string, int>("Field Title", 40),
                new Tuple<string, int>("Field Value", 40),
                new Tuple<string, int>("Attachments", 100),
                    new Tuple<string, int>("Notes", 100),
                        new Tuple<string, int>("Signature", 100),
                        new Tuple<string, int>("Answer", 100),
                        new Tuple<string, int>("Checked", 100),
                        new Tuple<string, int>("Acceptance", 100),
    new Tuple<string, int>("Date Submitted", 100)
};


            CreateTableHeader(grid, columns);

            #region ["Activity Book/CheckList"]

            foreach (var chapter in customDocument.CLContentModels)
            {
                var checkListChapterUserResult = new CheckListChapterUserResult();
                var chapterUploads = new List<CheckListChapterUserUploadResult>();
                if (assignDoc != null)
                {
                    checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.AssignedDocumentId == assignDoc.Id && c.CheckListChapterId == chapter.Id && !c.IsGlobalAccessed);
                    chapterUploads = _checkListChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.AssignedDocumentId == assignDoc.Id && c.CheckListChapterId == chapter.Id && !c.IsGlobalAccessed).ToList();
                }
                else
                {
                    checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.DocumentId == chapter.Id && c.UserId == userId.ToString() && c.IsGlobalAccessed && c.CheckListChapterId == chapter.Id);
                    chapterUploads = _checkListChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.DocumentId == chapter.Id && c.UserId == userId.ToString() && c.IsGlobalAccessed && c.CheckListChapterId == chapter.Id).ToList();
                }

                var attachments = new List<string>();
                var signatureURL = string.Empty;
                if (chapter.Attachments.Any())
                {
                    foreach (var attachment in chapter.Attachments)
                    {
                        attachments.Add(attachment.Name);
                    }
                }
                foreach (var item in chapterUploads)
                {
                    if (item.Upload != null && !item.isSignOff)
                    {
                        attachments.Add(item.Upload.Name);
                    }
                    else
                    {
                        signatureURL = item.Upload.Name;
                    }
                }

                var status = (checkListChapterUserResult != null && checkListChapterUserResult.IsChecked) ? "Yes" : "No";
                CreateTableDataRow(grid,
                    "Check",
                    chapter.Title,
                    string.Empty,
                    string.Empty,
                    string.Join(",", attachments),
                   checkListChapterUserResult != null ? checkListChapterUserResult.IssueDiscription : string.Empty,
                    signatureURL,
                    string.Empty,
                   status,
                    string.Empty,
                 customDocument.CreatedOn
                    );
            }
            #endregion

            #region ["Training Manual"]

            foreach (var chapter in customDocument.TMContentModels)
            {
                var TrainingManualChapterUserResult = new TrainingManualChapterUserResult();
                var chapterUploads = new List<TrainingManualChapterUserUploadResult>();
                if (assignDoc != null)
                {
                    TrainingManualChapterUserResult = _trainingManualChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.AssignedDocumentId == assignDoc.Id && c.TrainingManualChapterId == chapter.Id && !c.IsGlobalAccessed);

                    chapterUploads = _trainingManualChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.AssignedDocumentId == assignDoc.Id && c.TrainingManualChapterId == chapter.Id && !c.IsGlobalAccessed).ToList();

                }
                else
                {
                    TrainingManualChapterUserResult = _trainingManualChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.DocumentId == chapter.Id && c.UserId == userId.ToString() && c.IsGlobalAccessed && c.TrainingManualChapterId == chapter.Id);
                    chapterUploads = _trainingManualChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.DocumentId == chapter.Id && c.UserId == userId.ToString() && c.IsGlobalAccessed && c.TrainingManualChapterId == chapter.Id).ToList();
                }
                var signatureURL = string.Empty;
                var attachments = new List<string>();
                if (chapter.Attachments.Any())
                {
                    foreach (var attachment in chapter.Attachments)
                    {
                        attachments.Add(attachment.Name);
                    }
                }
                foreach (var item in chapterUploads)
                {
                    if (item.Upload != null && !item.isSignOff)
                    {
                        attachments.Add(item.Upload.Name);
                    }
                    else
                    {
                        signatureURL = item.Upload.Name;
                    }
                }

                var status = (TrainingManualChapterUserResult != null && TrainingManualChapterUserResult.IsChecked) ? "Yes" : "No";
                CreateTableDataRow(grid,
                    "Content",
                    chapter.Title,
                    string.Empty,
                    string.Empty,
                    string.Join(",", attachments),
                   TrainingManualChapterUserResult != null ? TrainingManualChapterUserResult.IssueDiscription : string.Empty,
                    signatureURL,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                 customDocument.CreatedOn
                    );
            }
            #endregion

            #region ["Memo"]

            foreach (var chapter in customDocument.MemoContentModels)
            {
                var MemoChapterUserResult = new MemoChapterUserResult();
                var chapterUploads = new List<MemoChapterUserUploadResult>();
                if (assignDoc != null)
                {
                    MemoChapterUserResult = _MemoChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.AssignedDocumentId == assignDoc.Id && c.MemoChapterId == chapter.Id && !c.IsGlobalAccessed);

                    chapterUploads = _MemoChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.AssignedDocumentId == assignDoc.Id && c.MemoChapterId == chapter.Id && !c.IsGlobalAccessed).ToList();

                }
                else
                {
                    MemoChapterUserResult = _MemoChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.DocumentId == chapter.Id && c.UserId == userId.ToString() && c.IsGlobalAccessed && c.MemoChapterId == chapter.Id);
                    chapterUploads = _MemoChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.DocumentId == chapter.Id && c.UserId == userId.ToString() && c.IsGlobalAccessed && c.MemoChapterId == chapter.Id).ToList();
                }
                var signatureURL = string.Empty;
                var attachments = new List<string>();
                if (chapter.Attachments.Any())
                {
                    foreach (var attachment in chapter.Attachments)
                    {
                        attachments.Add(attachment.Name);
                    }
                }
                foreach (var item in chapterUploads)
                {
                    if (item.Upload != null && !item.isSignOff)
                    {
                        attachments.Add(item.Upload.Name);
                    }
                    else
                    {
                        signatureURL = item.Upload.Name;
                    }
                }
                CreateTableDataRow(grid,
                    DocumentType.Memo,
                    chapter.Title,
                    string.Empty,
                    string.Empty,
                    string.Join(",", attachments),
                   MemoChapterUserResult != null ? MemoChapterUserResult.IssueDiscription : string.Empty,
                    signatureURL,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                 customDocument.CreatedOn
                    );
            }
            #endregion

            #region ["Test"]

            foreach (var chapter in customDocument.TestContentModels)
            {
                var TestChapterUserResult = new TestChapterUserResult();
                var chapterUploads = new List<TestChapterUserUploadResult>();
                if (assignDoc != null)
                {
                    TestChapterUserResult = _TestChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.AssignedDocumentId == assignDoc.Id && c.TestChapterId == chapter.Id && !c.IsGlobalAccessed);

                    chapterUploads = _TestChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.AssignedDocumentId == assignDoc.Id && c.TestChapterId == chapter.Id && !c.IsGlobalAccessed).ToList();

                }
                else
                {
                    TestChapterUserResult = _TestChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.DocumentId == chapter.Id && c.UserId == userId.ToString() && c.IsGlobalAccessed && c.TestChapterId == chapter.Id);
                    chapterUploads = _TestChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.DocumentId == chapter.Id && c.UserId == userId.ToString() && c.IsGlobalAccessed && c.TestChapterId == chapter.Id).ToList();
                }
                var signatureURL = string.Empty;
                var attachments = new List<string>();
                if (chapter.Attachments.Any())
                {
                    foreach (var attachment in chapter.Attachments)
                    {
                        attachments.Add(attachment.Name);
                    }
                }
                foreach (var item in chapterUploads)
                {
                    if (item.Upload != null && !item.isSignOff)
                    {
                        attachments.Add(item.Upload.Name);
                    }
                    else
                    {
                        signatureURL = item.Upload.Name;
                    }
                }

                var status = (TestChapterUserResult != null && TestChapterUserResult.IsChecked) ? "Yes" : "No";
                CreateTableDataRow(grid,
                    "Question",
                    chapter.Title,
                    string.Empty,
                    string.Empty,
                    string.Join(",", attachments),
                   TestChapterUserResult != null ? TestChapterUserResult.IssueDiscription : string.Empty,
                    signatureURL,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                 customDocument.CreatedOn
                    );
            }
            #endregion

            #region ["Acrobat Field"]

            foreach (var chapter in customDocument.AcrobatFieldContentModels)
            {
                var AcrobatFieldChapterUserResult = new AcrobatFieldChapterUserResult();
                var chapterUploads = new List<AcrobatFieldChapterUserUploadResult>();
                if (assignDoc != null)
                {
                    AcrobatFieldChapterUserResult = _AcrobatFieldChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.AssignedDocumentId == assignDoc.Id && c.AcrobatFieldChapterId == chapter.Id && !c.IsGlobalAccessed);

                    chapterUploads = _AcrobatFieldChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.AssignedDocumentId == assignDoc.Id && c.AcrobatFieldChapterId == chapter.Id && !c.IsGlobalAccessed).ToList();

                }
                else
                {
                    AcrobatFieldChapterUserResult = _AcrobatFieldChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.DocumentId == chapter.Id && c.UserId == userId.ToString() && c.IsGlobalAccessed && c.AcrobatFieldChapterId == chapter.Id);
                    chapterUploads = _AcrobatFieldChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.DocumentId == chapter.Id && c.UserId == userId.ToString() && c.IsGlobalAccessed && c.AcrobatFieldChapterId == chapter.Id).ToList();
                }
                var signatureURL = string.Empty;
                var attachments = new List<string>();
                if (chapter.Attachments.Any())
                {
                    foreach (var attachment in chapter.Attachments)
                    {
                        attachments.Add(attachment.Name);
                    }
                }
                foreach (var item in chapterUploads)
                {
                    if (item.Upload != null && !item.isSignOff)
                    {
                        attachments.Add(item.Upload.Name);
                    }
                    else
                    {
                        signatureURL = item.Upload.Name;
                    }
                }

                var status = (AcrobatFieldChapterUserResult != null && AcrobatFieldChapterUserResult.IsChecked) ? "Yes" : "No";
                CreateTableDataRow(grid,
                    DocumentType.AcrobatField,
                    chapter.Title,
                    string.Empty,
                    string.Empty,
                    string.Join(",", attachments),
                   AcrobatFieldChapterUserResult != null ? AcrobatFieldChapterUserResult.IssueDiscription : string.Empty,
                    signatureURL,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                 customDocument.CreatedOn
                    );
            }
            #endregion

            #region ["Policy"]

            foreach (var chapter in customDocument.PolicyContentModels)
            {
                var PolicyChapterUserResult = new PolicyContentBoxUserResult();
                var chapterUploads = new List<PolicyContentBoxUserUploadResult>();
                if (assignDoc != null)
                {
                    PolicyChapterUserResult = _PolicyContentBoxUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.AssignedDocumentId == assignDoc.Id && c.PolicyContentBoxId == chapter.Id && !c.IsGlobalAccessed);
                    chapterUploads = _PolicyContentBoxUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.AssignedDocumentId == assignDoc.Id && c.PolicyContentBoxId == chapter.Id && !c.IsGlobalAccessed).ToList();
                }
                else
                {
                    PolicyChapterUserResult = _PolicyContentBoxUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).FirstOrDefault(c => c.DocumentId == chapter.Id && c.UserId == userId.ToString() && c.IsGlobalAccessed && c.PolicyContentBoxId == chapter.Id);
                    chapterUploads = _PolicyContentBoxUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.DocumentId == chapter.Id && c.UserId == userId.ToString() && c.IsGlobalAccessed && c.PolicyContentBoxId == chapter.Id).ToList();
                }
                var signatureURL = string.Empty;
                var attachments = new List<string>();
                if (chapter.Attachments.Any())
                {
                    foreach (var attachment in chapter.Attachments)
                    {
                        attachments.Add(attachment.Name);
                    }
                }
                foreach (var item in chapterUploads)
                {
                    if (item.Upload != null && !item.isSignOff)
                    {
                        attachments.Add(item.Upload.Name);
                    }
                    else
                    {
                        signatureURL = item.Upload.Name;
                    }
                }


                CreateTableDataRow(grid,
                    "Call to Action",
                    chapter.Title,
                    string.Empty,
                    string.Empty,
                    string.Join(",", attachments),
                   PolicyChapterUserResult != null ? PolicyChapterUserResult.IssueDiscription : string.Empty,
                    signatureURL,
                    string.Empty,
                    string.Empty,
                   PolicyChapterUserResult != null ? PolicyChapterUserResult.IsActionNeeded : string.Empty,
                 customDocument.CreatedOn
                    );
            }
            #endregion

            #region ["Form Fields"]

            foreach (var chapter in customDocument.FormContentModels)
            {
                foreach (var frm in chapter.FormFields)
                {



                    CreateTableDataRow(grid,
                        DocumentType.Form,
                        chapter.Title,
                       frm.FormFieldName,
                        frm.FormFieldDescription,
                        string.Empty,
                       string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                     customDocument.CreatedOn
                        );
                }
            }
            #endregion


            section.AddElement(grid);
        }
        private void CreateChecklistTrackedTables(string checklistId, List<CheckListChapter> checkListChapter, AssignedDocument assignDoc, ReportSection section)
        {
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

            foreach (var chapter in checkListChapter.OrderBy(c => c.Number))
            {

                var checkListChapterUserResult = new CheckListChapterUserResult();
                var chapterUploads = new List<CheckListChapterUserUploadResult>();

                if (assignDoc != null)
                {
                    checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.AssignedDocumentId == assignDoc.Id && c.CheckListChapterId == chapter.Id && !c.IsGlobalAccessed).FirstOrDefault();
                    chapterUploads = _checkListChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.AssignedDocumentId == assignDoc.Id && c.CheckListChapterId == chapter.Id && !c.IsGlobalAccessed).ToList();
                }
                else
                {
                    checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.DocumentId == checklistId && c.UserId == userId.ToString() && c.CheckListChapterId == chapter.Id && c.IsGlobalAccessed).FirstOrDefault();
                    chapterUploads = _checkListChapterUserUploadResultRepository.GetAll().OrderBy(c => c.CreatedDate).Where(c => c.DocumentId == checklistId && c.UserId == userId.ToString() && c.CheckListChapterId == chapter.Id && c.IsGlobalAccessed).ToList();
                }

                var attachments = new List<string>();
                foreach (var item in chapterUploads)
                {
                    if (item.Upload != null && !item.isSignOff)
                    {
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
        private void CreatePersonalDetails(string checkListId, AssignedDocument assignedDocument, ReportSection section)
        {
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
                user.Group = new CustomerGroup { Title = name };
            }




            //code region end

            var userDetail = Project.UserViewModelFrom(user);
            var checkListUserResult = new CheckListUserResult();
            if (assignedDocument != null)
            {
                checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == assignedDocument.Id && !c.IsGlobalAccessed).FirstOrDefault();
            }
            else
            {
                checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.DocumentId == checkListId && c.UserId == userId.ToString() && c.IsGlobalAccessed).FirstOrDefault();
            }

            var status = (checkListUserResult != null && checkListUserResult.Status) ? "Completed" : "Incomplete";

            var userDetailModel = new UserDetail()
            {
                Name = userDetail.FullName,
                Email = userDetail.EmailAddress,
                Status = status,
                DateCompleted = checkListUserResult != null ? checkListUserResult.SubmittedDate.ToShortDateString() : "",
                Group = user.Group.Title
            };
            if (assignedDocument != null)
            {
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


            foreach (var field in fields)
            {
                var row = new GridRowBlock();
                row.AddElement(new GridCellBlock(field.Item1,
                    new FontElementStyle(new Font(defaultFont.Font, FontStyle.Bold))));
                row.AddElement(new GridCellBlock(field.Item2(userDetailModel)));

                grid.AddElement(row);
            }

            section.AddElement(grid);
        }



        private ReportSection CreateUserSection(string checkListId, AssignedDocument assignedDocument)
        {
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

        private ReportSection CreateChapterSection(CheckListChapter checkListChapter, string assignDocId, int count)
        {
            var section = CreateSection("", PageOrientation.Portrait, false);
            var checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == assignDocId && c.CheckListChapterId == checkListChapter.Id).FirstOrDefault();
            if (checkListChapterUserResult != null && checkListChapterUserResult.IsChecked)
            {
                section.AddElement(CreateParagraph(count.ToString() + ". " + "[ X ]  " + checkListChapter.Title, LeftAligned));
            }
            else
            {
                section.AddElement(CreateParagraph(count.ToString() + ". " + "[ ]  " + checkListChapter.Title, LeftAligned));
            }
            section.AddElement(AddBreak(5));
            if (checkListChapterUserResult != null && !string.IsNullOrEmpty(checkListChapterUserResult.IssueDiscription))
            {
                section.AddElement(CreateParagraph("Additional Notes: " + checkListChapterUserResult.IssueDiscription, LeftAligned));

                //section.AddElement(CreateParagraph(checkListChapterUserResult.IssueDiscription, LeftAligned));
            }
            section.AddElement(AddBreak(5));

            section.AddElement(AddBreak(30));
            return section;
        }
        private void CreateChecklistChapterTables(List<CheckListChapter> checkListChapter, ReportSection section)
        {
            var grid = CreateGrid();
            var columns = new[]
            {
                new Tuple<string, int>("Check Item", 30),
                new Tuple<string, int>("Check Status", 40),
                new Tuple<string, int>("Comments", 40),
                new Tuple<string, int>("Attachments", 100)
            };

            CreateTableHeader(grid, columns);

            foreach (var chapter in checkListChapter.OrderBy(c => c.Number))
            {

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


}
