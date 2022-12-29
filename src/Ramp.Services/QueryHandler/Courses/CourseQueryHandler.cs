using Common.Data;
using Common.Query;
using Domain.Customer;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.PolicyResponse;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Domain.Customer.Models;
using Ramp.Contracts.Query.Course;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Ramp.Services.Projection;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models.Document;
using Data.EF.Customer;
using Domain.Customer.Models.CheckLists;
using Common;

namespace Ramp.Services.QueryHandler.Courses
{
    public class CourseQueryHandler :
        IQueryHandler<FetchByIdQuery, CourseModel>,
        IQueryHandler<CourseListQuery, IEnumerable<CourseModel>>,
        IQueryHandler<FetchTotalDocumentsQuery<Course>, int>
    {

        private readonly IRepository<StandardUser> _standardUsers;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<AssignedCourse> _assignedCourseRepository;
        private readonly IRepository<AssociatedDocument> _associatedDocumentRepository;
        private readonly IRepository<Upload> _uploadRepository;
        private readonly ITransientRepository<Memo> _memoRepository;
        private readonly ITransientRepository<Policy> _policyRepository;
        private readonly ITransientRepository<Test> _testRepository;
        private readonly ITransientRepository<TrainingManual> _trainingManualRepository;
        private readonly ITransientRepository<StandardUser> _userRepository;
        private readonly ITransientRepository<Test_Result> _testResultRepository;
        private readonly ITransientRepository<PolicyResponse> _policyResponseRepository;
        private readonly ITransientRepository<TestSession> _testSessionRepository;
        private readonly ITransientRepository<CheckList> _checkListRepository;

        public CourseQueryHandler(
            IRepository<AssociatedDocument> associatedDocumentRepository,
            IRepository<AssignedCourse> assignedCourseRepository,
            IRepository<Course> courseRepository,
            IRepository<StandardUser> standardUsers, IRepository<Upload> upload,
            ITransientRepository<Memo> memoRepository,
            ITransientRepository<Policy> policyRepository,
            ITransientRepository<Test> testRepository,
            ITransientRepository<TrainingManual> trainingManualRepository,
            ITransientRepository<StandardUser> userRepository,
            ITransientRepository<Test_Result> testResultRepository,
             ITransientRepository<PolicyResponse> policyResponseRepository,
             ITransientRepository<TestSession> testSessionRepository,
             ITransientRepository<CheckList> checkListRepository
            )
        {
            _courseRepository = courseRepository;
            _associatedDocumentRepository = associatedDocumentRepository;
            _assignedCourseRepository = assignedCourseRepository;
            _standardUsers = standardUsers;
            _uploadRepository = upload;
            _memoRepository = memoRepository;
            _policyRepository = policyRepository;
            _testRepository = testRepository;
            _trainingManualRepository = trainingManualRepository;
            _userRepository = userRepository;
            _testResultRepository = testResultRepository;
            _policyResponseRepository = policyResponseRepository;
            _testSessionRepository = testSessionRepository;
            _checkListRepository = checkListRepository;
        }


        public IEnumerable<CourseModel> ExecuteQuery(CourseListQuery query)

        {

            //AllocatedAdmins = c.AllocatedAdmins.Split(',').Select(Guid.Parse).Select(r => new UserCourse
            //{
            //	Id = r.ToString(),
            //	Name = _standardUsers.Find(r).FirstName + " " + _standardUsers.Find(r).LastName,
            //	Email = _standardUsers.Find(r).EmailAddress
            //}).ToList()
            try
            {

                var course = _courseRepository.List.Where(x => !x.IsDeleted).ToList().Select(c => new CourseModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    CertificateId = c.AchievementId.ToString(),
                    CoverPicture = c.CoverPicture,
                    IsGlobalEnabled = c.GlobalAccessEnabled,
                    IsCourseExpiryEnabled = c.ExpiryEnabled,
                    ExpiryInDays = (c.EndDate.Subtract(c.StartDate)).Days,
                    AchievementType = c.AchievementId.ToString(),
                    // Certificate = x.Certificate != null ? Project.Certificate_UploadResultViewModel.Invoke(c.Certificate) : null,
                    WorkflowEnabled = c.WorkflowEnabled,
                    AllocatedAdminsName = String.Join(",", c.AllocatedAdmins.Split(',').Select(Guid.Parse).Select(r => new UserCourse
                    {
                        Name = _standardUsers.Find(r).FirstName + " " + _standardUsers.Find(r).LastName
                    }).Select(m => m.Name).ToList()),
                    AllocatedAdmins = c.AllocatedAdmins,
                    CategoryId = c.CategoryId.ToString(),
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Documents = c.Documents.Select(r => new NewDocument { Title = r.Title, Id = r.Id.ToString(), DocId = r.DocumentId.ToString(), Type = r.Type, OrderNo = r.OrderNo }).ToList(),
                    AssignedDocumentList = GetAssignedDocument(c.Documents.Select(r => new NewDocument { Title = r.Title, Id = r.Id.ToString(), DocId = r.DocumentId.ToString(), Type = r.Type, OrderNo = r.OrderNo }).ToList(), query.userID),
                    CourseStatus = c.Status,
                    CreatedBy = new UserCourse
                    {
                        Id = c.CreatedBy.ToString(),
                        Name = _standardUsers.Find(c.CreatedBy).FirstName + " " + _standardUsers.Find(c.CreatedBy).LastName,
                        Email = _standardUsers.Find(c.CreatedBy).EmailAddress
                    },
                    Points = c.Points,
                    Status = Convert.ToString((DocumentStatus)c.Status),

                });

                return course.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        private List<AssignedDocumentListModel> GetAssignedDocument(List<NewDocument> query, string userid)
        {
            var policyResponseSource = _policyResponseRepository.GetAll().ToList();
            //   var checkListChaptersSource = _checkListChapterRepository.GetAll().ToList();

            var memoSource = _memoRepository.List.ToList();
            var policySource = _policyRepository.List.ToList();
            var testSource = _testRepository.List.ToList();
            var testResultSource = _testResultRepository.List.ToList();
            var trainingManualSource = _trainingManualRepository.List.ToList();
            var testSessionSource = _testSessionRepository.GetAll().ToList();
            var checklistSource = _checkListRepository.List.ToList();
            // var checklistUserResultSource = _checkListUserResultRepository.GetAll().ToList();

            //neeraj changes >= from <= in below assigneddate condition by neeraj alert it back before delivering

            CustomerContext _context = new CustomerContext();
            var docID = query.ToString();



            var results = new List<AssignedDocumentListModel>();

            foreach (var q in query)
            {
                var assignedDocuments =
                           _context.AssignedDocuments

                           .Where(x => x.DocumentId == q.DocId && x.UserId == userid && x.Deleted == false)
                           .OrderByDescending(x => x.AssignedDate)
                           .ThenBy(x => x.OrderNumber).ToList();
                foreach (var assignedDocument in assignedDocuments)
                {
                    var result = new AssignedDocumentListModel
                    {
                        AssignedDocumentId = assignedDocument.Id,
                        Id = assignedDocument.DocumentId,
                        DocumentType = assignedDocument.DocumentType,
                        //AssignedDate = assignedDocument.AssignedDate.ToLocalTime()
                        AssignedDate = assignedDocument.AssignedDate
                    };

                    var du = _context.DocumentUsages
                        .Where(x => x.DocumentId == assignedDocument.DocumentId &&
                                    x.DocumentType == assignedDocument.DocumentType &&
                                     !x.IsGlobalAccessed)
                        .OrderByDescending(x => x.ViewDate)
                        .FirstOrDefault();                    

                    var lastViewed = du?.ViewDate.ToLocalTime();
                    result.LastViewedDate = lastViewed;


                    result.DocStatus = lastViewed == null ? AssignedDocumentStatus.Pending.ToString() : AssignedDocumentStatus.UnderReview.ToString();
                    result.Status = lastViewed == null ? AssignedDocumentStatus.Pending : AssignedDocumentStatus.UnderReview;

                    if (lastViewed.HasValue)
                    {
                        var test = DateTime.Compare(lastViewed.Value, result.AssignedDate);
                        if (test == -1)
                        {
                            result.Status= AssignedDocumentStatus.Pending;
                            result.DocStatus = AssignedDocumentStatus.Pending.ToString();
                            result.LastViewedDate = null;
                        }
                        else if (test == 1)
                        {
                            //result.Status = AssignedDocumentStatus.InProgress;
                        }
                    }


                    if (assignedDocument.DocumentType == DocumentType.TrainingManual || assignedDocument.DocumentType == DocumentType.Memo 
                        || assignedDocument.DocumentType == DocumentType.Policy || assignedDocument.DocumentType == DocumentType.Checklist )
                    {

                        if (lastViewed == null)
                        {
                            result.Status = AssignedDocumentStatus.Pending;
                            result.DocStatus = AssignedDocumentStatus.Pending.ToString();
                        }
                        else if (lastViewed != null && du.Status == null)
                        {
                            result.Status = AssignedDocumentStatus.Pending;
                            result.DocStatus = AssignedDocumentStatus.Pending.ToString();
                        }
                        else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.UnderReview)
                        {
                            result.Status = AssignedDocumentStatus.UnderReview;
                            result.DocStatus = AssignedDocumentStatus.UnderReview.ToString();
                        }
                        else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.Complete)
                        {
                            result.Status = AssignedDocumentStatus.Complete;
                            result.DocStatus = AssignedDocumentStatus.Complete.ToString();
                        }
                        else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.InProgress)
                        {
                            result.Status = AssignedDocumentStatus.InProgress;
                            result.DocStatus = AssignedDocumentStatus.InProgress.ToString();
                        }
                    }
                    if (assignedDocument.DocumentType == DocumentType.Policy)
                    {
                        var policyResponse = _context.PolicyResponses.Where(c => c.PolicyId == assignedDocument.DocumentId && c.UserId == assignedDocument.UserId && !c.IsGlobalAccessed).OrderByDescending(c => c.Created).FirstOrDefault();

                        if (lastViewed == null && policyResponse == null)
                        {
                            result.Status = AssignedDocumentStatus.Pending;
                            result.DocStatus = AssignedDocumentStatus.Pending.ToString();
                        }
                        else if (lastViewed != null && du.Status == null && policyResponse == null)
                        {
                            result.Status = AssignedDocumentStatus.Pending;
                            result.DocStatus = AssignedDocumentStatus.Pending.ToString();
                        }
                        else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.UnderReview)
                        {
                            result.Status = AssignedDocumentStatus.UnderReview;
                            result.DocStatus = AssignedDocumentStatus.UnderReview.ToString();
                        }
                        else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.Complete)
                        {
                            result.Status = AssignedDocumentStatus.Complete;
                            result.DocStatus = AssignedDocumentStatus.Complete.ToString();
                        }
                        else if (lastViewed.HasValue && du.Status.HasValue && (int)du.Status == (int)DocumentUsageStatus.InProgress)
                        {
                            result.Status = AssignedDocumentStatus.InProgress;
                            result.DocStatus = AssignedDocumentStatus.InProgress.ToString();
                        }
                        else if (policyResponse != null && policyResponse.Response.HasValue)
                        {
                            result.Status = AssignedDocumentStatus.Complete;
                            result.DocStatus = AssignedDocumentStatus.Complete.ToString();
                        }

                    }

