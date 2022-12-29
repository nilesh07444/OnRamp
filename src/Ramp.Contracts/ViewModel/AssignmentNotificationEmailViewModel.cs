using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class AssignmentNotificationViewModel
    {
        public string ReferenceId { get; set; }
        public string CustomerUserFirstName { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public Guid UserID { get; set; }
        public Guid Id { get; set; }
    }
}