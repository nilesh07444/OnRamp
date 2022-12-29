using Domain.Customer.Models;
using Domain.Models;
using System;

namespace Ramp.Contracts.ViewModel
{
    public class UserCorrespondenceLogViewModel
    {
        public Guid CorrespondenceId { get; set; }
        public UserCorrespondenceEnum CorrespondenceType { get; set; }
        public String Description { get; set; }
        public Guid UserId { get; set; }
        public UserViewModel UserViewModel { get; set; }
        public DateTime CorrespondenceDate { get; set; }
        public string MessageContent { get; set; }
    }
}