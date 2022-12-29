using Domain.Customer.Models;
using Domain.Customer.Models.Document;
using Domain.Enums;
using Ramp.Contracts.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel
{
    public class UserViewModel : IViewModel
    {
        public UserViewModel()
        {
            UserList = new List<UserModelShort>();
            GroupList = new List<GroupViewModelShort>();
            UserRoles = new List<UserRole>();
            Roles = new List<RoleViewModel>();
            CustomerRoles = new List<string>();
            RaceCodes = new List<RaceCodeViewModel>();
        }
		public string StartDate { get; set; }
		public string EndDate { get; set; }
		public string AdditionalNote { get; set; }
		public	string VirtualClassroom { get; set; }
	    public	string Department { get; set; }
		public bool IsConfirmEmail { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<SerializableSelectListItem> Users { get; set; }
        public IEnumerable<SerializableSelectListItem> UserRolesListDropDownForCustomerUser { get; set; }
        public IEnumerable<SelectListItem> DropDownForGroup { get; set; }
        public IList<UserModelShort> UserList { get; set; }
        public IList<GroupViewModelShort> GroupList { get; set; }
        public List<string> CustomerRoles { get; set; }
        public TestResultViewModel TestResult { get; set; }
        public Guid Id { get; set; }
        public Guid ParentUserId { get; set; }
        public CompanyViewModel Company { get; set; }
        public virtual Guid CompanyId { get; set; }

        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please select role")]
        public Guid SelectedCustomerUserRole { get; set; }

        [Required(ErrorMessage = "Please enter Name")]
        [MinLength(6)]
        public string FullName { get; set; }

        public Guid SelectedUser { get; set; }

        [Required(ErrorMessage = "Please select group")]
        public Guid? SelectedGroupId { get; set; }

        public string LastName { get; set; }

        //[Required(ErrorMessage = "Please enter email")]
        //[RegularExpression(".+\\@.+\\..+", ErrorMessage = "Please enter a valid email address")]
        //[Remote("DoesEmailAlreadyPresent", "UserMgmt", "UserManagement",
        //    ErrorMessage = "This email address is already in use in the OnRAMP system, please enter a unique email address")]
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

        [RegularExpression("^[0-9]*$", ErrorMessage = "Please enter a valid Contact number")]
        public string ContactNumber { get; set; }

        public bool Status { get; set; }

        public HttpPostedFileBase UserCVSFile { get; set; }

        public List<RoleViewModel> Roles { get; set; }

        public int UserLoginFrequecy { get; set; }

        public double AverageUserRating { get; set; }

        public String TestName { get; set; }

        public DateTime? TestExpiryDate { get; set; }

        public Guid TestId { get; set; }

        public List<UserRole> UserRoles { get; set; }

        public string RoleName { get; set; }
		public string GroupName { get; set; } = "Default";

		public string SignupType => !IsFromSelfSignUp ? "Other": "Self-signup";

		public bool IsChangePasswordFirstLogin { get; set; }
        public bool IsSendWelcomeSMS { get; set; }

        public DateTime? CreatedOn { get; set; }
        public bool IsUserExpire { get; set; }

        [Required(ErrorMessage = "Please enter Expiry Days")]
        public int? ExpireDays { get; set; }

        public bool IsFromSelfSignUp { get; set; }
        public string EmployeeNo { get; set; }

        [DisplayName(Role.StandardUserDesc)]
        public bool StandardUser { get; set; }

        [DisplayName(Role.CategoryAdminDesc)]
        public bool CategoryAdmin { get; set; }

        [DisplayName(Role.ContentAdminDesc)]
        public bool ContentAdmin { get; set; }

        [DisplayName(Role.PublisherDesc)]
        public bool Publisher { get; set; }

        [DisplayName(Role.ReporterDesc)]
        public bool Reporter { get; set; }

        [DisplayName(Role.UserAdminDesc)]
        public bool UserAdmin { get; set; }

        [DisplayName(Role.PortalAdminDesc)]
        public bool PortalAdmin { get; set; }

        [DisplayName(Role.CustomerAdminDesc)]
        public bool CustomerAdmin { get; set; }

        [DisplayName(Role.NotificationAdminDesc)]
        public bool NotificationAdmin { get; set; }
        [DisplayName(Role.TrainingActivityAdminDesc)]
        public bool TrainingActivityAdmin { get; set; }
        [DisplayName(Role.TrainingActivityReporterDesc)]
        public bool TrainingActivityReporter { get; set; }
		[DisplayName(Role.ManageTagsDesc)]
        public bool ManageTags { get; set; }
		[DisplayName(Role.ManageVirtualMeetingsDesc)]
        public bool ManageAutoWorkflow { get; set; }
        [DisplayName(Role.ManageAutoWorkflowDesc)]
        public bool ManageReportSchedule { get; set; }
        [DisplayName(Role.ManageReportScheduleDesc)]
        public bool ManageVirtualMeetings { get; set; }
		[DisplayName(Role.ManageActivityLog)]
        public bool ManageActivityLog { get; set; }

        public string CompanyName { get; set; }
        public string SelectedCustomerType { get; set; }
        public List<SelectListItem> CustomerTypesSelectList { get; set; }
		public List<SelectListItem> CustomUserRoleSelectList { get; set; }

		public Guid? SelectedCustomUserRole { get; set; }

		public string IDNumber { get; set; }

        [Required]
        public string Gender { get; set; }

        public IEnumerable<SelectListItem> GenderDropDown { get; set; }
        public Guid? RaceCodeId { get; set; }
        public List<RaceCodeViewModel> RaceCodes { get; set; }
        public IEnumerable<UserDisclaimerActivityLogEntryModelShort> UserDisclaimerActivity { get; set; } = new List<UserDisclaimerActivityLogEntryModelShort>();
		[Required]
		public List<string> SelectedGroups { get; set; }
		public List<string> SelectedLabel { get; set; }
		public string TrainingLabels { get; set; } = string.Empty;
		public Guid? CustomUserRoleId { get; set; }
	}

	public class UserModelShort
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public Guid? GroupId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
		public string MobileNumber { get; set; }
        public bool IsActive { get; set; }
		public List<AssignedDocumentInfo> AssignedDocumentUsers { get; set; } = new List<AssignedDocumentInfo>();
		public Guid? CustomUserRoleId { get; set; }
		
	}
}