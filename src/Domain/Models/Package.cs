namespace Domain.Models
{
    public class Package : DomainObject
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int MaxNumberOfGuides { get; set; }
        public int MaxNumberOfChaptersPerGuide { get; set; }
        public bool IsForSelfProvision { get; set; }

    }
}
