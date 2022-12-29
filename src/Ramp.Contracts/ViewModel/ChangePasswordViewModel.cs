using System;
using System.ComponentModel.DataAnnotations;

namespace Ramp.Contracts.ViewModel
{
    public class ChangePasswordViewModel
    {
        public Guid UserId  { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please enter confirm password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirm password do not match.")]
        [Display(Name = "ConfirmPassword")]
        public string ConfirmPassword { get; set; }


    }
}
