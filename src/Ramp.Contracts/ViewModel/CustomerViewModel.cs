using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ramp.Contracts.ViewModel
{
    public class CustomerViewModel
    {
        public CustomerViewModel()
        {
            CustomerViewModelList = new List<CustomerViewModelShort>();
        }

        public List<CustomerViewModelShort> CustomerViewModelList { get; set; }

        public CustomerViewModelShort CustomerViewModelShort { get; set; }
    }

    public class CustomerViewModelShort
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Please enter CompanyName")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Please enter PhysicalAddress")]
        public string PhysicalAddress { get; set; }

        [Required(ErrorMessage = "Please enter PostalAddress")]
        public string PostalAddress { get; set; }

        [Required(ErrorMessage = "Please enter TelephoneNumber")]
        public string TelephoneNumber { get; set; }

        [Required(ErrorMessage = "Please enter WebsiteAddress")]
        public string WebsiteAddress { get; set; }

        [Required(ErrorMessage = "Please enter LayerSubDomain")]
        public string LayerSubDomain { get; set; }

        [Required(ErrorMessage = "Please choose ProvisionalAccountLink")]
        public string SelectedProvisionalAccountLink { get; set; }

        public IEnumerable<SerializableSelectListItem> ProvisionalAccountLinks { get; set; }
    }
}