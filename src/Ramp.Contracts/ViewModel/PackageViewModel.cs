using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ramp.Contracts.ViewModel
{
    // TODO: Delete when completely unused
    public class PackageViewModel
    {
        public PackageViewModel()
        {
            PackageViewModelList = new List<PackageViewModelShort>();
        }

        public List<PackageViewModelShort> PackageViewModelList { get; set; }

        public PackageViewModelShort PackageViewModelShort { get; set; }
    }

    public class PackageViewModelShort
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please enter Max Number Of Guides")]
        [RegularExpression("^[0-9]*", ErrorMessage = "Please enter numbers only")]
        public int MaxNumberOfGuides { get; set; }

        [Required(ErrorMessage = "Please enter Max Number Of Chapters Per Guide")]
        [RegularExpression("^[0-9]*", ErrorMessage = "Please enter numbers only")]
        public int MaxNumberOfChaptersPerGuide { get; set; }

        public bool IsForSelfProvision { get; set; }
    }
}