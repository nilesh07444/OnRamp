using Common.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel
{
    public class CategoryViewModel 
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Playbook Category Title is required.")]
        [Remote("DoesCategoryNameAlreadyPresent", "Categories", "Categories", ErrorMessage = "Playbook Category name already exist")]
        public string CategorieTitle { get; set; }

        [Required(ErrorMessage = "Playbook Category Description is required.")]
        public string Description { get; set; }

        public Guid? ParentCategoryId { get; set; } 

        public IEnumerable<SerializableSelectListItem> Categories { get; set; }
    }

    public class CategoryViewModelShort : IdentityModel<string>
    {
        public string Name { get; set; }
    }
}