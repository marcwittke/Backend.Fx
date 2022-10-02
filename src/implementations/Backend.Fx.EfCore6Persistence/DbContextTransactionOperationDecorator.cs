using System.Data;
using System.Data.Common;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.EfCore6Persistence;

public class DbContextTransactionOperationDecorator : IOperation
{
    private readonly DbContext _dbContext;
    private readonly ICurrentTHolder<IDbTransaction> _dbTransactionHolder;
    private readonly IOperation _operation;

    public DbContextTransactionOperationDecorator(
        DbContext dbContext,
        ICurrentTHolder<IDbTransaction> dbTransactionHolder,
        IOperation operation)
    {
        _dbContext = dbContext;
        _dbTransactionHolder = dbTransactionHolder;
        _operation = operation;
    }

    public void Begin(IServiceScope scope)
    {
        _operation.Begin(scope);
        _dbContext.Database.UseTransaction((DbTransaction)_dbTransactionHolder.Current);
    }

    public void Complete()
    {
        _operation.Complete();
    }

    public void Cancel()
    {
        _operation.Cancel();
    }
}