using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtuaCon;

namespace Ramp.Contracts.ViewModel
{
    public class KnockoutModel<TViewModel>
    {
        public object Data { get; set; }
        public  Type Type { get; set; }
        public IDictionary<string, string> Links { get; set; } = new Dictionary<string, string>();
        public string Mode { get; set; }
    }
    public enum Mode
    {
        [EnumFriendlyName("Create")]
        Create=0,
        [EnumFriendlyName("Edit")]
        Edit =1,
        [EnumFriendlyName("List")]
        List =2
    }
}
