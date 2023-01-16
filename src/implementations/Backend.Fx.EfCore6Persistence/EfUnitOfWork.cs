using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Domain;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.EfCore6Persistence;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly ILogger _logger = Log.Create<EfUnitOfWork>();
    private readonly DbContext _dbContext;

    public EfUnitOfWork(DbContext dbContext)
    {
        _dbContext = dbContext;
        
        _logger.LogInformation("Disabling change tracking on {DbContextTypeName} instance", dbContext.GetType().Name);
        _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
    }
    
    public async Task RegisterNewAsync(IAggregateRoot aggregateRoot, CancellationToken cancellation)
    {
        await _dbContext.AddAsync(aggregateRoot, cancellation).ConfigureAwait(false);
    }

    public Task RegisterDirtyAsync(IAggregateRoot aggregateRoot, CancellationToken cancellation)
    {
        GetEntityEntry(aggregateRoot).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task RegisterDeletedAsync(IAggregateRoot aggregateRoot, CancellationToken cancellation)
    {
        GetEntityEntry(aggregateRoot).State = EntityState.Deleted;
        return Task.CompletedTask;
    }

    public Task RegisterDirtyAsync(IAggregateRoot[] aggregateRoots, CancellationToken cancellationToken)
    {
        foreach (var aggregateRoot in aggregateRoots)
        {
            GetEntityEntry(aggregateRoot).State = EntityState.Modified;
        }
        
        return Task.CompletedTask;
    }

    private EntityEntry GetEntityEntry(IAggregateRoot aggregateRoot)
    {
        return _dbContext.ChangeTracker.Entries().SingleOrDefault(entry => entry.Entity == aggregateRoot)
               ?? throw new InvalidOperationException($"The {aggregateRoot} is not tracked by this context");
    }
}