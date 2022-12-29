using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;
namespace Ramp.Contracts.CommandParameter.TestManagement.Question
{
    public class UpdateTrainingQuestionCommand: ICommand
    {
        public Guid? TrainingTestId { get; set; }
        public TrainingTestQuestionViewModel Model { get; set; }
    }
}
