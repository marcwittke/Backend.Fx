namespace Backend.Fx.EfCorePersistence.Mssql
{
    using System.Linq;
    using BuildingBlocks;
    using EfCorePersistence;
    using Environment.MultiTenancy;
    using Microsoft.EntityFrameworkCore;
    using Patterns.Authorization;
    using Patterns.DependencyInjection;

    public class MsSqlFullTextSearchService<TAggregateRoot> : IFullTextSearchService<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly EasyFts _easyFts = new EasyFts();
        private readonly DbContext _dbContext;
        private readonly ICurrentTHolder<TenantId> _currentTenantholder;
        private readonly IAggregateMapping<TAggregateRoot> _aggregateMapping;
        private readonly IAggregateAuthorization<TAggregateRoot> _aggregateAuthorization;
        private readonly string _schema;
        private readonly string _table;

        public MsSqlFullTextSearchService(
            DbContext dbContext,
            ICurrentTHolder<TenantId> currentTenantholder,
            IAggregateMapping<TAggregateRoot> aggregateMapping,
            IAggregateAuthorization<TAggregateRoot> aggregateAuthorization)
        {
            _dbContext = dbContext;
            _currentTenantholder = currentTenantholder;
            _aggregateMapping = aggregateMapping;
            _aggregateAuthorization = aggregateAuthorization;

            var entityTypeRelational = dbContext.Model.FindEntityType(typeof(TAggregateRoot)).Relational();
            _schema = entityTypeRelational.Schema ?? "dbo";
            _table = entityTypeRelational.TableName;
        }

        public IQueryable<TAggregateRoot> Search(string searchQuery)
        {
            // mitigate SQL injection
            searchQuery = searchQuery.Replace("\'", "");

            string ftsQuery = _easyFts.ToFtsQuery(searchQuery);

            IQueryable<TAggregateRoot> queryable;

            if (string.IsNullOrEmpty(ftsQuery))
            {
                queryable = _aggregateAuthorization.Filter(_dbContext.Set<TAggregateRoot>());
            }
            else
            {
                var sql = $"SELECT * FROM [{_schema}].[{_table}] WHERE Contains ({_table}.*, '{ftsQuery}')";
#pragma warning disable EF1000 // Possible SQL injection vulnerability.
                queryable = _aggregateAuthorization.Filter(_dbContext.Set<TAggregateRoot>().FromSql(sql));
#pragma warning restore EF1000 // Possible SQL injection vulnerability.
            }

            queryable = _currentTenantholder.Current.HasValue
                                                           ? queryable.Where(agg => agg.TenantId == _currentTenantholder.Current.Value)
                                                           : queryable.Where(agg => false);

            foreach (var includeDefinition in _aggregateMapping.IncludeDefinitions)
            {
                queryable = queryable.Include(includeDefinition);
            }

            return queryable;
        }
    }
}