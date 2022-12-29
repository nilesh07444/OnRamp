using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class AutoCompleteModel
    {
        public string Control { get; set; }
        public AutoCompleteSection[] Sections { get; set; }
        public bool Typeahead { get; set; }
        public bool Tags { get; set; }
        public AutoCompleteModel(string control,params AutoCompleteSection[] sections)
        {
            Control = control;
            Sections = sections;
        }

    }
    public class AutoCompleteSection
    {
        public string Name { get; set; }
        public string Action { get; set; }

        public static AutoCompleteSection AllUsers = new AutoCompleteSection { Action = "Users?AllCompanyUsers=true", Name = "Users"};
        public static AutoCompleteSection AllExternalTrainingProviders = new AutoCompleteSection { Action = "ExternalTrainingProviders", Name = "External Training Providers"};
        public static AutoCompleteSection TrainingLabels = new AutoCompleteSection { Action = "TrainingLabels", Name = "Training Labels" };
        public static AutoCompleteSection Labels = new AutoCompleteSection { Action = "Labels", Name = "Labels" };
        public static AutoCompleteSection BursaryType = new AutoCompleteSection { Action = "BursaryType", Name = "Bursary Type" };
        public static AutoCompleteSection Groups = new AutoCompleteSection { Action = "Groups", Name = "Groups" };
        public static AutoCompleteSection Races = new AutoCompleteSection { Action = "Races", Name = "Races" };
    }
}
