using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class CategoryViewModelLong : IViewModel
    {
        public CategoryViewModelLong()
        {
            CategoryViewModelList = new List<CategoryViewModel>();
        }

        public List<CategoryViewModel> CategoryViewModelList { get; set; }
        public CategoryViewModel CategorieViewModel { get; set; }
    }
}