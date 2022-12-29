namespace Ramp.Contracts.ViewModel
{
    public class UserLoginFrequencyReportViewModel : IViewModel
    {
        //public UserViewModel User { get; set; }
        public string UserName { get; set; }
        public int UserLoginCount { get; set; }
        public double LoginFrequency { get; set; }
    }
}