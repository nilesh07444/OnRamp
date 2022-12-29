using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class CustomerConfiguration : DomainObject
    {
        public virtual Company Company { get; set; }
        public bool? Deleted { get; set; }
        public int Version { get; set; }
        public CustomerConfigurationType Type { get; set; }
        public virtual FileUpload Upload { get; set; }
        public string Description { get; set; }

    }
    public enum CustomerConfigurationType
    {
        Certificate,
        CSS,
        DashboardLogo,
        LoginLogo,
        Trophy,
        IconSet,
        FooterLogo,
		NotificationHeaderLogo,
		NotificationFooterLogo
	}
   
}
