using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public enum ViewMode
    {
        Create = 0,
        Edit = 1,
        Preview = 2
    }
    public enum ActionMode
    {
        
    }
    public enum FunctionalMode
    {
        Playbooks, Tests
    }
    public enum AssignMode
    {
        Users,Groups
    }
}
