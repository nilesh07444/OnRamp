using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
  public class ChangeCompanyUserExpiryStatusCommandParameter : ICommand
    {

        public Guid UserId { get; set; }
        public bool IsExpiryStatus { get; set; }
		public bool IsSelfSignUp { get; set; }
	}
}
