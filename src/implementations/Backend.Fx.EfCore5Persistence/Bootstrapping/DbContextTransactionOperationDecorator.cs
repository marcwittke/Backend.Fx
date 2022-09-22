using System.Data;
using System.Data.Common;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Extensions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore5Persistence.Bootstrapping
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