using Domain.Customer.Models.CustomRole;
using Domain.Customer.Models.ScheduleReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel
{
    public class ScheduleReportVM
    {
        public Guid? Id { get; set; }
        public string ScheduleName { get; set; }
        public string RecipientsList { get; set; }
        public string ReportAssignedId { get; set; }
        public string Occurences { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public DateTime? DateCreated { get; set; }
        public bool IsDeleted { get; set; }
        public bool Status { get; set; }
        public List<Parameters> Params { get; set; }
    }

    public class Parameters
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class scheduleReportStatusVM
    {
        public Guid? ReportId { get; set; }
        public bool Status { get; set; }
    }
    public class CustomScheduleReportViewModel
    {
        public string ScheduleName { get; set; }
        public string Recipients { get; set; }
        public IEnumerable<SerializableSelectListItem> TestDropDown { get; set; }
        public List<CustomUserRoles> CustomRoles { get; set; }
        public List<AutoAssignWorkflowViewModel> WorkflowList { get; set; }
        public List<GroupViewModel> Groups { get; set; }
        public IEnumerable<SerializableSelectListItem> Categories { get; set; }
        public IEnumerable<UserViewModel> Users { get; set; }
        public string PointReportData { get; set; }
        public IEnumerable<VirtualClassModel> VirtualMeetings { get; set; }
        public SelectList TrainingLabelList { get; set; }
          public SelectList   TrainingActivityTypeList { get; set; }
        public List<MyCheckList> ActivityCheckLists { get; set; }
        public ScheduleReportModel EditScheduleReport { get; set; }
    }

    public class MyCheckList
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public string Extra { get; set; }

    }
}
