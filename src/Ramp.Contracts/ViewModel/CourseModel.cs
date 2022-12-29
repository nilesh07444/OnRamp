using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{

    public class CourseViewModel
    {

        public CourseViewModel()
        {
            CourseList = new CourseListModel();
            Course = new CourseModel();
        }
        public CourseListModel CourseList { get; set; }
        public CourseModel Course { get; set; }
    }

    public class CourseListModel : List<CourseModel>
    {
    }

    public class CourseModel
    {

        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CoverPicture { get; set; }
        public bool IsGlobalEnabled { get; set; }
        public bool IsCourseExpiryEnabled { get; set; }
        public int ExpiryInDays { get; set; }
        public string AchievementType { get; set; }
        public bool WorkflowEnabled { get; set; }

        public string AllocatedAdmins { get; set; }
        public string AllocatedAdminsName { get; set; }

        public string CategoryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string[] AssignedUsers { get; set; }
        public List<NewDocument> Documents { get; set; }

        public DocumentStatus CourseStatus { get; set; }

        public UserCourse CreatedBy { get; set; }
        public string Status { get; set; }

        public int Points { get; set; }
        public string Achievements { get; set; }
    public List<AssignedDocumentListModel> AssignedDocumentList { get; set; }
        public object CoverPictureUpload { get; set; }

        public string CertificateId { get; set; }
        public string CertificateThumbnailId { get; set; }
        public UploadResultViewModel Certificate { get; set; }
    }

    public class NewDocument
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public string DocId { get; set; }
        public DocumentType Type { get; set; }
        public int OrderNo { get; set; } 
        public DateTime AssignedDate { get; set; }
    }

    public class UserCourse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class courseFilter
    {
        public string Status { get; set; }
        public string searchText { get; set; }
    }

}
