namespace Ramp.Contracts.ViewModel
{
    public class TrainingLabelListModel : Common.Data.IdentityModel<string>
    {
        public string Name { get; set; }
        public string Description { get; set; }
	}
    public class TrainingLabelModel : TrainingLabelListModel
    {

    }
}
