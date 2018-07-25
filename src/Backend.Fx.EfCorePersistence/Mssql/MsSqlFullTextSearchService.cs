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
        private readonly EasyFts easyFts = new EasyFts();
        private readonly DbContext dbContext;
        private readonly ICurrentTHolder<TenantId> currentTenantholder;
        private readonly IAggregateMapping<TAggregateRoot> aggregateMapping;
        private readonly IAggregateAuthorization<TAggregateRoot> aggregateAuthorization;
        private readonly string schema;
        private readonly string table;

        public MsSqlFullTextSearchService(
            DbContext dbContext,
            ICurrentTHolder<TenantId> currentTenantholder,
            IAggregateMapping<TAggregateRoot> aggregateMapping,
            IAggregateAuthorization<TAggregateRoot> aggregateAuthorization)
        {
            this.dbContext = dbContext;
            this.currentTenantholder = currentTenantholder;
            this.aggregateMapping = aggregateMapping;
            this.aggregateAuthorization = aggregateAuthorization;

            var entityTypeRelational = dbContext.Model.FindEntityType(typeof(TAggregateRoot)).Relational();
            schema = entityTypeRelational.Schema ?? "dbo";
            table = entityTypeRelational.TableName;
        }

        public IQueryable<TAggregateRoot> Search(string searchQuery)
        {
            string ftsQuery = easyFts.ToFtsQuery(searchQuery);

            IQueryable<TAggregateRoot> queryable;

            if (string.IsNullOrEmpty(ftsQuery))
            {
                queryable = aggregateAuthorization.Filter(dbContext.Set<TAggregateRoot>());
            }
            else
            {
                var sql = $"SELECT * FROM [{schema}].[{table}] WHERE Contains ({table}.*, '{ftsQuery}')";
#pragma warning disable EF1000 // Possible SQL injection vulnerability.
                queryable = aggregateAuthorization.Filter(dbContext.Set<TAggregateRoot>().FromSql(sql));
#pragma warning restore EF1000 // Possible SQL injection vulnerability.
            }

            queryable = currentTenantholder.Current.HasValue
                                                           ? queryable.Where(agg => agg.TenantId == currentTenantholder.Current.Value)
                                                           : queryable.Where(agg => false);

            foreach (var includeDefinition in aggregateMapping.IncludeDefinitions)
            {
                queryable = queryable.Include(includeDefinition);
            }

            return queryable;
        }
    }
}