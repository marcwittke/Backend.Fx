namespace Backend.Fx.EfCorePersistence.Mssql
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using BuildingBlocks;
    using Microsoft.EntityFrameworkCore;

    public abstract class MsSqlFullTextSearchIndex<TAggregateRoot> : IFullTextSearchIndex where TAggregateRoot : AggregateRoot
    {
        private const string FullTextCatalogName = "FullTextSearch";
        private readonly string mssqlLocaleId = "1031"; // TODO: still hard coded to Germany

        public abstract Expression<Func<TAggregateRoot, object>>[] IndexedProperties { get; }

        public void EnsureIndex(DbContext dbContext)
        {
            var entityType = dbContext.Model.FindEntityType(typeof(TAggregateRoot));
            var relationalEntityTypeAnnotations = entityType.Relational();
            string schema = relationalEntityTypeAnnotations.Schema ?? "dbo";
            string table = relationalEntityTypeAnnotations.TableName;

            IEnumerable<string> indexedColumnDefinitions = IndexedProperties
                    .Select(prop => entityType.FindProperty(GetMemberName(prop)).Relational().ColumnName)
                    .Select(col => $"{col} Language {mssqlLocaleId}");

            //var primaryKey = entityType.FindPrimaryKey();
            //var mutableIndex = entityType.FindIndex(primaryKey.Properties);
            //string primaryKeyIndexName = mutableIndex.Relational().Name;
            string primaryKeyIndexName = $"PK_{table}";

            string indexedColumnsSqlFragment = string.Join(", ", indexedColumnDefinitions);

            dbContext.Database.ExecuteSqlCommand(
                    $"IF ((SELECT count(*) FROM sys.fulltext_catalogs WHERE name = '{FullTextCatalogName}') = 0) CREATE FULLTEXT CATALOG " + FullTextCatalogName);

            dbContext.Database.ExecuteSqlCommand(
                    $"IF ((SELECT count(*) FROM sys.fulltext_indexes i INNER JOIN sys.tables t ON t.object_id = i.object_id WHERE t.name = '{table}') = 0) " +
                    " CREATE FULLTEXT INDEX ON ["+schema+"].["+table+"] ("+indexedColumnsSqlFragment+") KEY INDEX "+primaryKeyIndexName+" ON "+FullTextCatalogName);
        }

        private static string GetMemberName(Expression<Func<TAggregateRoot, object>> exp)
        {
            if (!(exp.Body is MemberExpression body))
            {
                UnaryExpression ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            Debug.Assert(body != null, nameof(body) + " != null");
            return body.Member.Name;
        }
    }

}
