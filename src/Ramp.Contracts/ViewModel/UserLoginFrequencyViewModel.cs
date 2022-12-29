using System;

namespace Ramp.Contracts.ViewModel
{
    public class UserLoginFrequencyViewModel : IViewModel
    {
        public double TotalLoginPercentageOfSystem { get; set; }
        public long AllLoginsForSystem { get; set; }
        public double NumberOfDaysInTimeSpan { get; set; }
        public string CompanyName { set; get; }
    }
}