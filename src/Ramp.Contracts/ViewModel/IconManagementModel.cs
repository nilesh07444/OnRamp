using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class IconManagementModel
    {
        public IList<IconSetModel> IconSets { get; set; } = new List<IconSetModel>();
    }
}
