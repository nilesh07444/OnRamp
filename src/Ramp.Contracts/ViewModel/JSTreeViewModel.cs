using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class JSTreeViewModel
    {
        public string id { get; set; }
        public int order { get; set; }
       public string parent { get; set; }
        public string text { get; set; }
        public string type { get; set; }
		public bool isParentNode { get; set; } = false;
        //Changes made by Pawan
        public string url { get; set; }
    }
}