using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
   public class IconModel
    {
        public string Id { get; set; }
        public FileUploadViewModel UploadModel { get; set; }
        public IconType IconType { get; set; }
        public string Description { get; set; }
    }
}
