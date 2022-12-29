using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.TrainingGuide
{
    public class TrainingGuideListQuery : ICommand
    {
        public bool OnlyActive { get; set; }
        public Guid? CollaboratorId { get; set; }
    }
}
