namespace Ramp.Contracts.Query.Upload
{
    public class FetchUploadFromCompanyQuery
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public bool ExcludeBytes { get; set; }
    }
}