using Domain.Customer.Models.Feedback;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class FeedbackViewModel
    {
        public FeedbackType Type { get; set; }
        public string Subject { get; set; }
        public string Option { get; set; }
        [Required]
        public string Message { get; set; }
        public DateTime FeedbackDate { get; set; }
        public Guid UserId { get; set; }
        public UserViewModel User { get;set; }
        public UserViewModel Collaborator { get; set; }
        public CompanyViewModel CompanyViewModel { get; set; }
        public string SubjectName { get; set; }
        public ICollection<FeedbackReadViewModel> Reads { get; set; }
        public int? TestVersion { get; set; }
        public Guid Id { get; set; }
    }
}
