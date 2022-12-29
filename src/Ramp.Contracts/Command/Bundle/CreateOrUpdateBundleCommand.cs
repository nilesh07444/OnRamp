using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Command.Bundle
{
    public class CreateOrUpdateBundleCommand
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int MaxNumberOfDocuments { get; set; }
        public bool IsForSelfProvision { get; set; }
    }
}
