using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CompanyType = Domain.Enums.CompanyType;

namespace Ramp.Contracts.ViewModel
{
    public class CustomerSelfProvisionViewModel : IViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Please enter System name")]
        //[Remote("DoesClientSystemNameAlreadyPresent", "ProvisionalMgmt", "ProvisionalManagement", ErrorMessage = "Client System Name already exists")]
        [MaxLength(50)]
        public string ClientSystemName { get; set; }

        public HttpPostedFileBase CompanyLogo { get; set; }

        [Required(ErrorMessage = "Please enter Company name")]
        //[Remote("DoesCompanyNameAlreadyPresent", "ProvisionalMgmt", "ProvisionalManagement", ErrorMessage = "Company Name already exist")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Please enter Physical Address")]
        public string PhysicalAddress { get; set; }

        [Required(ErrorMessage = "Please enter Postal Address")]
        public string PostalAddress { get; set; }

        [Required(ErrorMessage = "Please enter Telephone Number")]
        [RegularExpression("^[0-9]*", ErrorMessage = "Please enter a valid telephone number")]
        public string TelephoneNumber { get; set; }

        [Required(ErrorMessage = "Please enter Website Address")]
        [RegularExpression(@"^((http(s?):\/\/[a-zA-Z]+\.)|[a-zA-Z]+\.)[a-zA-Z0-9]+\.[a-zA-Z]{2,5}$", ErrorMessage = "Please enter a valid Url")]
        public string WebsiteAddress { get; set; }

        [Required(ErrorMessage = "Please enter Layer Sub-Domain")]
        [Remote("DoesLayerSubDomainAlreadyExistSelfProvition", "ProvisionalMgmt", "ProvisionalManagement", ErrorMessage = "Layer Sub-Domain Name already exists")]
        public string LayerSubDomain { get; set; }

        public string LogoImageUrl { get; set; }

        // user details in model
        [Required(ErrorMessage = "Please enter Name")]
        [MinLength(6)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        [Remote("DoesEmailAlreadyPresentSelfProvision", "ProvisionalMgmt", "ProvisionalManagement",
            ErrorMessage = "This email address is already in use in the OnRAMP system, please enter a unique email address")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please enter confirm password")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password",
            ErrorMessage = "The password and confirm password do not match.")]
        [Display(Name = "ConfirmPassword")]
        public string ConfirmPassword { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "Please enter a valid mobile number")]
        public string MobileNumber { get; set; }

        public string IDNumber { get; set; }

        public string CompanyConnectionString { get; set; }
        public string Gender { get; set; }
        public IEnumerable<SelectListItem> GenderDropDown { get; set; }
    }
}