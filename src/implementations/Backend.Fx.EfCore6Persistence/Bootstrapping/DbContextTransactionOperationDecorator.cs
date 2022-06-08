using System.Data;
using System.Data.Common;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public class DbContextTransactionOperationDecorator : DbTransactionOperationDecorator
    {
        private readonly DbContext _dbContext;
        
        public DbContextTransactionOperationDecorator(DbContext dbContext, IDbConnection dbConnection, IOperation operation) 
            : base(dbConnection, operation)
        {
            _dbContext = dbContext;
        }

        public override void Begin()
        {
            base.Begin();
            _dbContext.Database.UseTransaction((DbTransaction) CurrentTransaction);
        }
    }
}