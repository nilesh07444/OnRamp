using System;
using Common.Command;

namespace Ramp.Contracts.CommandParameter.PackageManagement
{
    public class PackageCommandParameter : ICommand
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int MaxNumberOfGuides { get; set; }
        public int MaxNumberOfChaptersPerGuide { get; set; } 
        public bool IsForSelfProvision { get; set; }

    }
}