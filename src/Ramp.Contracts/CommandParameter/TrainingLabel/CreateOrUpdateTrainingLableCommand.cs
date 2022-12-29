using Common.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Ramp.Contracts.CommandParameter.TrainingLabel
{
    public class CreateOrUpdateTrainingLableCommand : IdentityModel<string>
    {
        [Required(AllowEmptyStrings = false,ErrorMessage = "Please provide a Name for the Label")]
        public string Name { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide a Description for the Label")]
        public string Description { get; set; }
    }
}
