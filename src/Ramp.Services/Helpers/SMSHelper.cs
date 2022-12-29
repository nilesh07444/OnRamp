using Domain.Customer.Models;
using Domain.Models;
using System.Collections.Generic;

namespace Ramp.Services.Helpers
{
    public class SMSRecipient
    {
        public User User { get; set; }
        public string ReferenceNumber { get; set; }
        public StandardUser StandardUser { get; set; }
    }

    public class CompareSMSRecipients : IEqualityComparer<SMSRecipient>
    {
        public bool Equals(SMSRecipient x, SMSRecipient y)
        {
            return x.User.MobileNumber == y.User.MobileNumber;
        }

        public int GetHashCode(SMSRecipient codeh)
        {
            return codeh.User.MobileNumber.GetHashCode();
        }
    }
}