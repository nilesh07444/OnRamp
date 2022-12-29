using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel
{
    public class GroupViewModel : IViewModel
    {
        public Guid GroupId { get; set; }

        [Required(ErrorMessage = "Please enter group name")]
        [Remote("DoesGroupNameAlreadyPresent", "ManageGroups", "Configurations", ErrorMessage = "Group name already exist")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please enter group description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please select customer company")]
        public Guid SelectedCustomerCompanyId { get; set; }

        [Required(ErrorMessage = "Please select provisional company")]
        public Guid SelectedProvisionalCompanyId { get; set; }

        public bool IsforSelfSignUpGroup { get; set; }

        public CompanyViewModel Company { get; set; }

		public Guid? ParentGroupId { get; set; }
	}

    public class GroupViewModelShort
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }
}