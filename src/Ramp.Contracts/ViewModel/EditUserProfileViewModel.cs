using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ramp.Contracts.ViewModel
{
    public class EditUserProfileViewModel
    {
        public EditUserProfileViewModel()
        {
            GroupList = new List<GroupViewModelShort>();
        }

        public IEnumerable<SerializableSelectListItem> DropDownForGroup { get; set; }

        public IList<GroupViewModelShort> GroupList { get; set; }

        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter email")]
        [RegularExpression(".+\\@.+\\..+", ErrorMessage = "Please enter a valid email address")]
        public string EmailAddress { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [RegularExpression("^[0-9]*", ErrorMessage = "Please enter a valid mobile number")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Please select group")]
        public Guid SelectedGroup { get; set; }

        public string EmployeeNo { get; set; }
        public string IDNumber { get; set; }
    }

    public class EditUserProfileModelShort
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}