using Common.Data;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.ExternalTrainingProvider
{
    public class CreateAndUpdateExternalTrainingProviderCommand : IdentityModel<string>
    {
        [Required(AllowEmptyStrings = false,ErrorMessage ="Name Is Required")]
        public string CompanyName { get; set; }
        [Required(AllowEmptyStrings =false,ErrorMessage ="Contact Number Is Required")]
        [MinLength(10,ErrorMessage ="Please Enter A Valid Contact Number")]
        public string ContactNumber { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Contact Person Is Required")]
        public string ContactPerson { get; set; }
        public string MobileNumber { get; set; }
        public string BEEStatusLevel { get; set; }
        public string Address { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email Address Is Required")]
        [EmailAddress(ErrorMessage ="Please Enter A Valid Email Address")]
        public string EmailAddress { get; set; }
		public string CertificateUploadId { get; set; }
	}
}
