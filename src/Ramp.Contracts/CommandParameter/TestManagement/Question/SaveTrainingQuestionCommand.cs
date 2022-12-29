using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement.Question
{
    public class SaveTrainingQuestionCommand : ICommand
    {
        public Guid? TestId { get; set; }
        public TrainingTestQuestionViewModel Model { get; set; }
    }
}
