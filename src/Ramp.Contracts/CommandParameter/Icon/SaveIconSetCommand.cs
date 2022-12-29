using Common.Command;
using Domain.Models;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Icon
{
    public class SaveIconSetCommand:ICommand
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<IconModel> Icons { get; set; } = new List<IconModel>();
    }
}
