using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class BundleViewModel
    {
        public BundleViewModel()
        {
            BundleViewModelList = new List<BundleViewModelShort>();
        }

        public IEnumerable<BundleViewModelShort> BundleViewModelList { get; set; }

        public BundleViewModelShort BundleViewModelShort { get; set; }
    }

    public class BundleViewModelShort
    {
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please enter Max Number of Documents")]
        [RegularExpression("^[0-9]*", ErrorMessage = "Please enter numbers only")]
        public int MaxNumberOfDocuments { get; set; }

        public bool IsForSelfProvision { get; set; }
    }
}
