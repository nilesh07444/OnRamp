using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement.QuestionUpload
{
    public class DeleteQuestionUploadCommand : ICommand
    {
        public Guid? QuestionUploadId { get; set; }
        public QuestionUploadType Type { get; set; }
    }
}
