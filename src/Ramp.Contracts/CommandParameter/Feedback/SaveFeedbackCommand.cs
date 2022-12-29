using Common.Command;
using Domain.Customer.Models.Feedback;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Feedback
{
    public class SaveFeedbackCommand : 
        ICommand
    {
        [Required]
        public FeedbackType Type { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Option { get; set; }
        [Required]
        public string Message { get; set; }
        public Guid UserId { get; set; }
    }
}
