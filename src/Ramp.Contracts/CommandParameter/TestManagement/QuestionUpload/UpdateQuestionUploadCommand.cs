using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement.QuestionUpload
{
    public class UpdateQuestionUploadCommand : ICommand
    {
        public FileUploadResultViewModel Model { get; set; }
        public Guid? QuestionUploadId { get; set; }
    }
}
