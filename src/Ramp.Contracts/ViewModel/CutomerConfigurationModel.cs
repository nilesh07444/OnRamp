using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class CustomerConfigurationModel
    {
        public FileUploadViewModel UploadModel { get; set; }
        public CustomerConfigurationType Type { get; set; }
        public bool? Deleted { get; set; }
        public int Version { get; set; }
        public CompanyModelShort Company { get; set; }
    }
}
