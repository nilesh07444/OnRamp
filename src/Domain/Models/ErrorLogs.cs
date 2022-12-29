using System;

namespace Domain.Models
{
    public class ErrorLogs : DomainObject
    {
        public Guid UserId { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDescription { get; set; }
        public DateTime ErrorDate { get; set; }

    }
}