using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.ScheduleReport
{
    public class  ScheduleReportModel 
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
    }
}
