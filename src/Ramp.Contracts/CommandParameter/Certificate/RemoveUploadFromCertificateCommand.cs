using Common.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Certificate
{
    public class RemoveUploadFromCertificateCommand  : IdentityModel<string>
    {
        [Required]
        public string UploadId { get; set; }
    }
}