                    IDocument doc = null;

                    switch (assignedDocument.DocumentType)
                    {
                        case DocumentType.Memo:
                            doc = memoSource.Where(x => x.Id == assignedDocument.DocumentId).FirstOrDefault();
                            break;
                        case DocumentType.Policy:
                            doc = _context.Policies.Where(x => x.Id == assignedDocument.DocumentId).FirstOrDefault();

                            var policyResponse = _context.PolicyResponses.Where(c => c.PolicyId == assignedDocument.DocumentId && c.UserId == assignedDocument.UserId && !c.IsGlobalAccessed).OrderByDescending(c => c.Created).FirstOrDefault();

                            break;
                        case DocumentType.Test:
                            var test = _context.Tests.Where(x => x.Id == assignedDocument.DocumentId).FirstOrDefault();
                            if (test != null)
                            {
                                doc = test;
                                if (test.TestExpiresNumberDaysFromAssignment.HasValue)
                                {
                                    result.ExpiryDate =
                                        assignedDocument.AssignedDate.AddDays(
                                            test.TestExpiresNumberDaysFromAssignment.Value).ToLocalTime();
                                }
                                var assignedDate = assignedDocument.AssignedDate;

                                //neeraj put x.Created.UtcDateTime > assignedDate after x.TestId == assignedDocument.DocumentId inside below where statement iside linq
                                var testResults =
                                    _context.Test_Results.Where(x => !x.Deleted && x.TestId == assignedDocument.DocumentId && !x.IsGloballyAccessed).ToList();

                                if (testResults.Any())
                                {
                                    result.CertificateUrl = testResults.OrderByDescending(x => x.Score).ToList()[0].Certificate?.Id;

                                    result.LastViewedDate =
                                        testResults.OrderByDescending(x => x.Created.DateTime).ToList()[0].Created.LocalDateTime;

                                    result.Status = testResults.Any(r => r.Passed)
                                        ? AssignedDocumentStatus.Passed
                                        : AssignedDocumentStatus.ActionRequired;
                                    result.DocStatus = testResults.Any(r => r.Passed)
                                        ? AssignedDocumentStatus.Passed.ToString()
                                        : AssignedDocumentStatus.ActionRequired.ToString();
                                }
                                var testSession = _context.TestSessions.Where(c => c.UserId == assignedDocument.UserId && c.CurrentTestId == assignedDocument.DocumentId && !c.IsGlobalAccessed).FirstOrDefault();
                                if (testSession != null)
                                {
                                    result.Status = AssignedDocumentStatus.UnderReview;
                                    result.DocStatus = AssignedDocumentStatus.UnderReview.ToString();
                                    result.LastViewedDate = testSession.StartTime.Value.ToLocalTime();
                                }
                                else
                                {
                                    //result.Status = result
                                }

                                result.PassMarks = test.PassMarks;
                                result.Duration = test.Duration;
                                result.AttemptsRemaining = test.MaximumAttempts - testResults.Count;
                                result.EmailSummary = test.EmailSummary;
                                result.HighlightAnswersOnSummary = test.HighlightAnswersOnSummary == null ? false : test.HighlightAnswersOnSummary;
                            }

                            break;
                        case DocumentType.TrainingManual:
                            doc = _context.TrainingManuals.Where(x => x.Id == assignedDocument.DocumentId).FirstOrDefault();
                            break;
                        case DocumentType.Checklist:

                            doc = _context.CheckLists.Where(x => x.Id == assignedDocument.DocumentId).FirstOrDefault();
                            if (!doc.Deleted)
                            {

                                //   var checkListChapters = _context.CheckListChapters.Where(c => c.CheckListId == assignedDocument.DocumentId).ToList();
                                //  var checkListUserResult = checklistUserResultSource.Where(c => c.AssignedDocumentId == assignedDocument.Id && !c.IsGlobalAccessed).OrderByDescending(c => c.SubmittedDate).FirstOrDefault();
                            }
                            break;
                    }

