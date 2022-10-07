using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
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

    public async Task BeginAsync(IServiceScope scope)
    {
        await _operation.BeginAsync(scope).ConfigureAwait(false);
        await _dbContext.Database.UseTransactionAsync((DbTransaction)_dbTransactionHolder.Current).ConfigureAwait(false);
    }

    public Task CompleteAsync()
    {
        return _operation.CompleteAsync();
    }

    public Task CancelAsync()
    {
        return _operation.CancelAsync();
    }
}