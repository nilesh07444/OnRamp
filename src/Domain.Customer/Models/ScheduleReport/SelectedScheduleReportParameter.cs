using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.ScheduleReport
{
    public class SelectedScheduleReportParameter 
    {
        public Guid ID { get; set; }

        public Guid ParameterID { get; set; }

        public string Selectedparameter { get; set; }
    }
}
