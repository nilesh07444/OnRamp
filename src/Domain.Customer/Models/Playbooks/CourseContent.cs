namespace Domain.Customer.Models
{
    public class CourseContent : Base.CustomerDomainObject
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string FileType { get; set; }
        public string FileName { get; set; }
        public string FileNameDemo { get; set; }
    }

}
