namespace Common.Query
{
    public interface IQueryHandler<in TQuery, out TViewModel>
    {
        /// <summary>
        ///     Retrieve a query result from a query
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns>Retrieve Query Result</returns>
        TViewModel ExecuteQuery(TQuery query);
    }
}