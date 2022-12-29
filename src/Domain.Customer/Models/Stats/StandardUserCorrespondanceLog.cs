using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class StandardUserCorrespondanceLog : Base.CustomerDomainObject
    {
        public virtual StandardUserCorrespondenceEnum CorrespondenceType { get; set; }
        public string Description { get; set; }
        public virtual Guid UserId { get; set; }
        public virtual StandardUser User { get; set; }
        public virtual DateTime CorrespondenceDate { get; set; }
        public string Content { get; set; }
    }

    public enum StandardUserCorrespondenceEnum
    {
        [Description("received an email")]
        Email = 0,

        [Description("received an SMS")]
        Sms = 1,
    }
}