using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class CustomConfigurationViewModel
    {
        public FileUploadViewModel Certificate { get; set; }
        public FileUploadViewModel CSS { get; set; }
        public FileUploadViewModel LoginLogo { get; set; }
        public FileUploadViewModel DashboardLogo { get; set; }
		public FileUploadViewModel NotificationHeaderLogo { get; set; }
        public FileUploadViewModel NotificationFooterLogo { get; set; }
        public IList<FileUploadViewModel> Trophies { get; set; }
    }
}