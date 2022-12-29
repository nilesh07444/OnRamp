using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.CommandParameter.Certificate
{
    public class CreateOrUpdateCertificateCommand : IdentityModel<string>
    {
        [Required]
        public UploadResultViewModel Upload { get; set; }
		public string Title { get; set; }
	}
}
