namespace Common.Query
{
   public interface IPagedQuery : IQuery
    {
        int Page { get; set; }
        int? PageSize { get; set; }
		bool? EnableChecklistDocument { get; set; } 
	}
}
