namespace Domain.Customer.Models.Document
{
    public interface IContentBox : IContentUploads
    {
        string Title { get; set; }
        int Number { get; set; }
        string Content { get; set; }
        bool Deleted { get; set; }
    }
}
