using Common.Command;
using Domain.Customer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Settings
{
    public class CreatedUpdateCustomConfigurationCommandParameter : ICommand
    {
        public string CertificatePath { get; set; }
        public string CSSPath { get; set; }
        public bool Seach { get; set; }
        public FileUploads Cert { get; set; }
        public FileUploads CSS { get; set; }
    }
}