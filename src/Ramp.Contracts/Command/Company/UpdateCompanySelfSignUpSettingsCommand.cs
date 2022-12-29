using System;

namespace Ramp.Contracts.Command.Company {
	public class UpdateCompanySelfSignUpSettingsCommand
    {
        public Guid Id { get; set; }
        public bool IsForSelfSignUp { get; set; }
        public bool IsSelfSignUpApprove { get; set; }
		public bool IsEmployeeCodeReq { get; set; }
		public bool IsEnabledEmployeeCode { get; set; }
		public string CompanySiteTitle { get; set; }

		public bool ActiveDirectoryEnabled { get; set; }
		public string Domain { get; set; }
		public string Port { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
	}
}
