using Microsoft.Practices.ServiceLocation;

namespace Common.Query
{
    public class QueryExecutor : IQueryExecutor
    {
        #region IQueryExecutor Members

        public TResult Execute<TParameters, TResult>(TParameters parameters)
            where TParameters : class
        {
            var queryHandler = ServiceLocator.Current.GetInstance<IQueryHandler<TParameters, TResult>>();
            return queryHandler.ExecuteQuery(parameters);
        }

        #endregion
    }
}