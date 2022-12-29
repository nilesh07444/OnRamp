using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class CustomConfiguration : Base.CustomerDomainObject
    {
        public virtual FileUploads Upload { get; set; }
        public CustomType Type { get; set; }
        public int Version { get; set; }
        public virtual FileUploads CSS { get; set; }
        public virtual FileUploads Certificate { get; set; }
        public bool? Deleted { get; set; }
    }
    public enum CustomType
    {
        Certificate,
        CSS,
        DashboardLogo,
        LoginLogo,
		NotificationHeaderLogo,
		NotificationFooterLogo,
        Trophy
    }
}