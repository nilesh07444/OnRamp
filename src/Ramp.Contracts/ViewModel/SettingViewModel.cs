using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ramp.Contracts.ViewModel
{
    public class SettingViewModel
    {
        public SettingViewModel()
        {
            SettingViewModelList = new List<SettingViewModelShort>();
        }

        public List<SettingViewModelShort> SettingViewModelList { get; set; }

        public SettingViewModelShort SettingViewModelShort { get; set; }
    }

    public class SettingViewModelShort
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Please enter number of days")]
        [RegularExpression("^[0-9]*", ErrorMessage = "Please enter numbers only")]
        public int PasswordPolicy { get; set; }
    }
}
