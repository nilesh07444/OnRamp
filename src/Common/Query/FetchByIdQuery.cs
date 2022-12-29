using Common.Data;
using System;

namespace Common.Query
{
    public class FetchByIdQuery<TEntity> : IdentityModel<string>
    {
    }
    public class FetchByIdQuery
    {
        public object Id { get; set; }

        public string userId { get; set; }
    }
    public class FetchByCustomIdQuery
    {
        public object Id { get; set; }
        public string chapterId { get; set; }
        public string userId { get; set; }
    }

    public class FetchByNameQuery
    {
        public string Name { get; set; }
    }
    public class FetchAllQuery
    {
        public object Id { get; set; }
        public string SearchText { get; set; }
        public string Filters { get; set; }
    }
    public class FetchUserQuery
    {
        public object Id { get; set; }
        public object UserId { get; set; }
    }
    public class FetchByCategoryIdQuery
    {
        public object Id { get; set; }
    }
    public class FetchByDocumentIdQuery
    {
        public object Id { get; set; }
    }
    public class FetchByCustomDocumentIdQuery
    {
        public object Id { get; set; }
    }
    public class FetchAllRecordsQuery { }

    public class FetchAllScheduleReportQuery { }

    //public class FetchAllAutoWorkflowQuery {
    //	public Guid Id { get; set; }
    //	public string SearchText { get; set; }
    //	public string Filters { get; set; }
    //}
}
