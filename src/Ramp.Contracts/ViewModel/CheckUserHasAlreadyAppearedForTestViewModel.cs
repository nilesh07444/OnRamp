namespace Ramp.Contracts.ViewModel{
    public class CheckUserHasAlreadyAppearedForTestViewModel : IViewModel
    {
        public bool IsUserEligibleToTakeTest { get; set; }
        public string Message { get; set; }
    }
}