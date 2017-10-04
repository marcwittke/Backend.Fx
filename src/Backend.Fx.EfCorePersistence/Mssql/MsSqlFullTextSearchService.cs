namespace Backend.Fx.EfCorePersistence.Mssql
{
    using System.Linq;
    using BuildingBlocks;
    using EfCorePersistence;
    using Microsoft.EntityFrameworkCore;
    using Patterns.Authorization;

    public class MsSqlFullTextSearchService<TAggregateRoot> : IFullTextSearchService<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly EasyFts easyFts = new EasyFts();
        private readonly DbContext dbContext;
        private readonly IAggregateRootMapping<TAggregateRoot> aggregateRootMapping;
        private readonly IAggregateAuthorization<TAggregateRoot> aggregateAuthorization;
        private readonly string schema;
        private readonly string table;
        
        public MsSqlFullTextSearchService(
            DbContext dbContext, 
            IAggregateRootMapping<TAggregateRoot> aggregateRootMapping, 
            IAggregateAuthorization<TAggregateRoot> aggregateAuthorization)
        {
            this.dbContext = dbContext;
            this.aggregateRootMapping = aggregateRootMapping;
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
                queryable = aggregateAuthorization.Filter(dbContext.Set<TAggregateRoot>().FromSql(sql));
            }

            foreach (var includeDefinition in aggregateRootMapping.IncludeDefinitions)
            {
                queryable = queryable.Include(includeDefinition);
            }

            return queryable;
        }
    }
}