                    if (result.DocumentType != DocumentType.Checklist)
                    {
                        if (doc != null)
                        {
                            result.ReferenceId = doc.ReferenceId;
                            result.Title = doc.Title;
                            result.Description = doc.Description;
                            //result.CategoryId = doc.CategoryId;
                            result.Printable = doc.Printable;
                            var author = FindUser(doc.CreatedBy);
                            result.Author = author != null ? author.Name : "Unknown";
                            result.TrainingLabels = string.IsNullOrEmpty(doc.TrainingLabels) ? "none" : doc.TrainingLabels;
                        }

                        results.Add(result);
                    }
                    if (result.DocumentType == DocumentType.Checklist)
                    {
                        if (doc != null)
                        {
                            if (!doc.Deleted)
                            {
                                result.ReferenceId = doc.ReferenceId;
                                result.Title = doc.Title;
                                result.Description = doc.Description;
                                result.CategoryId = doc.Category.Id;
                                result.Printable = doc.Printable;
                                var author = FindUser(doc.CreatedBy);
                                result.Author = author != null ? author.Name : "Unknown";
                                result.TrainingLabels = string.IsNullOrEmpty(doc.TrainingLabels) ? "none" : doc.TrainingLabels;
                                results.Add(result);
                            }

                        }
                    }

                }

            }
            return results;
        }
        
        private UserModelShort FindUser(string id)
        {
            var gId = id.ConvertToGuid();

            if (gId.HasValue)
            {
                var user = _userRepository.Find(gId);
                if (user != null)
                {
                    var userRcd = new UserModelShort
                    {
                        Id = user.Id,
                        MobileNumber = user.MobileNumber,
                        UserName = user.FirstName + " " + user.LastName,
                        Name = !(user.FirstName == null || user.FirstName == "") && !(user.LastName == null || user.LastName == "") ? user.FirstName + " " + user.LastName : user.FirstName,
                        Email = user.EmailAddress
                    };

                    return userRcd;
                }
            }
            return null;
        }

        public CourseModel ExecuteQuery(FetchByIdQuery query)
        {
            var course = _courseRepository.List.Where(x => x.Id.ToString() == query.Id.ToString() && !x.IsDeleted).Select(c => new CourseModel
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                CertificateId = c.AchievementId.ToString(),
                CoverPicture = c.CoverPicture,
                IsGlobalEnabled = c.GlobalAccessEnabled,
                IsCourseExpiryEnabled = c.ExpiryEnabled,
                ExpiryInDays = c.ExpiryInDays,
                AchievementType = c.AchievementId.ToString(),
                Achievements = c.AchievementId != null ? _uploadRepository.Find(c.AchievementId).Name : null,
                WorkflowEnabled = c.WorkflowEnabled,
                AllocatedAdminsName = String.Join(",", c.AllocatedAdmins.Split(',').Select(Guid.Parse).Select(r => new UserCourse
                {
                    Name = _standardUsers.Find(r).FirstName + " " + _standardUsers.Find(r).LastName
                }).Select(m => m.Name).ToList()),
                AllocatedAdmins = c.AllocatedAdmins,
                CategoryId = c.CategoryId.ToString(),
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Documents = c.Documents.Select(r => new NewDocument { Title = r.Title, Id = r.Id.ToString(), DocId = r.DocumentId.ToString(), Type = r.Type, OrderNo = r.OrderNo }).ToList(),
                CourseStatus = c.Status,
                CreatedBy = new UserCourse
                {
                    Id = c.CreatedBy.ToString(),
                    Name = _standardUsers.Find(c.CreatedBy).FirstName + " " + _standardUsers.Find(c.CreatedBy).LastName,
                    Email = _standardUsers.Find(c.CreatedBy).EmailAddress
                },
                Points = c.Points,
                Status = Convert.ToString((DocumentStatus)c.Status)
            }).FirstOrDefault();

            return course;
        }

        public int ExecuteQuery(FetchTotalDocumentsQuery<Course> query)
        {
            throw new NotImplementedException();
        }


    }
}
