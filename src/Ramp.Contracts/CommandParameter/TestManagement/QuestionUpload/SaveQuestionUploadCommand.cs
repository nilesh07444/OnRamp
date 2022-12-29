using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.CommandParameter.TestManagement.QuestionUpload
{
    public class SaveQuestionUploadCommand : ICommand
    {
        public  FileUploadResultViewModel Model { get; set; }
        public Guid QuestionId { get; set; }
        public QuestionUploadType Type { get; set; }
    }
    public enum QuestionUploadType
    {
        Image,Video,Audio
    }
}
