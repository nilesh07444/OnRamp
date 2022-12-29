using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel
{
   public  class IconSetModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<IconModel> Icons { get; set; } = new List<IconModel>();
        public IEnumerable<SelectListItem> AvailableIcons { get; set; } = new List<SelectListItem>();
        public bool Master { get; set; }
    }
}
