using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;

namespace Ramp.Contracts.CommandParameter.Certificate
{
    public class RemoveCertificateUploadCommand: IdentityModel<string>
    {
        [Required]
        public string UploadId { get; set; }
    }
}
