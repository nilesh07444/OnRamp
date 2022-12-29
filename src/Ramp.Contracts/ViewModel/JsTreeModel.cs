using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class JsTreeModel
    {
        public JsTreeModel()
        {
            children = new List<JsTreeModel>();
            attr = new JsTreeAttribute();
        }
        public string data;
        public Guid id;
        public JsTreeState state;
        public string text
        {
            get
            {
                return data;
            }
        }
        public JsTreeAttribute attr;

        public List<JsTreeModel> children;
    }

    public class JsTreeAttribute
    {
        public Guid id;
        public bool selected;
    } 

    public class JsTreeState
    {
        public bool opened;
        public bool selected;
    }
}
