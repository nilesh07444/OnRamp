using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement.Answer
{
   public class DeleteTestAnswerCommand : ICommand
    {
        public TestAnswerViewModel Model { get; set; }
    }
}
