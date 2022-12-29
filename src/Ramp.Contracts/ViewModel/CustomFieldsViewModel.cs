using Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
  public class CustomFieldsViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string FieldName { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateEdited { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}
