namespace Common.Query
{
    public interface IQueryExecutor
    {
        TResult Execute<TParameters, TResult>(TParameters parameters)
            where TParameters : class;
    }
}