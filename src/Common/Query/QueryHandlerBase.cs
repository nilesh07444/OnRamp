namespace Common.Query
{
    public abstract class QueryHandlerBase<TQuery, TViewModel> : /*RepositoryProviderBase,*/IQueryHandler<TQuery, TViewModel>
    {
        //public ICurrentUserIdentity CurrentUser { get; set; }

    /*    protected QueryHandlerBase(IRepository<object> repository)
            : base(repository)
        {

        }

        protected QueryHandlerBase()
        {

        }*/
        public abstract TViewModel ExecuteQuery(TQuery queryParameters);
    }
}