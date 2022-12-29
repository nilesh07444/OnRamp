using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.ScheduleReport
{
   public class DeleteScheduleReportCommand : ICommand
    {
      public Guid Id { get; set; }

       
    }
}
