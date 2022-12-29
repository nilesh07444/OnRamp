using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ramp.Contracts;

namespace Web.UI.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required(ErrorMessage = "Please enter current password")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Please enter New Password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Required(ErrorMessage = "Please enter Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    /* public class LoginViewModel
     {
         [Required]
         [Display(Name = "User name")]
         public string UserName { get; set; }

         [Required]
         [DataType(DataType.Password)]
         [Display(Name = "Password")]
         public string Password { get; set; }

         [Display(Name = "Remember me?")]
         public bool RememberMe { get; set; }
     }*/

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    //public class EditUserProfileViewModel
    //{
    //    public IList<RoleViewModel> RoleListOld { get; set; }

    //    public IEnumerable<SerializableSelectListItem> Roles { get; set; }

    //    public IList<RoleModelShort> RoleList { get; set; }

    //    public Guid Id { get; set; }

    //    [Required]
    //    [Display(Name = "Full Name")]
    //    public string FullName { get; set; }

    //    [Required]
    //    [Display(Name = "Contact Number")]
    //    [RegularExpression("^[0-9]*", ErrorMessage = "Please enter numbers only")]
    //    public string ContactNumber { get; set; }

    //    [Required(ErrorMessage = "Please enter email")]
    //    [RegularExpression(".+\\@.+\\..+", ErrorMessage = "Please enter a valid email address")]
    //    public string EmailAddress { get; set; }

    //    [Required]
    //    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "Password")]
    //    public string Password { get; set; }

    //    [DataType(DataType.Password)]
    //    [Display(Name = "Confirm password")]
    //    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    //    public string ConfirmPassword { get; set; }

    //    [Required(ErrorMessage = "Please enter mobile number")]
    //    [RegularExpression("^[0-9]*", ErrorMessage = "Please enter a valid mobile number")]
    //    public string MobileNumber { get; set; }

    //}

    //public class RoleModelShort
    //{
    //    public Guid Id { get; set; }
    //    public string Name { get; set; }
    //}
}
