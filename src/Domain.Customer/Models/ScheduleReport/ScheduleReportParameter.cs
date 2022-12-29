using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.ScheduleReport
{
    public class ScheduleReportParameter 
    {
        public Guid Id { get; set; }

        public Guid ReportID { get; set; }
        public string ParameterType { get; set; }

        public string ParameterID { get; set; }

    }
}
