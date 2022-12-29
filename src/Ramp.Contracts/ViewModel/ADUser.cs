using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{


	public class ADUser
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string CreatedOn { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public List<string> Groups { get; set; }
		public string EmployeeNo { get; set; }
		public bool IsActive { get; set; }
		public string Department { get; set; }

	}

	public class ADGroups
	{
		public string Title { get; set; }
		public string Desription { get; set; }
		public string CreatedOn { get; set; }
	}

	public class Emailist
	{
		public string emails { get; set; }
	}



}
