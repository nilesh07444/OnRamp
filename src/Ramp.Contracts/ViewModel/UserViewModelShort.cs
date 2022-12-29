using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel {
    public class UserViewModelShort {
        public string FullName { get; set; }
        public string GroupName { get; set; }
        public Guid GroupId { get; set; }
        public Guid Id { get; set; }
        public bool Selected { get; set; }
    }
}
