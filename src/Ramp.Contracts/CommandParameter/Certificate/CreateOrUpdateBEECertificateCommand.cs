using Common.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;
namespace Ramp.Contracts.CommandParameter.Certificate
{
    public class CreateOrUpdateBEECertificateCommand : IdentityModel<string>
    {
        [Required]
        public string ExternalTrainingProviderId { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public FileUploadResultViewModel Upload { get; set; }
    }
}
