using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class ResetPasswordViewModel
    {
        [Required]
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "New password and confirmation does not match.")]
        public string ConfirmPassword { get; set; }

        public string ReturnToken { get; set; }
    }
}