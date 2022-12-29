using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Ramp.Contracts.CommandParameter.Login
{
    public class LoginUserCommandParameter : ICommand
    {
        [Required(ErrorMessage = "Please enter Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string companyId { get; set; }
        public IEnumerable<SelectListItem> Companies { get; set; }
        public string SelectedCompany { get; set; }
        public PortalContextViewModel PortalContext { get; set; }
    }
}