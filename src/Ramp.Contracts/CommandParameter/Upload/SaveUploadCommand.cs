using Common.Command;
using Domain.Customer.Models;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ramp.Contracts.CommandParameter.Upload
{
    public class SaveUploadCommand : ICommand
    {
        public UploadModel FileUploadV { get; set; }
        public bool? MainContext { get; set; }
    }

    public class SaveFileUploadCommand : ICommand
    {
        public FileUploadViewModel FileUploadV { get; set; }
        public bool? MainContext { get; set; }
    }
}